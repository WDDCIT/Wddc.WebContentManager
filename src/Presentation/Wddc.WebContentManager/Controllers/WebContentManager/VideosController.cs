using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Kendo.Mvc.Extensions;
using Wddc.WebContentManager.Services.WebContent.Videos;
using Wddc.WebContentManager.Models.WebContent.Videos;
using Serilog;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using PagedList;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class VideosController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly IVideosService _videosService;

        public VideosController(IVideosService videosService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _videosService = videosService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetCategoriesAsync()
        {
            Log.Logger.Information("The system is getting the list of Videos Categories from table dbo.VID001");
            List<VID001> VID001s = await _videosService.GetAllVID001();
            return Json(VID001s.OrderBy(_ => _.VD01_CTGY_SORT));
        }

        public async Task<JsonResult> GetFirstCategoryAsync()
        {
            Log.Logger.Information("The system is getting the first Video Category from table dbo.VID001");
            List<VID001> VID001s = await _videosService.GetAllVID001();
            return Json(VID001s.OrderBy(_ => _.VD01_CTGY_SORT).FirstOrDefault());
        }

        public async Task<JsonResult> GetAlbumsByCategoryAsync(int CTGY_NBR)
        {
            Log.Logger.Information("The system is getting the list of Videos Albums from table dbo.VID002 of VD01_CTGY_NBR: " + CTGY_NBR);
            List<VID002> VID002s = await _videosService.GetVID002ByCTGY_NBR(CTGY_NBR);
            return Json(VID002s.OrderBy(_ => _.VD02_NAME));
        }


        [HttpPost]
        public async Task<ActionResult> ReorderVID001sAsync(VideosModel model)
        {
            Log.Logger.Information("The system is reordering the list of Videos Categories in table dbo.VID001 by updating field VD01_CTGY_SORT");
            IEnumerable<VID001> VID001s = model.VID001s;
            try
            {
                await _videosService.ReorderVID001s(VID001s);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} reordered videos categories successfully");
                _logger.Information($"Reordered videos categories successfully", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error reordering videos categories by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error reordering videos categories: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                throw ex;
            }
            return Json(VID001s);
        }

        [HttpPost]
        public async Task<ActionResult> AddCategoryAsync(string newCategoryName)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Video Categroy to table dbo.VID001");

            VID001 newVID001 = new VID001();
            newVID001.VD01_CTGY_NAME = newCategoryName;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Video Categroy: {@newVID001}", newVID001);

            try
            {
                newVID001 = await _videosService.CreateVID001(newVID001);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new Video Categroy with VD01_CTGY_NBR: {newVID001.VD01_CTGY_NBR} successfully");
                _logger.Information($"Added new Video Categroy successfully. Category name: {newVID001.VD01_CTGY_NAME}", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding new Video Categroy by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding new Video Categroy. Category name: {newVID001.VD01_CTGY_NAME}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                throw ex;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> AddAlbumAsync(int newAlbumCategoryNumber, string newAlbumName, string newAlbumNumber)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Video Album to table dbo.VID002");

            VID002 newVID002 = new VID002();
            newVID002.VD01_CTGY_NBR = newAlbumCategoryNumber;
            newVID002.VD02_NAME = newAlbumName;
            newVID002.VD02_EMBD_CODE = "//vimeo.com/album/" + newAlbumNumber;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Video Album: {@newVID002}", newVID002);

            try
            {
                newVID002 = await _videosService.CreateVID002(newVID002);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new Video Album with VD02_ID: {newVID002.VD02_ID} successfully");
                _logger.Information($"Added new Video Album successfully. Name: {newVID002.VD02_NAME}", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding new Video Album by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding new Video Album. Name: {newVID002.VD02_NAME}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                throw ex;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCategoryAsync(int categoryNumber, string categoryName)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating the Video Category of VD01_CTGY_NBR: {categoryNumber}");

            var toUpdateCategory = await _videosService.GetVID001ByCTGY_NBR(categoryNumber);
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Video Category from: " + "{@toUpdateCategory}", toUpdateCategory);
            toUpdateCategory.VD01_CTGY_NAME = categoryName;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Video Category to: " + "{@toUpdateCategory}", toUpdateCategory);

            try
            {
                await _videosService.UpdateVID001(toUpdateCategory, categoryNumber);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated Video Category successfully");
                _logger.Information($"Updated Video Category successfully. Category name: {toUpdateCategory.VD01_CTGY_NAME}", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating Video Category by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating Video Category. Category name: {toUpdateCategory.VD01_CTGY_NAME}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                throw ex;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAlbumAsync(int albumId, int albumCategoryNumber, string albumName, string albumNumber)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating the Video Album of VD02_ID: {albumId}");

            var toUpdateAlbum = await _videosService.GetVID002ByID(albumId);
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Video Album from: " + "{@toUpdateAlbum}", toUpdateAlbum);
            toUpdateAlbum.VD01_CTGY_NBR = albumCategoryNumber;
            toUpdateAlbum.VD02_NAME = albumName;
            toUpdateAlbum.VD02_EMBD_CODE = "//vimeo.com/album/" + albumNumber;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Video Album to: " + "{@toUpdateAlbum}", toUpdateAlbum);

            try
            {
                await _videosService.UpdateVID002(toUpdateAlbum, albumId);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated Video Album successfully");
                _logger.Information($"Updated Video Album successfully. Name: {toUpdateAlbum.VD02_NAME}", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating Video Album by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating Video Album. Name: {toUpdateAlbum.VD02_NAME}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                throw ex;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCategoryAsync(int CTGY_NBR)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting Video Category of VD01_CTGY_NBR: {CTGY_NBR}");
            var toDelete = await _videosService.GetVID001ByCTGY_NBR(CTGY_NBR);

            try
            {
                await _videosService.DeleteVID001(CTGY_NBR);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted Video Category successfully");
                _logger.Information($"Deleted Video Category successfully. Category name: {toDelete.VD01_CTGY_NAME}", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Video Category by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Video Category. Category name: {toDelete.VD01_CTGY_NAME}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAlbumAsync(int ID)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting Video Album of VD02_ID: {ID}");
            var toDelete = await _videosService.GetVID002ByID(ID);

            try
            {
                await _videosService.DeleteVID002(ID);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted Video Album successfully");
                _logger.Information($"Deleted Video Album successfully. Name: {toDelete.VD02_NAME}", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Video Album by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Video Album. Name: {toDelete.VD02_NAME}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(new { success = false });
            }

            return Json(new { success = true });
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
