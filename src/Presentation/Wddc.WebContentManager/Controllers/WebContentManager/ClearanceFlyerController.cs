using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using Wddc.WebContentManager.Services.Logging;
using Wddc.WebContentManager.Services.WebContent.Sales.ClearanceFlyer;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    /// <summary>
    /// Clearance Flyers (2026-10-27): the PDF flyer of each website clearance section - Retail Clearance, Equipment
    /// Clearance, Fire Sale. Bucky sets each flyer's look once (cover picture, title, month, colour, notes); the items
    /// come from the live clearance list (the Apps API) with their website pictures. Preview builds the PDF; Publish
    /// puts it where the website's flyer banner opens it (CS/Promos/&lt;FileName&gt;.pdf), keeping the one it replaces
    /// in "Flyer backups".
    /// </summary>
    public class ClearanceFlyerController : BaseController
    {
        private const int MaxCoverBytes = 8_000_000;
        private const int CoverPixels = 1700;    // fills a letter page at ~200 dpi
        private const int PicturePixels = 360;   // a product picture is under an inch wide on the page

        private readonly IClearanceFlyerService _flyers;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly string _promosFolder;
        private readonly string _picturesFolder;

        public ClearanceFlyerController(IClearanceFlyerService flyers, Wddc.WebContentManager.Services.Logging.ILogger logger, IConfiguration configuration)
        {
            _flyers = flyers;
            _logger = logger;
            // Where the website's flyers are and its product pictures (ClearanceFlyer in appsettings; the dev
            // settings point the flyers at a local folder, so a test never replaces the live ones).
            _promosFolder = configuration?["ClearanceFlyer:PromosFolder"] ?? @"\\WEBSRVR\WDDCWebPages\wddc_members\CS\Promos";
            _picturesFolder = configuration?["ClearanceFlyer:PicturesFolder"] ?? @"\\WEBSRVR\WDDCWebPages\wddc_members\images\ProductPics";
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>Every flyer, with how many items it would have now.</summary>
        [HttpGet]
        public async Task<JsonResult> FlyersAsync()
        {
            try
            {
                return FormattedJson(await _flyers.GetFlyersAsync(), true);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not load the clearance flyers");
                return FormattedJson(null, false, "The flyers could not be loaded.");
            }
        }

        /// <summary>A flyer's cover picture, for the page.</summary>
        [HttpGet]
        public async Task<IActionResult> CoverAsync(int id)
        {
            var cover = await _flyers.GetCoverAsync(id);
            return cover?.Content is { Length: > 0 } ? File(cover.Content, cover.ContentType ?? "image/jpeg") : NotFound();
        }

        /// <summary>
        /// Saves a flyer's look. A cover picture (JPG or PNG, up to 8 MB) is made letter-page size first; removeCover
        /// takes the cover away.
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> SaveAsync(int id, string title, string subtitle, string intro, string note, string footer, string accentColor,
            IFormFile cover, bool removeCover)
        {
            byte[] coverBytes = null;
            string coverType = null;
            if (cover != null && cover.Length > 0)
            {
                if (cover.Length > MaxCoverBytes)
                {
                    return FormattedJson(null, false, "The cover picture can be up to 8 MB.");
                }

                var extension = Path.GetExtension(cover.FileName ?? string.Empty).ToLowerInvariant();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    return FormattedJson(null, false, "The cover picture must be a JPG or PNG.");
                }

                using var stream = new MemoryStream();
                await cover.CopyToAsync(stream);
                var fitted = ClearanceFlyerPictures.Fit(stream.ToArray(), CoverPixels, 88);
                if (fitted == null)
                {
                    return FormattedJson(null, false, "That file is not a picture that can be read.");
                }

                coverBytes = fitted.Bytes;
                coverType = IsPng(fitted.Bytes) ? "image/png" : "image/jpeg";
            }

            try
            {
                await _flyers.SaveAsync(id, title, subtitle, intro, note, footer, accentColor, coverBytes, coverType, removeCover, UserName());
                _logger.Information($"Saved clearance flyer {id} ({title}){(coverBytes != null ? " with a new cover picture" : removeCover ? " without a cover picture" : "")}",
                    null, User, "ClearanceFlyer");
                return FormattedJson(null, true);
            }
            catch (ArgumentException ex) when (ex.Message.StartsWith("BadRequest: ", StringComparison.Ordinal))
            {
                // The API's own reason (no title, not a colour...).
                return FormattedJson(null, false, ex.Message.Substring("BadRequest: ".Length).Trim().Trim('"'));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not save clearance flyer {ID}", id);
                return FormattedJson(null, false, "The flyer could not be saved.");
            }
        }

        /// <summary>The flyer as it would be published now, to look at (opens in the browser).</summary>
        [HttpGet]
        public async Task<IActionResult> PreviewAsync(int id)
        {
            try
            {
                var (flyer, result) = await BuildAsync(id);
                if (flyer == null)
                {
                    return NotFound();
                }

                Response.Headers["Content-Disposition"] = $"inline; filename=\"{flyer.FileName} (preview).pdf\"";
                return File(result.Pdf, "application/pdf");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not build the preview of clearance flyer {ID}", id);
                return Content("The flyer could not be built: " + ex.Message);
            }
        }

        /// <summary>
        /// Builds the flyer and puts it on the website as CS/Promos/&lt;FileName&gt;.pdf - the file its banner opens.
        /// The file it replaces goes to CS/Promos/Flyer backups first.
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> PublishAsync(int id)
        {
            try
            {
                var (flyer, result) = await BuildAsync(id);
                if (flyer == null)
                {
                    return FormattedJson(null, false, "That flyer is not there - reload the page.");
                }

                var target = Path.Combine(_promosFolder, flyer.FileName + ".pdf");
                string backup = null;
                if (System.IO.File.Exists(target))
                {
                    var backups = Path.Combine(_promosFolder, "Flyer backups");
                    Directory.CreateDirectory(backups);
                    backup = Path.Combine(backups, $"{flyer.FileName} {DateTime.Now:yyyy-MM-dd HHmmss}.pdf");
                    System.IO.File.Copy(target, backup, false);
                }

                // Written beside it first, then swapped in, so the website never serves half a file.
                var temp = target + ".new";
                await System.IO.File.WriteAllBytesAsync(temp, result.Pdf);
                System.IO.File.Move(temp, target, true);

                await _flyers.PublishedAsync(id, result.Items, UserName());
                _logger.Information($"Published clearance flyer {flyer.FileName}: {result.Items} items, {result.Pages} pages{(backup != null ? $" (the old one kept as {Path.GetFileName(backup)})" : "")}",
                    null, User, "ClearanceFlyer");
                return FormattedJson(new { result.Items, result.Pages, Size = result.Pdf.Length, File = flyer.FileName + ".pdf", Backup = backup != null ? Path.GetFileName(backup) : null }, true);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not publish clearance flyer {ID}", id);
                return FormattedJson(null, false, "The flyer could not be put on the website: " + ex.Message);
            }
        }

        // The flyer, its items as they are now, their pictures (made small, loaded side by side), and the PDF.
        private async Task<(ClearanceFlyerDto Flyer, ClearanceFlyerPdf.Result Result)> BuildAsync(int id)
        {
            var flyer = (await _flyers.GetFlyersAsync()).FirstOrDefault(f => f.ID == id);
            if (flyer == null)
            {
                return (null, null);
            }

            var items = await _flyers.GetItemsAsync(flyer.CategoryID);
            var coverDto = flyer.HasCover ? await _flyers.GetCoverAsync(id) : null;
            var cover = coverDto?.Content is { Length: > 0 } ? ClearanceFlyerPictures.Fit(coverDto.Content, CoverPixels, 88) : null;

            var pictures = new ConcurrentDictionary<string, ClearanceFlyerPictures.Picture>(StringComparer.OrdinalIgnoreCase);
            await Task.Run(() => Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = 6 }, item =>
            {
                try
                {
                    var file = Path.Combine(_picturesFolder, (item.ItemNumber ?? string.Empty).Trim() + "_01.jpg");
                    if (System.IO.File.Exists(file))
                    {
                        var picture = ClearanceFlyerPictures.Fit(System.IO.File.ReadAllBytes(file), PicturePixels);
                        if (picture != null)
                        {
                            pictures[item.ItemNumber.Trim()] = picture;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // One picture that can't be read leaves an empty box, not a failed flyer.
                    Log.Logger.Warning(ex, "Clearance flyer picture of {Item} could not be read", item.ItemNumber);
                }
            }));

            return (flyer, ClearanceFlyerPdf.Build(flyer, cover, items, pictures, DateTime.Now));
        }

        private static bool IsPng(byte[] bytes) => bytes.Length > 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;

        // "WDDCHO\festuka" -> "festuka"
        private string UserName()
        {
            var name = User?.Identity?.Name ?? "unknown";
            var slash = name.LastIndexOf('\\');
            return (slash >= 0 ? name.Substring(slash + 1) : name).ToLowerInvariant();
        }
    }
}
