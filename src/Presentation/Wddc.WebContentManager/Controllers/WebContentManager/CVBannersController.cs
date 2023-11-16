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
using Wddc.Core.Domain.Media;
using JsonResult = Microsoft.AspNetCore.Mvc.JsonResult;
using ActionResult = Microsoft.AspNetCore.Mvc.ActionResult;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using System;
using System.IO;
using iTextSharp.text.pdf.parser;
using Path = System.IO.Path;
using Wddc.Core.Domain.ClientVantage.Catalog;
using Category = Wddc.Core.Domain.Media.Category;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class CVBannersController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Services.Logging.ILogger _logger;
        private readonly IClientVantageBannersService _clientVantageBannersService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public CVBannersController(IClientVantageBannersService clientVantageBannersService, Services.Logging.ILogger logger, IHostingEnvironment hostingEnvironment)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _clientVantageBannersService = clientVantageBannersService;
            _hostingEnvironment = hostingEnvironment;
        }

        public ActionResult Index(string response, string message)
        {
            string tempPath = Path.Combine(this._hostingEnvironment.WebRootPath, "img\\banners");

            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);

            ViewBag.response = response;
            ViewBag.Message = message;
            return View();
        }

        public async Task<JsonResult> GetCategoriesAsync()
        {
            List<Category> categories = await _clientVantageBannersService.GetCategories();

            return Json(categories.OrderBy(_ => _.Name));
        }

        public async Task<JsonResult> GetBannersOfCategoryAsync(int categoryId)
        {
            List<BannersOfCategory> banners = await _clientVantageBannersService.GetBannersOfCategory(categoryId);

            List<Setting> settings = await _clientVantageBannersService.GetSettings();

            var path = settings.SingleOrDefault(_ => _.Name == "banner.path").Value;
            var mobilePath = settings.SingleOrDefault(_ => _.Name == "banner.path.mobile").Value;

            string tempPath = Path.Combine(this._hostingEnvironment.WebRootPath, "img\\banners");
            string tempMobilePath = Path.Combine(this._hostingEnvironment.WebRootPath, "img\\banners\\mobile");

            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);

            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(tempMobilePath);

            foreach (BannersOfCategory banner in banners)
            {
                FileInfo bannerFile = new FileInfo(path + "\\" + banner.Filename.Trim());

                if (bannerFile.Exists && bannerFile.Extension != ".db")
                {
                    bannerFile.CopyTo(Path.Combine(tempPath, banner.Filename.Trim()), true);
                }

                if (banner.HasMobileVersion == true)
                {
                    FileInfo mobileBannerFile = new FileInfo(mobilePath + "\\" + banner.MobileFilename.Trim());

                    if (mobileBannerFile.Exists && mobileBannerFile.Extension != ".db")
                    {
                        mobileBannerFile.CopyTo(Path.Combine(tempMobilePath, banner.MobileFilename.Trim()), true);
                    }
                }
            }

            return Json(banners.OrderBy(_ => _.Name));
        }

        public async Task<JsonResult> GetBannerByIdAsync(int Id)
        {
            Banner banner = await _clientVantageBannersService.GetBannerById(Id);

            List<Setting> settings = await _clientVantageBannersService.GetSettings();

            var path = settings.SingleOrDefault(_ => _.Name == "banner.path").Value;
            var mobilePath = settings.SingleOrDefault(_ => _.Name == "banner.path.mobile").Value;

            string tempPath = Path.Combine(this._hostingEnvironment.WebRootPath, "img\\banners");
            string tempMobilePath = Path.Combine(this._hostingEnvironment.WebRootPath, "img\\banners\\mobile");


            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);

            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(tempMobilePath);

            FileInfo bannerFile = new FileInfo(path + "\\" + banner.Filename.Trim());

            if (bannerFile.Exists && bannerFile.Extension != ".db")
            {
                bannerFile.CopyTo(Path.Combine(tempPath, banner.Filename.Trim()), true);
            }

            if (banner.HasMobileVersion == true)
            {
                FileInfo mobileBannerFile = new FileInfo(mobilePath + "\\" + banner.MobileFilename.Trim());

                if (mobileBannerFile.Exists && mobileBannerFile.Extension != ".db")
                {
                    mobileBannerFile.CopyTo(Path.Combine(tempMobilePath, banner.MobileFilename.Trim()), true);
                }
            }

            return Json(banner);
        }


        [HttpPost]
        public async Task<ActionResult> UploadCVBannerAsync(int categoryId, string bannerName, IFormFile bannerImage, IFormFile bannerImageMobile)
        {
            if (string.IsNullOrEmpty(bannerName))
            {
                return RedirectToAction("Index", new { response = "Failure", message = "Banner name must be filled out!" });
            }

            if (bannerImage == null)
            {
                return RedirectToAction("Index", new { response = "Failure", message = "You must select a banner to upload!" });
            }

            List<Banner> banners = await _clientVantageBannersService.GetBanners();
            if (banners.Where(_ => _.Name.Trim() == bannerName.Trim()).Count() > 0)
            {
                return RedirectToAction("Index", new { response = "Failure", message = "Banner name selected already exists! Type in a different one" });
            }

            List<Setting> settings = await _clientVantageBannersService.GetSettings();
            List<Category> categories = await _clientVantageBannersService.GetCategories();
            string catgory = categories.SingleOrDefault(_ => _.Id == categoryId).Name;

            char[] invalidList = Path.GetInvalidFileNameChars();

            foreach (char c in invalidList)
            {
                bannerName = bannerName.Replace(c.ToString(), "");
            }

            if (bannerName.Contains('\''))
                bannerName = bannerName.Replace("'", "");

            string bannerFileName = bannerName.Trim();
            string mobileBannerFileName = bannerName.Trim() + "-mobile";

            Banner banner = new Banner();
            banner.Name = bannerName;
            banner.Active = true;
            banner.IsNew = true;

            Banner_Category_Mapping bannerCategoryMapping = new Banner_Category_Mapping();
            bannerCategoryMapping.CategoryId = categoryId;

            if (bannerImage != null)
            {
                if (bannerImage.ContentType != "image/jpg" && bannerImage.ContentType != "image/jpeg" && bannerImage.ContentType != "image/pjpeg" &&
                    bannerImage.ContentType != "image/gif" && bannerImage.ContentType != "image/x-png" && bannerImage.ContentType != "image/png")
                    return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be an image type (jpg, jpeg, pjpeg, gif, png)" });

                var path = settings.SingleOrDefault(_ => _.Name == "banner.path").Value;
                var thumbnailPath = settings.SingleOrDefault(_ => _.Name == "banner.path.thumbnails").Value;

                string imageUrlExt = Path.GetExtension(bannerImage.FileName);

                string imagePath = Path.Combine(@path, bannerFileName + imageUrlExt);
                string thumbnailImagePath = Path.Combine(@thumbnailPath, bannerFileName + imageUrlExt);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (!Directory.Exists(thumbnailPath))
                    Directory.CreateDirectory(thumbnailPath);

                try
                {
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await bannerImage.CopyToAsync(stream);
                    }

                    using var image = Image.Load(bannerImage.OpenReadStream());
                    image.Mutate(x => x.Resize(300, 75));
                    image.Save(thumbnailImagePath);

                    banner.Filename = bannerFileName + imageUrlExt;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error uploading ClientVantage banner {bannerFileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error uploading ClientVantage banner {bannerFileName}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure uploading ClientVantage banner: " + bannerFileName + ": " + ex.Message.Substring(0, 300) });
                }
            }

            if (bannerImageMobile != null)
            {
                if (bannerImageMobile.ContentType != "image/jpg" && bannerImageMobile.ContentType != "image/jpeg" && bannerImageMobile.ContentType != "image/pjpeg" &&
                    bannerImageMobile.ContentType != "image/gif" && bannerImageMobile.ContentType != "image/x-png" && bannerImageMobile.ContentType != "image/png")
                    return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be an image type (jpg, jpeg, pjpeg, gif, png)" });

                var path = settings.SingleOrDefault(_ => _.Name == "banner.path.mobile").Value;
                var thumbnailPath = settings.SingleOrDefault(_ => _.Name == "banner.path.mobile.thumbnails").Value;

                string imageMobileUrlExt = Path.GetExtension(bannerImageMobile.FileName);

                string imagePath = Path.Combine(@path, mobileBannerFileName + imageMobileUrlExt);
                string thumbnailImagePath = Path.Combine(@thumbnailPath, mobileBannerFileName + imageMobileUrlExt);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (!Directory.Exists(thumbnailPath))
                    Directory.CreateDirectory(thumbnailPath);

                try
                {
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await bannerImageMobile.CopyToAsync(stream);
                    }

                    using var image = Image.Load(bannerImageMobile.OpenReadStream());
                    //image.Mutate(x => x.Resize(200, 167));
                    image.Mutate(x => x.Resize(175, 146));
                    image.Save(thumbnailImagePath);

                    banner.HasMobileVersion = true;
                    banner.MobileFilename = mobileBannerFileName + imageMobileUrlExt;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error uploading ClientVantage mobile banner {mobileBannerFileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error uploading ClientVantage mobile banner {mobileBannerFileName}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure uploading ClientVantage mobile banner: " + mobileBannerFileName + ": " + ex.Message.Substring(0, 300) });
                }
            }
            else
            {
                banner.HasMobileVersion = false;
                banner.MobileFilename = null;
            }

            try
            {
                banner = await _clientVantageBannersService.AddBanner(banner);
                bannerCategoryMapping.BannerId = banner.Id;
                bannerCategoryMapping = await _clientVantageBannersService.AddBannerCategoryMapping(bannerCategoryMapping);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error inserting record for ClientVantage banner {bannerName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error inserting record for ClientVantage banner {bannerName}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure inserting record for ClientVantage banner: " + bannerName + ": " + ex.Message.Substring(0, 300) });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} uploaded ClientVantage banner {bannerName} successfully");
            _logger.Information($"Uploaded banner: {bannerName}, category {catgory}, successfully", null, User, "WebOrdering");
            return RedirectToAction("Index", new { response = "Success", message = "ClientVantage banner: " + bannerName + " was uploaded successfully! " });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCVBannerAsync(int selectedCategoryId, int selectedBannerId, int bannerId, int categoryId, string bannerName, IFormFile bannerImage, IFormFile bannerImageMobile)
        {
            if (string.IsNullOrEmpty(bannerName))
            {
                return RedirectToAction("Index", new { response = "Failure", message = "Banner name must be filled out!" });
            }

            Banner banner = await _clientVantageBannersService.GetBannerById(bannerId);

            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is updating CV banner: " + banner.Name);

            List<Setting> settings = await _clientVantageBannersService.GetSettings();

            char[] invalidList = Path.GetInvalidFileNameChars();
            foreach (char c in invalidList)
            {
                bannerName = bannerName.Replace(c.ToString(), "");
            }

            if (bannerName.Contains('\''))
                bannerName = bannerName.Replace("'", "");

            string bannerFileName = bannerName.Trim();
            string mobileBannerFileName = bannerName.Trim() + "-mobile";

            var bannerPath = settings.SingleOrDefault(_ => _.Name == "banner.path").Value;
            var bannerPathThumbnails = settings.SingleOrDefault(_ => _.Name == "banner.path.thumbnails").Value;
            var bannerPathMobile = settings.SingleOrDefault(_ => _.Name == "banner.path.mobile").Value;
            var bannerPathMobileThumbnails = settings.SingleOrDefault(_ => _.Name == "banner.path.mobile.thumbnails").Value;

            if (bannerImage != null)
            {
                if (bannerImage.ContentType != "image/jpg" && bannerImage.ContentType != "image/jpeg" && bannerImage.ContentType != "image/pjpeg" &&
                    bannerImage.ContentType != "image/gif" && bannerImage.ContentType != "image/x-png" && bannerImage.ContentType != "image/png")
                    return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be an image type (jpg, jpeg, pjpeg, gif, png)" });

                string imageUrlExt = Path.GetExtension(bannerImage.FileName);

                string imagePath = Path.Combine(@bannerPath, bannerFileName + imageUrlExt);
                string thumbnailImagePath = Path.Combine(@bannerPathThumbnails, bannerFileName + imageUrlExt);

                if (!string.IsNullOrEmpty(banner.MobileFilename))
                {
                    string imagePathOld = Path.Combine(@bannerPath, banner.Filename);
                    string thumbnailImagePathOld = Path.Combine(@bannerPathThumbnails, banner.Filename);

                    FileInfo imageFileOld = new FileInfo(imagePathOld);
                    FileInfo thumbnailImageFileOld = new FileInfo(thumbnailImagePathOld);

                    try
                    {
                        if (imageFileOld.Exists)
                            imageFileOld.Delete();

                        if (thumbnailImageFileOld.Exists)
                            thumbnailImageFileOld.Delete();

                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"Error deleting ClientVantage banner {imagePathOld} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                        _logger.Error($"Error deleting ClientVantage banner {imagePathOld}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                        return RedirectToAction("Index", new { response = "Failure", message = "Failure deleting ClientVantage banner: " + imagePathOld + ": " + ex.Message.Substring(0, 300) });
                    }
                }

                try
                {
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await bannerImage.CopyToAsync(stream);
                    }

                    using var image = Image.Load(bannerImage.OpenReadStream());
                    image.Mutate(x => x.Resize(300, 75));
                    image.Save(thumbnailImagePath);

                    banner.Filename = bannerFileName + imageUrlExt;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error uploading ClientVantage banner {bannerFileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error uploading ClientVantage banner {bannerFileName}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure uploading ClientVantage banner: " + bannerFileName + ": " + ex.Message.Substring(0, 300) });
                }
            }
            else
            {
                if (banner.Name.Trim() != bannerName.Trim() && !string.IsNullOrEmpty(banner.Filename))
                {
                    string imagePathOld = Path.Combine(@bannerPath, banner.Filename);
                    string thumbnailImagePathOld = Path.Combine(@bannerPathThumbnails, banner.Filename);

                    string imageExt = Path.GetExtension(banner.Filename);

                    string imagePathRename = Path.Combine(@bannerPath, bannerName.Trim() + imageExt);
                    string thumbnailImagePathRename = Path.Combine(@bannerPathThumbnails, bannerName.Trim() + imageExt);

                    try
                    {
                        FileInfo imageFileOld = new FileInfo(imagePathOld);
                        FileInfo thumbnailImageFileOld = new FileInfo(thumbnailImagePathOld);

                        if (imageFileOld.Exists)
                            imageFileOld.MoveTo(imagePathRename);

                        if (thumbnailImageFileOld.Exists)
                            thumbnailImageFileOld.MoveTo(thumbnailImagePathRename);

                        banner.Filename = bannerName.Trim() + imageExt;

                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"Error renaming ClientVantage banner {imagePathOld} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                        _logger.Error($"Error renaming ClientVantage banner {imagePathOld}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                        return RedirectToAction("Index", new { response = "Failure", message = "Failure renaming ClientVantage banner: " + imagePathOld + ": " + ex.Message.Substring(0, 300) });
                    }
                }
            }

            if (bannerImageMobile != null)
            {
                if (bannerImageMobile.ContentType != "image/jpg" && bannerImageMobile.ContentType != "image/jpeg" && bannerImageMobile.ContentType != "image/pjpeg" &&
                    bannerImageMobile.ContentType != "image/gif" && bannerImageMobile.ContentType != "image/x-png" && bannerImageMobile.ContentType != "image/png")
                    return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be an image type (jpg, jpeg, pjpeg, gif, png)" });

                string imageMobileUrlExt = Path.GetExtension(bannerImageMobile.FileName);

                string imagePath = Path.Combine(@bannerPathMobile, mobileBannerFileName + imageMobileUrlExt);
                string thumbnailImagePath = Path.Combine(@bannerPathMobileThumbnails, mobileBannerFileName + imageMobileUrlExt);

                if (!string.IsNullOrEmpty(banner.MobileFilename)) {
                    string imagePathOld = Path.Combine(@bannerPathMobile, banner.MobileFilename);
                    string thumbnailImagePathOld = Path.Combine(@bannerPathMobileThumbnails, banner.MobileFilename);

                    FileInfo imageFileOld = new FileInfo(imagePathOld);
                    FileInfo thumbnailImageFileOld = new FileInfo(thumbnailImagePathOld);

                    try
                    {
                        if (imageFileOld.Exists)
                            imageFileOld.Delete();

                        if (thumbnailImageFileOld.Exists)
                            thumbnailImageFileOld.Delete();

                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"Error deleting ClientVantage banner {imagePathOld} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                        _logger.Error($"Error deleting ClientVantage banner {imagePathOld}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                        return RedirectToAction("Index", new { response = "Failure", message = "Failure deleting ClientVantage banner: " + imagePathOld + ": " + ex.Message.Substring(0, 300) });
                    }
                }

                try
                {
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await bannerImageMobile.CopyToAsync(stream);
                    }

                    using var image = Image.Load(bannerImageMobile.OpenReadStream());
                    //image.Mutate(x => x.Resize(200, 167));
                    image.Mutate(x => x.Resize(175, 146));
                    image.Save(thumbnailImagePath);

                    banner.HasMobileVersion = true;
                    banner.MobileFilename = mobileBannerFileName + imageMobileUrlExt;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error uploading ClientVantage mobile banner {mobileBannerFileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error uploading ClientVantage mobile banner {mobileBannerFileName}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure uploading ClientVantage mobile banner: " + mobileBannerFileName + ": " + ex.Message.Substring(0, 300) });
                }
            }
            else
            {
                if (banner.Name.Trim() != bannerName.Trim() && !string.IsNullOrEmpty(banner.MobileFilename))
                {
                    string imagePathOld = Path.Combine(@bannerPathMobile, banner.MobileFilename);
                    string thumbnailImagePathOld = Path.Combine(@bannerPathMobileThumbnails, banner.MobileFilename);

                    string imageExt = Path.GetExtension(banner.MobileFilename);

                    string imagePathRename = Path.Combine(@bannerPathMobile, bannerName.Trim() + "-mobile" + imageExt);
                    string thumbnailImagePathRename = Path.Combine(@bannerPathMobileThumbnails, bannerName.Trim() + "-mobile" + imageExt);

                    try
                    {
                        FileInfo imageFileOld = new FileInfo(imagePathOld);
                        FileInfo thumbnailImageFileOld = new FileInfo(thumbnailImagePathOld);

                        if (imageFileOld.Exists)
                            imageFileOld.MoveTo(imagePathRename);

                        if (thumbnailImageFileOld.Exists)
                            thumbnailImageFileOld.MoveTo(thumbnailImagePathRename);

                        banner.MobileFilename = bannerName.Trim() + "-mobile" + imageExt;

                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"Error renaming ClientVantage mobile banner {imagePathOld} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                        _logger.Error($"Error renaming ClientVantage mobile banner {imagePathOld}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                        return RedirectToAction("Index", new { response = "Failure", message = "Failure renaming ClientVantage mobile banner: " + imagePathOld + ": " + ex.Message.Substring(0, 300) });
                    }
                }
            }

            try
            {
                banner.Name = bannerName;
                banner.Active = true;
                banner.IsNew = true;

                await _clientVantageBannersService.UpdateBanner(banner, bannerId);

                if (categoryId != selectedCategoryId)
                {
                    Banner_Category_Mapping bannerCategoryMapping = await _clientVantageBannersService.GetBannerCategoryMappingByBannerId(bannerId);
                    if (bannerCategoryMapping.CategoryId == selectedCategoryId)
                    {
                        await _clientVantageBannersService.DeleteBannerCategoryMapping(bannerCategoryMapping);

                        bannerCategoryMapping = new Banner_Category_Mapping();
                        bannerCategoryMapping.BannerId = bannerId;
                        bannerCategoryMapping.CategoryId = categoryId;
                        bannerCategoryMapping = await _clientVantageBannersService.AddBannerCategoryMapping(bannerCategoryMapping);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating record for Banner Category Mapping by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating record for Banner Category Mapping: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure updating record for Banner Category Mapping: " + ex.Message.Substring(0, 300) });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated ClientVantage banner {bannerName} successfully");
            _logger.Information($"Updated banner: {bannerName} successfully", null, User, "WebOrdering");
            return RedirectToAction("Index", new { response = "Success", message = "ClientVantage banner: " + bannerName + " was updated successfully! " });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCVBannerAsync(int bannerId)
        {
            Banner banner = await _clientVantageBannersService.GetBannerById(bannerId);

            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is deleting CV banner: " + banner.Name);

            try
            {
                await _clientVantageBannersService.DeleteBanner(bannerId);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting ClientVantage banner {banner.Name} from db by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting ClientVantage banner {banner.Name} from db by: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(new { success = false, message = "Failure deleting ClientVantage banner: " + banner.Name + " from db" });
            }

            List<Setting> settings = await _clientVantageBannersService.GetSettings();

            var bannerPath = settings.SingleOrDefault(_ => _.Name == "banner.path").Value;
            var bannerPathThumbnails = settings.SingleOrDefault(_ => _.Name == "banner.path.thumbnails").Value;

            string imagePath = Path.Combine(@bannerPath, banner.Filename);
            string thumbnailImagePath = Path.Combine(@bannerPathThumbnails, banner.Filename);

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
                    Log.Logger.Error($"Error deleting ClientVantage banner {banner.Filename} by {User.Identity.Name.Substring(7).ToLower()}: file not found");
                    _logger.Error($"Error deleting ClientVantage banner {banner.Filename}: file not found", null, User, "WebOrdering");
                    return Json(new { success = false, message = "Failure deleting ClientVantage banner: "+ banner.Filename + ", file not found" });
                }
            }
            catch (IOException ex)
            {
                Log.Logger.Error($"Error deleting ClientVantage banner {banner.Filename} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting ClientVantage banner {banner.Filename}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return Json(new { success = false, message = "Failure deleting ClientVantage banner: " + banner.Filename + ", " + ex.Message.Substring(0, 300) });
            }

            if (banner.MobileFilename != null)
            {
                var bannerPathMobile = settings.SingleOrDefault(_ => _.Name == "banner.path.mobile").Value;
                var bannerPathMobileThumbnails = settings.SingleOrDefault(_ => _.Name == "banner.path.mobile.thumbnails").Value;

                string imageMobilePath = Path.Combine(@bannerPathMobile, banner.MobileFilename);
                string thumbnailImageMobilePath = Path.Combine(@bannerPathMobileThumbnails, banner.MobileFilename);

                FileInfo imageMobileFile = new FileInfo(imageMobilePath);
                FileInfo thumbnailImageMobileFile = new FileInfo(thumbnailImageMobilePath);

                try
                {
                    if (imageMobileFile.Exists && thumbnailImageMobileFile.Exists)
                    {
                        imageMobileFile.Delete();
                        thumbnailImageMobileFile.Delete();
                    }
                    else
                    {
                        Log.Logger.Error($"Error deleting ClientVantage mobile banner {banner.MobileFilename} by {User.Identity.Name.Substring(7).ToLower()}: file not found");
                        _logger.Error($"Error deleting ClientVantage mobile banner {banner.MobileFilename}: file not found", null, User, "WebOrdering");
                        return Json(new { success = false, message = "Failure deleting ClientVantage mobile banner: " + banner.MobileFilename + ", file not found" });
                    }
                }
                catch (IOException ex)
                {
                    Log.Logger.Error($"Error deleting ClientVantage mobile banner {banner.MobileFilename} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error deleting ClientVantage mobile banner {banner.MobileFilename}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                    return Json(new { success = false, message = "Failure deleting ClientVantage mobile banner: " + banner.MobileFilename + ", " + ex.Message.Substring(0, 300) });
                }
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted ClientVantage banner: {banner.Name} successfully");
            _logger.Information($"Deleted ClientVantage banner: {banner.Name}, successfully", null, User, "WebOrdering");
            return Json(new { success = true, message = "ClientVantage banner: " + banner.Name + " was deleted successfully! " });
        }

        //[HttpPost]
        //public async Task<ActionResult> DeleteCVBannerAsync(string categoryToDelete, string bannerName)
        //{
        //    Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is deleting CV banner " + bannerName + ", category: " + categoryToDelete);

        //    string imagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToDelete.Trim() + "\\" + bannerName.Trim();
        //    string thumbnailImagePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\images\\Client Vantage\\" + categoryToDelete.Trim() + "\\small\\" + bannerName.Trim();

        //    FileInfo imageFile = new FileInfo(imagePath);
        //    FileInfo thumbnailImageFile= new FileInfo(thumbnailImagePath);

        //    try
        //    {
        //        if (imageFile.Exists && thumbnailImageFile.Exists)
        //        {
        //            imageFile.Delete();
        //            thumbnailImageFile.Delete();
        //        }
        //        else
        //        {
        //            Log.Logger.Error($"Error deleting ClientVantage banner, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: file not found");
        //            _logger.Error($"Error deleting ClientVantage banner, category {categoryToDelete}: file not found", null, User, "WebOrdering");
        //            return Json(new { success = false, message = "Failure deleting ClientVantage banner: file not found"});
        //        }
        //    }
        //    catch (IOException ex)
        //    {
        //        Log.Logger.Error($"Error deleting ClientVantage banner, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
        //        _logger.Error($"Error deleting ClientVantage banner, category {categoryToDelete}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
        //        return Json(new { success = false, message = "Failure deleting ClientVantage banner: " + ex.Message.Substring(0, 300) });
        //    }

        //    Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted ClientVantage banner, category {categoryToDelete} successfully");
        //    _logger.Information($"Deleted banner from ClientVantage banners library, category {categoryToDelete}, successfully", null, User, "WebOrdering");
        //    return Json(new { success = true, message = "ClientVantage banner was deleted successfully! "});
        //}

        [HttpPost]
        public async Task<ActionResult> DeleteMobileCVBannerAsync(string categoryToDelete, string bannerName)
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
                    Log.Logger.Error($"Error deleting ClientVantage mobile banner, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: file not found");
                    _logger.Error($"Error deleting ClientVantage mobile banner, category {categoryToDelete}: file not found", null, User, "WebOrdering");
                    return Json(new { success = false, message = "Failure deleting ClientVantage mobile banner: file not found" });
                }
            }
            catch (IOException ex)
            {
                Log.Logger.Error($"Error deleting ClientVantage mobile banner, category {categoryToDelete} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting ClientVantage mobile banner, category {categoryToDelete}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return Json(new { success = false, message = "Failure deleting ClientVantage mobile banner: " + ex.Message.Substring(0, 300) });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted ClientVantage mobile banner, category {categoryToDelete} successfully");
            _logger.Information($"Deleted banner from ClientVantage mobile banners library, category {categoryToDelete}, successfully", null, User, "WebOrdering");
            return Json(new { success = true, message = "ClientVantage mobile banner was deleted successfully! " });
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
