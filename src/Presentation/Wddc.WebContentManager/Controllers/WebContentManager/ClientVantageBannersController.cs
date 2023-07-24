using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Http;
using System.IO;
using PagedList;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using Wddc.WebContentManager.Services.WebContent.ClientVantageBanners;
using static System.Net.WebRequestMethods;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class ClientVantageBannersController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Services.Logging.ILogger _logger;
        private readonly IClientVantageBannersService _clientVantageBannersService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ClientVantageBannersController(IClientVantageBannersService clientVantageBannersService, Services.Logging.ILogger logger, IHostingEnvironment hostingEnvironment)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _clientVantageBannersService = clientVantageBannersService;
            _hostingEnvironment = hostingEnvironment;
        }

        public ActionResult Index(string response, string message)
        {
            ViewBag.response = response;
            ViewBag.Message = message;
            return View();
        }

        public Task<JsonResult> GetBannersCategoriesAsync()
        {
            string targetPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage";

            List<string> dirs = new List<string>();
            foreach (var d in System.IO.Directory.GetDirectories(targetPath))
            {
                var dir = new DirectoryInfo(d);
                var dirName = dir.Name;

                dirs.Add(dirName);
            }

            return Task.FromResult(Json(dirs));
        }

        public Task<JsonResult> GetBannersOfCategoryAsync(string category)
        {
            string sourceDir1 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + category + "\\small",
                   sourceDir2 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + category;
          
            string tempDir1 = Path.Combine(this._hostingEnvironment.WebRootPath, "ClientVantage_Banners_Temp\\" + category + "\\small"),
                   tempDir2 = Path.Combine(this._hostingEnvironment.WebRootPath, "ClientVantage_Banners_Temp\\" + category);

            if (Directory.Exists(Path.Combine(this._hostingEnvironment.WebRootPath, "ClientVantage_Banners_Temp")))
                Directory.Delete(Path.Combine(this._hostingEnvironment.WebRootPath, "ClientVantage_Banners_Temp"), true);

            Directory.CreateDirectory(tempDir1);

            var dir1 = new DirectoryInfo(sourceDir1);

            if (!dir1.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir1.FullName}");

            List<string> fileNames = new List<string>();

            foreach (FileInfo file1 in dir1.GetFiles())
            {
                FileInfo file2 = new FileInfo(sourceDir2 + "\\" + file1.Name.Trim());

                if (file2.Exists && file2.Extension != ".db")
                {
                    file1.CopyTo(Path.Combine(tempDir1, file1.Name.Trim()), true);
                    file2.CopyTo(Path.Combine(tempDir2, file2.Name.Trim()), true);
                    fileNames.Add(file1.Name);
                } 
            }

            return Task.FromResult(Json(fileNames));
        }


        [HttpPost]
        public async Task<ActionResult> UploadCVBannerAsync(string categoryToUpload, IFormFile imageUrl)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is uploading a new CV banner, category: " + categoryToUpload);

            if (imageUrl == null)
                return RedirectToAction("Index", new { response = "Failure", message = "No banner image was selected" });

            if (imageUrl.ContentType != "image/jpg" && imageUrl.ContentType != "image/jpeg" && imageUrl.ContentType != "image/pjpeg" &&
                imageUrl.ContentType != "image/gif" && imageUrl.ContentType != "image/x-png" && imageUrl.ContentType != "image/png" )
                return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be an image type (jpg, jpeg, pjpeg, gif, png)" });

            string imageName = Path.GetFileName(imageUrl.FileName);
            string imagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToUpload + "\\" + imageName;
            string thumbnailImagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToUpload + "\\small\\" + imageName;

            try
            {
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageUrl.CopyToAsync(stream);
                }

                using var image = Image.Load(imageUrl.OpenReadStream());
                image.Mutate(x => x.Resize(300, 75));
                image.Save(thumbnailImagePath);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error uploading ClientVantage banner, folder {categoryToUpload} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error uploading ClientVantage banner, category {categoryToUpload}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure uploading ClientVantage banner: " + ex.Message.Substring(0, 300) });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} uploaded ClientVantage banner, category {categoryToUpload} successfully");
            _logger.Information($"Uploaded banner to ClientVantage banners library, category {categoryToUpload}, successfully", null, User, "WebOrdering");
            return RedirectToAction("Index", new { response = "Success", message = "ClientVantage banner was uploaded successfully! " });
        }


        [HttpPost]
        public async Task<ActionResult> DeleteCVBannerAsync(string categoryToDelete, string bannerName)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is deleting CV banner " + bannerName + ", category: " + categoryToDelete);

            string imagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToDelete.Trim() + "\\" + bannerName.Trim();
            string thumbnailImagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToDelete.Trim() + "\\small\\" + bannerName.Trim();

            FileInfo imageFile = new FileInfo(imagePath);
            FileInfo thumbnailImageFile= new FileInfo(thumbnailImagePath);

            try
            {
                if (imageFile.Exists && thumbnailImageFile.Exists)
                {
                    imageFile.Delete();
                    thumbnailImageFile.Delete();
                }
                else
                {
                    Log.Logger.Error($"Error deleting ClientVantage banner, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: file not found");
                    _logger.Error($"Error deleting ClientVantage banner, category {categoryToDelete}: file not found", null, User, "WebOrdering");
                    return Json(new { success = false, message = "Failure deleting ClientVantage banner: file not found"});
                }
            }
            catch (IOException ex)
            {
                Log.Logger.Error($"Error deleting ClientVantage banner, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting ClientVantage banner, category {categoryToDelete}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return Json(new { success = false, message = "Failure deleting ClientVantage banner: " + ex.Message.Substring(0, 300) });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted ClientVantage banner, category {categoryToDelete} successfully");
            _logger.Information($"Deleted banner from ClientVantage banners library, category {categoryToDelete}, successfully", null, User, "WebOrdering");
            return Json(new { success = true, message = "ClientVantage banner was deleted successfully! "});
        }


        public async Task<ActionResult> LogAsync(int? pageNumber, int pageSize = 5, string referrerUrl = null)
        {
            IPagedList<WebOrderingLog> results = await _logger.GetAllWebOrderingLogsAsync(referrerUrl: referrerUrl, pageSize: 10, pageIndex: pageNumber ?? 1);

            var ajaxResult = new AjaxResults()
            {
                ResultStatus = ResultStatus.Success,
                Results = results
                    .Select(l => new { Text = l.ShortMessage.Length > 100 ? l.ShortMessage.Substring(0, 100) : l.ShortMessage, l.User, Created = l.CreatedOnUtc.ToLocalTime().ToString() }),
                TotalItemCount = results.TotalItemCount,
            };
            return Json(ajaxResult);
        }
    }
}
