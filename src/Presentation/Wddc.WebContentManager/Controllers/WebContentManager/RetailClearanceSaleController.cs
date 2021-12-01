using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Wddc.WebContentManager.Services.WebContent.Sales.RetailClearanceSale;
using Wddc.WebContentManager.Models.WebContent.Sales.RetailClearanceSale;
using Serilog;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using PagedList;
using Wddc.WebContentManager.Models;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class RetailClearanceSaleController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly IRetailClearanceSaleService _retailClearanceSaleService;

        public RetailClearanceSaleController(IRetailClearanceSaleService retailClearanceSaleService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _retailClearanceSaleService = retailClearanceSaleService;
        }

        public async Task<ActionResult> IndexAsync(string response, string message)
        {
            Log.Logger.Information("The system is getting the list of Web Clearance Retail Sale from table dbo.Web_Clearance");
            List<Web_Clearance> webClearanceRetailSales = await _retailClearanceSaleService.GetAllWebClearanceRetail();
            RetailClearanceSaleModel retailClearanceSaleModel = new RetailClearanceSaleModel
            {
                WebRetailClearance = webClearanceRetailSales.OrderBy(_ => _.ItemNumber)
            };

            ViewBag.response = response;
            ViewBag.Message = message;
            return View();
        }

        public async Task<JsonResult> GetWebClearanceRetailAsync()
        {
            Log.Logger.Information("The system is getting the list of Web Clearance Retail Sale from table dbo.Web_Clearance to populate the grid");
            List<Web_Clearance> items = await _retailClearanceSaleService.GetAllWebClearanceRetail();
            return Json(items.OrderBy(_ => _.ItemNumber));
        }

        public async Task<ActionResult> UpdateWebClearanceRetailAsync([DataSourceRequest] DataSourceRequest request, Web_Clearance model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Clearance Retail Sale of ItemNumber: {@model.ItemNumber}");

            var toUpdate = await _retailClearanceSaleService.GetWebClearanceRetailByItemNumber(model.ItemNumber);
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Clearance Retail Sale of ItemNumber: {@model.ItemNumber} from: " + "{@toUpdate}", toUpdate);

            toUpdate.regular_price = model.regular_price;
            toUpdate.sale_price = model.sale_price;
            toUpdate.override_qty = model.override_qty;
            toUpdate.description = model.description;
            toUpdate.expiry_date = model.expiry_date;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Clearance Retail Sale of ItemNumber: {@model.ItemNumber} to: " + "{@model}", model);

            try
            {
                await _retailClearanceSaleService.UpdateWebClearanceRetail(toUpdate, model.ItemNumber);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated Web Clearance Retail Sale successfully");
                _logger.Information($"Updated Web Clearance Retail Sale successfully. Item number: {toUpdate.ItemNumber}, Regular price: {toUpdate.regular_price}, Sale price: {toUpdate.sale_price}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating Web Clearance Retail Sale by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating Web Clearance Retail Sale. Item number: {toUpdate.ItemNumber}, Regular price: {toUpdate.regular_price}, Sale price: {toUpdate.sale_price} : {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> CreateWebClearanceRetailAsync([DataSourceRequest] DataSourceRequest request, Web_Clearance model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Clearance Retail Sale.");

            Web_Clearance newWebClearanceRetail = new Web_Clearance();
            newWebClearanceRetail.ItemNumber = model.ItemNumber;
            newWebClearanceRetail.CategoryID = 1;
            newWebClearanceRetail.regular_price = model.regular_price;
            newWebClearanceRetail.sale_price = model.sale_price;
            newWebClearanceRetail.override_qty = model.override_qty;
            newWebClearanceRetail.description = model.description;
            newWebClearanceRetail.expiry_date = model.expiry_date;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Clearance Retail Sale: {@newWebClearanceRetail}", newWebClearanceRetail);

            try
            {
                await _retailClearanceSaleService.CreateWebClearanceRetail(newWebClearanceRetail);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new Web Clearance Retail Sale, ItemNumber: {newWebClearanceRetail.ItemNumber} successfully");
                _logger.Information($"Added new Web Clearance Retail Sale successfully. Item number: {newWebClearanceRetail.ItemNumber}, Regular price: {newWebClearanceRetail.regular_price}, Sale price: {newWebClearanceRetail.sale_price}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding Web Clearance Retail Sale by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding Web Clearance Retail Sale. Item number: {newWebClearanceRetail.ItemNumber}, Regular price: {newWebClearanceRetail.regular_price}, Sale price: {newWebClearanceRetail.sale_price}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> DeleteWebClearanceRetailAsync([DataSourceRequest] DataSourceRequest request, Web_Clearance model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting Web Clearance Retail Sale, Item number: {@model.ItemNumber}");

            try
            {
                await _retailClearanceSaleService.DeleteWebClearanceRetail(model.ItemNumber);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted Web Clearance Retail Sale successfully");
                _logger.Information($"Deleted Web Clearance Retail successfully. Item number: {model.ItemNumber}, Regular price: {model.regular_price}, Sale price: {model.sale_price}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Web Clearance Retail Sale by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Web Clearance Retail Sale. Item number: {model.ItemNumber}, Regular price: {model.regular_price}, Sale price: {model.sale_price}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        public async Task<JsonResult> GetItemInfoAsync(string itemNumber)
        {
            Log.Logger.Information("The system is getting item info of item number: " + itemNumber);
            GetItemInfo_Result info = await _retailClearanceSaleService.GetItemInfo(itemNumber);
            return Json(info);
        }

        public async Task<JsonResult> GetWebClearanceRetailByItemNumberAsync(string itemNumber)
        {
            Log.Logger.Information("The system is getting Web Clearance Retail Sale from table dbo.Web_Clearance of item number: " + itemNumber);
            Web_Clearance item = await _retailClearanceSaleService.GetWebClearanceRetailByItemNumber(itemNumber);
            return Json(item);
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
