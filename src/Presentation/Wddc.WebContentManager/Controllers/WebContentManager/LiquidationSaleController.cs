using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Serilog;
using Wddc.WebContentManager.Services.WebContent.Sales.LiquidationSale;
using Wddc.WebContentManager.Models.WebContent.Sales.LiquidationSale;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using PagedList;
using Wddc.WebContentManager.Models;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class LiquidationSaleController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly ILiquidationSaleService _liquidationSaleService;

        public LiquidationSaleController(ILiquidationSaleService liquidationSaleService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _liquidationSaleService = liquidationSaleService;
        }

        public async Task<ActionResult> IndexAsync(string response, string message)
        {
            Log.Logger.Information("The system is getting the list of liquidation sales from table dbo.Web_Liquidation_RET");
            List<Web_Liquidation_RET> webLiquidationSales = await _liquidationSaleService.GetAllWebLiquidationAsync();
            LiquidationSaleModel liquidationSaleModel = new LiquidationSaleModel
            {
                WebLiquidationRET = webLiquidationSales.OrderBy(_ => _.RET_Code)
            };

            ViewBag.response = response;
            ViewBag.Message = message;
            return View();
        }

        public async Task<JsonResult> GetWebLiquidationAsync()
        {
            Log.Logger.Information("The system is getting the list of liquidation sales from table dbo.Web_Liquidation_RET to populate the grid");
            List<Web_Liquidation_RET> items = await _liquidationSaleService.GetAllWebLiquidationAsync();
            return Json(items.OrderBy(_ => _.RET_Code));
        }

        public async Task<ActionResult> UpdateWebLiquidationAsync([DataSourceRequest] DataSourceRequest request, Web_Liquidation_RET model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Liquidation Sale ID: {@model.ID}");

            if (model.Expiry_DateString == "1969-12-31")
                model.Expiry_Date = null;

            var toUpdate = await _liquidationSaleService.GetWebLiquidationByIdAsync(model.ID);
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Liquidation Sale ID: {@model.ID} from: " + "{@toUpdate}", toUpdate);

            toUpdate.RET_Code = model.RET_Code;
            toUpdate.Description = model.Description;
            toUpdate.Reason = model.Reason;
            toUpdate.QOH = model.QOH;
            toUpdate.Expiry_Date = model.Expiry_Date;
            toUpdate.Regular_Cost = model.Regular_Cost;
            toUpdate.Liquidation_Cost = model.Liquidation_Cost;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Liquidation Sale ID: {@model.ID} to: " + "{@model}", model);

            try
            {
                await _liquidationSaleService.UpdateWebLiquidation(toUpdate, model.ID);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated Web Liquidation Sale successfully");
                _logger.Information($"Updated Web Liquidation Sale successfully. RET Code: {toUpdate.RET_Code}, REG Code: {toUpdate.REG_Code}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating Web Liquidation Sale by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating Web Liquidation Sale. RET Code: {toUpdate.RET_Code}, REG Code: {toUpdate.REG_Code} : {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> CreateWebLiquidationAsync([DataSourceRequest] DataSourceRequest request, Web_Liquidation_RET model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Liquidation Sale");

            if (model.Expiry_DateString == "1969-12-31")
                model.Expiry_Date = null;

            Web_Liquidation_RET newWeb_Liquidation_RET = new Web_Liquidation_RET();
            newWeb_Liquidation_RET.RET_Code = model.RET_Code;
            newWeb_Liquidation_RET.REG_Code = model.REG_Code;
            newWeb_Liquidation_RET.Description = model.Description;
            newWeb_Liquidation_RET.Reason = model.Reason;
            newWeb_Liquidation_RET.QOH = model.QOH;
            newWeb_Liquidation_RET.Expiry_Date = model.Expiry_Date;
            newWeb_Liquidation_RET.Regular_Cost = model.Regular_Cost;
            newWeb_Liquidation_RET.Liquidation_Cost = model.Liquidation_Cost;
            newWeb_Liquidation_RET.Date_Added = DateTime.Today;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Liquidation Sale: {@newWeb_Liquidation_RET}", newWeb_Liquidation_RET);

            try
            {
               newWeb_Liquidation_RET = await _liquidationSaleService.CreateWebLiquidation(newWeb_Liquidation_RET);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new Web Liquidation Sale with ID: {newWeb_Liquidation_RET.ID} successfully");
                _logger.Information($"Added new Web Liquidation Sale successfully. RET Code: {newWeb_Liquidation_RET.RET_Code}, REG Code: {newWeb_Liquidation_RET.REG_Code}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding Web Liquidation Sale by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding Web Liquidation Sale. RET Code: {newWeb_Liquidation_RET.RET_Code}, REG Code: {newWeb_Liquidation_RET.REG_Code} : {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> DeleteWebLiquidationAsync([DataSourceRequest] DataSourceRequest request, Web_Liquidation_RET model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting Web Liquidation Sale ID: {model.ID}");

            try
            {
                await _liquidationSaleService.DeleteWebLiquidation(model.ID);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted Web Liquidation Sale successfully");
                _logger.Information($"Deleted Web Liquidation Sale successfully. RET Code: {model.RET_Code}, REG Code: {model.REG_Code}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Web Liquidation Sale by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Web Liquidation Sale. RET Code: {model.RET_Code}, REG Code: {model.REG_Code} : {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        public async Task<JsonResult> GetItemInfoAsync(string itemNumber)
        {
            GetItemInfo_Result info = await _liquidationSaleService.GetItemInfo(itemNumber);
            return Json(info);
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
