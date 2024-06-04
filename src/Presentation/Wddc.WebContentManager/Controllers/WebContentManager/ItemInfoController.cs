using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Http;
using System.IO;
using PagedList;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using IronPdf;
using Wddc.WebContentManager.Services.WebContent.Newsletter;
using Wddc.Core.Domain.AppData.Items;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class ItemInfoController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Services.Logging.ILogger _logger;
        private readonly IItemInfoService _itemInfoService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ItemInfoController(IItemInfoService itemInfoService, Services.Logging.ILogger logger, IHostingEnvironment hostingEnvironment)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _itemInfoService = itemInfoService;
            _hostingEnvironment = hostingEnvironment;
        }

        public ActionResult Index(/*string response, string message*/)
        {
            bool? success = HttpContext.Session.GetString("Success") == null ? null : HttpContext.Session.GetString("Success") == "True" ? true : false;
            string message = HttpContext.Session.GetString("Message");

            //ViewBag.response = response;
            ViewBag.Success = success;
            ViewBag.Message = message;
            return View();
        }

        public async Task<JsonResult> GetItemsAsync()
        {
            Log.Logger.Information("The system is getting items");
            List<Items> items = await _itemInfoService.GetItems();
            return Json(items);
        }

        public async Task<JsonResult> CheckItemFileInfoAsync(string itemNumber)
        {
            string directoryPath = @"\\WEBsrvr\WDDCMembers\WDDCWebPages\wddc_members\CS\Item Info";

            bool exists = false;

            try
            {
                string[] files = await Task.Run(() => Directory.GetFiles(directoryPath));

                exists = files.Any(file => Path.GetFileName(file).Equals($"{itemNumber.Trim()}.pdf", StringComparison.OrdinalIgnoreCase));

                return Json(new { exists });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");

                return Json(new { exists, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddItemInfoFileAsync(string newItemNumber, IFormFile newInfoFileUrl /*, DateTime newIssueDate,*/)
        {
            if (newInfoFileUrl.ContentType != "application/pdf")
            {
                HttpContext.Session.SetString("Success", "False");
                HttpContext.Session.SetString("Message", "Failure adding info pdf file. File uploaded must be PDF!");
                return RedirectToAction("Index");
            }
                
            if (newInfoFileUrl != null)
            {
                string itemInfoPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Item Info";

                try
                {
                    using (var stream = new FileStream(Path.Combine(itemInfoPath, newItemNumber + ".pdf"), FileMode.Create))
                    {
                        await newInfoFileUrl.CopyToAsync(stream);
                    }

                    Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added pdf info file for item: {newItemNumber} successfully");
                    _logger.Information($"Added pdf info file successfully for item: {newItemNumber}", null, User, "WebOrdering");

                    HttpContext.Session.SetString("Success", "True");
                    HttpContext.Session.SetString("Message", "Item info pdf file was added successfully to the website!");
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error adding pdf info file for item: {newItemNumber} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error adding pdf info file for item: {newItemNumber}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");

                    HttpContext.Session.SetString("Success", "False");
                    HttpContext.Session.SetString("Message", "Failure adding info pdf file!");
                }
            }
            else
            {
                HttpContext.Session.SetString("Success", "False");
                HttpContext.Session.SetString("Message", "No pdf file was selected!");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteItemInfoFile(string deleteItemNumber)
        {
            if (deleteItemNumber != null)
            {
                string fileName = deleteItemNumber + ".pdf";
                string itemInfoPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Item Info";
                string toDeleteFile = Path.Combine(itemInfoPath, fileName);

                try
                {
                    System.IO.File.Delete(toDeleteFile);

                    Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted pdf info file for item: {deleteItemNumber} successfully");
                    _logger.Information($"Deleted pdf info file successfully for item: {deleteItemNumber}", null, User, "WebOrdering");

                    HttpContext.Session.SetString("Success", "True");
                    HttpContext.Session.SetString("Message", "Item info pdf file was deleted successfully from the website!");
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error deleting pdf info file for item: {deleteItemNumber} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error deleting pdf info file for item: {deleteItemNumber}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");

                    HttpContext.Session.SetString("Success", "False");
                    HttpContext.Session.SetString("Message", "Failure deleting info pdf file!");
                }
            }
            else
            {
                HttpContext.Session.SetString("Success", "False");
                HttpContext.Session.SetString("Message", "No item was selected!");
            }

            return RedirectToAction("Index");
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
