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

        public ActionResult Index(string category)
        {
            var response = TempData["response"] as string;
            var message = TempData["message"] as string;

            ViewBag.response = response;
            ViewBag.Message = message;
            ViewBag.Category = category;
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
          
            string tempDir1 = Path.Combine(this._hostingEnvironment.WebRootPath, "img\\ClientVantage_Banners_Temp\\" + category + "\\small"),
                   tempDir2 = Path.Combine(this._hostingEnvironment.WebRootPath, "img\\ClientVantage_Banners_Temp\\" + category);

            if (Directory.Exists(Path.Combine(this._hostingEnvironment.WebRootPath, "img\\ClientVantage_Banners_Temp")))
                Directory.Delete(Path.Combine(this._hostingEnvironment.WebRootPath, "img\\ClientVantage_Banners_Temp"), true);

            Directory.CreateDirectory(tempDir1);

            var dir1 = new DirectoryInfo(sourceDir1);

            List<string> fileNames = new List<string>();

            if (dir1.Exists)

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

        public Task<JsonResult> GetMobileBannersOfCategoryAsync(string category)
        {
            string sourceDir1 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage Mobile\\" + category + "\\small",
                   sourceDir2 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage Mobile\\" + category;

            string tempDir1 = Path.Combine(this._hostingEnvironment.WebRootPath, "img\\ClientVantage_Mobile_Banners_Temp\\" + category + "\\small"),
                   tempDir2 = Path.Combine(this._hostingEnvironment.WebRootPath, "img\\ClientVantage_Mobile_Banners_Temp\\" + category);

            if (Directory.Exists(Path.Combine(this._hostingEnvironment.WebRootPath, "img\\ClientVantage_Mobile_Banners_Temp")))
                Directory.Delete(Path.Combine(this._hostingEnvironment.WebRootPath, "img\\ClientVantage_Mobile_Banners_Temp"), true);

            Directory.CreateDirectory(tempDir1);

            var dir1 = new DirectoryInfo(sourceDir1);

            List<string> fileNames = new List<string>();

            if (dir1.Exists)

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
        public async Task<ActionResult> UploadCVBannerAsync(string categoryToUpload, IFormFile imageUrl, IFormFile imageMobileUrl)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is uploading a new CV banner, category: " + categoryToUpload);

            if (imageUrl == null && imageMobileUrl == null)
            {
                TempData["response"] = "Failure";
                TempData["message"] = "No banner image was selected";
                return RedirectToAction("Index");
            }
            else
            {
                if (imageUrl != null)
                {
                    if (Path.GetFileName(imageUrl.FileName).Contains('\'') || Path.GetFileName(imageUrl.FileName).Contains('&'))
                    {
                        TempData["response"] = "Failure";
                        TempData["message"] = "Banner name cannot contain special characters ('). Please rename the file and try again";
                        return RedirectToAction("Index");
                    }
    
                }

                if (imageMobileUrl != null)
                {
                    if (Path.GetFileName(imageMobileUrl.FileName).Contains('\'') || Path.GetFileName(imageUrl.FileName).Contains('&'))
                    {
                        TempData["response"] = "Failure";
                        TempData["message"] = "Mobile Banner name cannot contain special characters ('). Please rename the file and try again";
                        return RedirectToAction("Index");
                    }
                }
            }

            string bannerFileName = "", mobileBannerFileName = "";
            bannerFileName = imageUrl != null ? Path.GetFileName(imageUrl.FileName) : "";
            mobileBannerFileName = imageMobileUrl != null ? Path.GetFileName(imageMobileUrl.FileName) : "";

            if (imageUrl != null)
            {
                if (imageUrl.ContentType != "image/jpg" && imageUrl.ContentType != "image/jpeg" && imageUrl.ContentType != "image/pjpeg" &&
                imageUrl.ContentType != "image/gif" && imageUrl.ContentType != "image/x-png" && imageUrl.ContentType != "image/png")
                {
                    TempData["response"] = "Failure";
                    TempData["message"] = "File uploaded must be an image type (jpg, jpeg, pjpeg, gif, png)";
                    return RedirectToAction("Index");
                }

                string imageName = Path.GetFileName(imageUrl.FileName);
                string path = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToUpload;
                string thumbnailPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToUpload + "\\small";
                string imagePath = path + "\\" + imageName;
                string thumbnailImagePath = thumbnailPath + "\\" + imageName;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (!Directory.Exists(thumbnailPath))
                    Directory.CreateDirectory(thumbnailPath);

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
                    Log.Logger.Error($"Error uploading ClientVantage banner {bannerFileName}, folder {categoryToUpload} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error uploading ClientVantage banner {bannerFileName}, category {categoryToUpload}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                    TempData["response"] = "Failure";
                    TempData["message"] = "Failure uploading ClientVantage banner: " + ex.Message.Substring(0, 300);
                    return RedirectToAction("Index");
                }
            }

            if (imageMobileUrl != null)
            {
                if (imageMobileUrl.ContentType != "image/jpg" && imageMobileUrl.ContentType != "image/jpeg" && imageMobileUrl.ContentType != "image/pjpeg" &&
                    imageMobileUrl.ContentType != "image/gif" && imageMobileUrl.ContentType != "image/x-png" && imageMobileUrl.ContentType != "image/png")
                {
                    TempData["response"] = "Failure";
                    TempData["message"] = "File uploaded must be an image type (jpg, jpeg, pjpeg, gif, png)";
                    return RedirectToAction("Index");
                }

                string imageName = Path.GetFileName(imageMobileUrl.FileName);
                string path = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage Mobile\\" + categoryToUpload;
                string thumbnailPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage Mobile\\" + categoryToUpload + "\\small";
                string imagePath = path + "\\" + imageName;
                string thumbnailImagePath = thumbnailPath + "\\" + imageName;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (!Directory.Exists(thumbnailPath))
                    Directory.CreateDirectory(thumbnailPath);

                try
                {
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imageMobileUrl.CopyToAsync(stream);
                    }

                    using var image = Image.Load(imageMobileUrl.OpenReadStream());
                    image.Mutate(x => x.Resize(200, 167));
                    image.Save(thumbnailImagePath);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error uploading ClientVantage mobile banner {mobileBannerFileName}, folder {categoryToUpload} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error uploading ClientVantage mobile banner {mobileBannerFileName}, category {categoryToUpload}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                    TempData["response"] = "Failure";
                    TempData["message"] = "Failure uploading ClientVantage mobile banner: " + ex.Message.Substring(0, 300);
                    return RedirectToAction("Index");
                }
            }

            string tempBannerLog;
            if (imageUrl != null && imageMobileUrl != null)
                tempBannerLog = "Banner (" + bannerFileName + "), Mobile Banner (" + mobileBannerFileName + ")";
            else if (imageUrl != null)
                tempBannerLog = "Banner (" + bannerFileName + ")";
            else
                tempBannerLog = "Mobile Banner (" + mobileBannerFileName + ")";

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} uploaded ClientVantage banner: {bannerFileName}, mobile banner: {mobileBannerFileName}, category: {categoryToUpload} successfully");
            _logger.Information($"Uploaded ClientVantage {tempBannerLog}, category: {categoryToUpload}, successfully", null, User, "WebOrdering");
            TempData["response"] = "Success";
            TempData["message"] = "ClientVantage banner was uploaded successfully! ";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult DeleteCVBanner(string categoryToDelete, string bannerName)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is deleting CV banner " + bannerName + ", category: " + categoryToDelete);

            string imagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToDelete.Trim() + "\\" + bannerName.Trim();
            string thumbnailImagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToDelete.Trim() + "\\small\\" + bannerName.Trim();

            FileInfo imageFile = new FileInfo(imagePath);
            FileInfo thumbnailImageFile = new FileInfo(thumbnailImagePath);

            try
            {
                if (imageFile.Exists && thumbnailImageFile.Exists)
                {
                    imageFile.Delete();
                    thumbnailImageFile.Delete();
                }
                else
                {
                    Log.Logger.Error($"Error deleting ClientVantage banner {bannerName}, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: file not found");
                    _logger.Error($"Error deleting ClientVantage banner {bannerName}, category {categoryToDelete}: file not found", null, User, "WebOrdering");
                    return Json(new { success = false, message = "Failure deleting ClientVantage banner: file not found" });
                }
            }
            catch (IOException ex)
            {
                Log.Logger.Error($"Error deleting ClientVantage banner {bannerName}, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting ClientVantage banner {bannerName}, category {categoryToDelete}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return Json(new { success = false, message = "Failure deleting ClientVantage banner: " + ex.Message.Substring(0, 300) });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted ClientVantage banner {bannerName}, category {categoryToDelete} successfully");
            _logger.Information($"Deleted ClientVantage Banner ({bannerName}), category: {categoryToDelete}, successfully", null, User, "WebOrdering");
            return Json(new { success = true, message = "ClientVantage banner was deleted successfully! " });
        }

        [HttpPost]
        public ActionResult DeleteMobileCVBanner(string categoryToDelete, string bannerName)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is deleting CV mobile banner " + bannerName + ", category: " + categoryToDelete);

            string imagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage Mobile\\" + categoryToDelete.Trim() + "\\" + bannerName.Trim();
            string thumbnailImagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage Mobile\\" + categoryToDelete.Trim() + "\\small\\" + bannerName.Trim();

            FileInfo imageFile = new FileInfo(imagePath);
            FileInfo thumbnailImageFile = new FileInfo(thumbnailImagePath);

            try
            {
                if (imageFile.Exists && thumbnailImageFile.Exists)
                {
                    imageFile.Delete();
                    thumbnailImageFile.Delete();
                }
                else
                {
                    Log.Logger.Error($"Error deleting ClientVantage mobile banner {bannerName}, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: file not found");
                    _logger.Error($"Error deleting ClientVantage mobile banner {bannerName}, category {categoryToDelete}: file not found", null, User, "WebOrdering");
                    return Json(new { success = false, message = "Failure deleting ClientVantage mobile banner: file not found" });
                }
            }
            catch (IOException ex)
            {
                Log.Logger.Error($"Error deleting ClientVantage mobile banner {bannerName}, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting ClientVantage mobile banner {bannerName}, category {categoryToDelete}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return Json(new { success = false, message = "Failure deleting ClientVantage mobile banner: " + ex.Message.Substring(0, 300) });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted ClientVantage mobile banner {bannerName}, category {categoryToDelete} successfully");
            _logger.Information($"Deleted ClientVantage Mobile Banner ({bannerName}), category: {categoryToDelete}, successfully", null, User, "WebOrdering");
            return Json(new { success = true, message = "ClientVantage mobile banner was deleted successfully! " });
        }


        public async Task<ActionResult> LogAsync(int? pageNumber, int pageSize = 5, string referrerUrl = null)
        {
            IPagedList<WebOrderingLog> results = await _logger.GetAllWebOrderingLogsAsync(referrerUrl: referrerUrl, pageSize: 10, pageIndex: pageNumber ?? 1);

            var ajaxResult = new AjaxResults()
            {
                ResultStatus = ResultStatus.Success,
                Results = results
                   .Select(l => new { Text = l.ShortMessage, l.User, Created = l.CreatedOnUtc.ToLocalTime().ToString() }),
                TotalItemCount = results.TotalItemCount,
            };
            return Json(ajaxResult);
        }
    }
}
