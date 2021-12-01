using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Wddc.WebContentManager.Services.WebContent.Sales.EquipmentClearanceSale;
using Wddc.WebContentManager.Models.WebContent.Sales.EquipmentClearanceSale;
using Serilog;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using PagedList;
using Wddc.WebContentManager.Models;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class EquipmentClearanceSaleController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly IEquipmentClearanceSaleService _equipmentClearanceSaleService;

        public EquipmentClearanceSaleController(IEquipmentClearanceSaleService equipmentClearanceSaleService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _equipmentClearanceSaleService = equipmentClearanceSaleService;
        }

        public async Task<ActionResult> IndexAsync(string response, string message)
        {
            Log.Logger.Information("The system is getting the list of Web Clearance Equipment Sale from table dbo.Web_Clearance");
            List<Web_Clearance> webClearanceEquipmentSales = await _equipmentClearanceSaleService.GetAllWebClearanceEquipment();
            EquipmentClearanceSaleModel equipmentClearanceSaleModel = new EquipmentClearanceSaleModel
            {
                WebEquipmentClearance = webClearanceEquipmentSales.OrderBy(_ => _.ItemNumber)
            };

            ViewBag.response = response;
            ViewBag.Message = message;
            return View();
        }

        public async Task<JsonResult> GetWebClearanceEquipmentAsync()
        {
            Log.Logger.Information("The system is getting the list of Web Clearance Equipment Sale from table dbo.Web_Clearance to populate the grid");
            List<Web_Clearance> items = await _equipmentClearanceSaleService.GetAllWebClearanceEquipment();
            return Json(items.OrderBy(_ => _.ItemNumber));
        }

        public async Task<ActionResult> UpdateWebClearanceEquipmentAsync([DataSourceRequest] DataSourceRequest request, Web_Clearance model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Clearance Equipment Sale of ItemNumber: {@model.ItemNumber}");

            var toUpdate = await _equipmentClearanceSaleService.GetWebClearanceEquipmentByItemNumber(model.ItemNumber);
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Clearance Equipment Sale of ItemNumber: {@model.ItemNumber} from: " + "{@toUpdate}", toUpdate);
            toUpdate.regular_price = model.regular_price;
            toUpdate.sale_price = model.sale_price;
            toUpdate.override_qty = model.override_qty;
            toUpdate.description = model.description;
            toUpdate.expiry_date = model.expiry_date;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Clearance Equipment Sale of ItemNumber: {@model.ItemNumber} to: " + "{@model}", model);

            try
            {
                await _equipmentClearanceSaleService.UpdateWebClearanceEquipment(toUpdate, model.ItemNumber);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated Web Clearance Equipment Sale successfully");
                _logger.Information($"Updated Web Clearance Equipment Sale successfully. Item number: {toUpdate.ItemNumber}, Regular price: {toUpdate.regular_price}, Sale price: {toUpdate.sale_price}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating Web Clearance Equipment Sale by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating Web Clearance Equipment Sale. Item number: {toUpdate.ItemNumber}, Regular price: {toUpdate.regular_price}, Sale price: {toUpdate.sale_price} : {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> CreateWebClearanceEquipmentAsync([DataSourceRequest] DataSourceRequest request, Web_Clearance model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Clearance Equipment Sale.");

            Web_Clearance newWebClearanceEquipment = new Web_Clearance();
            newWebClearanceEquipment.ItemNumber = model.ItemNumber;
            newWebClearanceEquipment.CategoryID = 1;
            newWebClearanceEquipment.regular_price = model.regular_price;
            newWebClearanceEquipment.sale_price = model.sale_price;
            newWebClearanceEquipment.override_qty = model.override_qty;
            newWebClearanceEquipment.description = model.description;
            newWebClearanceEquipment.expiry_date = model.expiry_date;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Clearance Equipment Sale: {@newWebClearanceEquipment}", newWebClearanceEquipment);

            try
            {
                await _equipmentClearanceSaleService.CreateWebClearanceEquipment(newWebClearanceEquipment);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new Web Clearance Equipment Sale, ItemNumber: {newWebClearanceEquipment.ItemNumber} successfully");
                _logger.Information($"Added new Web Clearance Equipment Sale successfully. Item number: {newWebClearanceEquipment.ItemNumber}, Regular price: {newWebClearanceEquipment.regular_price}, Sale price: {newWebClearanceEquipment.sale_price}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding Web Clearance Equipment Sale by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding Web Clearance Equipment Sale. Item number: {newWebClearanceEquipment.ItemNumber}, Regular price: {newWebClearanceEquipment.regular_price}, Sale price: {newWebClearanceEquipment.sale_price}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> DeleteWebClearanceEquipmentAsync([DataSourceRequest] DataSourceRequest request, Web_Clearance model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting Web Clearance Equipment Sale, Item number: {@model.ItemNumber}");

            try
            {
                await _equipmentClearanceSaleService.DeleteWebClearanceEquipment(model.ItemNumber);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted Web Clearance Equipment Sale successfully");
                _logger.Information($"Deleted Web Clearance Equipment successfully. Item number: {model.ItemNumber}, Regular price: {model.regular_price}, Sale price: {model.sale_price}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Web Clearance Equipment Sale by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Web Clearance Equipment Sale. Item number: {model.ItemNumber}, Regular price: {model.regular_price}, Sale price: {model.sale_price}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        public async Task<JsonResult> GetItemInfoAsync(string itemNumber)
        {
            Log.Logger.Information("The system is getting item info of item number: " + itemNumber);
            GetItemInfo_Result info = await _equipmentClearanceSaleService.GetItemInfo(itemNumber);
            return Json(info);
        }

        public async Task<JsonResult> GetWebClearanceEquipmentByItemNumberAsync(string itemNumber)
        {
            Log.Logger.Information("The system is getting Web Clearance Equipment Sale from table dbo.Web_Clearance of item number: " + itemNumber);
            Web_Clearance item = await _equipmentClearanceSaleService.GetWebClearanceEquipmentByItemNumber(itemNumber);
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
