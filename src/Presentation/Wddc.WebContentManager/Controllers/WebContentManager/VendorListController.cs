using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Serilog;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using PagedList;
using Wddc.WebContentManager.Models;
using Wddc.WebContentManager.Services.WebContent.Vendors;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class VendorListController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly IVendorListService _vendorListService;

        public VendorListController(IVendorListService vendorListService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _vendorListService = vendorListService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetWebVendorListAsync()
        {
            List<Web_VendorList> vendors = await _vendorListService.GetWebVendorList();
            return Json(vendors.OrderBy(_ => _.Vendor_ID));
        }

        public async Task<ActionResult> UpdateWebVendorAsync([DataSourceRequest] DataSourceRequest request, Web_VendorList model)
        {
            if (String.IsNullOrEmpty(model.Vendor_ID))
            {
                Log.Logger.Error($"Error updating vendor by {User.Identity.Name.Substring(7).ToLower()}: Vendor Id is required");
                _logger.Error($"Error adding vendor. Vendor Id is required", null, User, "WebOrdering");
                return Json(0);
            }

            if (String.IsNullOrEmpty(model.Vendor_Short_Name))
            {
                Log.Logger.Error($"Error updating vendor by {User.Identity.Name.Substring(7).ToLower()}: Vendor Short Name is required");
                _logger.Error($"Error adding vendor. VendorId: {model.Vendor_ID}, Vendor Short Name is required", null, User, "WebOrdering");
                return Json(0);
            }

            if (String.IsNullOrEmpty(model.Vendor_Name))
            {
                Log.Logger.Error($"Error updating vendor by {User.Identity.Name.Substring(7).ToLower()}: Vendor Name is required");
                _logger.Error($"Error adding vendor. VendorId: {model.Vendor_ID}, Vendor Name is required", null, User, "WebOrdering");
                return Json(0);
            }

            var toUpdate = await _vendorListService.GetWebVendorById(model.Vendor_ID);

            toUpdate.Vendor_Short_Name = model.Vendor_Short_Name;
            toUpdate.Vendor_Name = model.Vendor_Name;
            toUpdate.Vendor_Email = model.Vendor_Email;
            toUpdate.Vendor_CCEmail = model.Vendor_CCEmail;

            try
            {
                toUpdate = await _vendorListService.UpdateWebVendor(toUpdate, model.Vendor_ID);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated vendor successfully");
                _logger.Information($"Updated vendor successfully. Vendor Id: {model.Vendor_ID}, Short Name: {model.Vendor_Short_Name}", null, User, "WebOrdering");
                return Json(model.Vendor_Short_Name);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating vendor by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating vendor. Vendor Id: {model.Vendor_ID}, Short Name: {model.Vendor_Short_Name} : {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(0);
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> CreateWebVendorAsync([DataSourceRequest] DataSourceRequest request, Web_VendorList model)
        {
            if (String.IsNullOrEmpty(model.Vendor_ID))
            {
                Log.Logger.Error($"Error adding vendor by {User.Identity.Name.Substring(7).ToLower()}: Vendor Id is required");
                _logger.Error($"Error adding vendor. Vendor Id is required", null, User, "WebOrdering");
                return Json(0);
            }

            if (String.IsNullOrEmpty(model.Vendor_Short_Name))
            {
                Log.Logger.Error($"Error adding vendor by {User.Identity.Name.Substring(7).ToLower()}: Vendor Short Name is required");
                _logger.Error($"Error adding vendor. VendorId: {model.Vendor_ID}, Vendor Short Name is required", null, User, "WebOrdering");
                return Json(0);
            }

            if (String.IsNullOrEmpty(model.Vendor_Name))
            {
                Log.Logger.Error($"Error adding vendor by {User.Identity.Name.Substring(7).ToLower()}: Vendor Name is required");
                _logger.Error($"Error adding vendor. VendorId: {model.Vendor_ID}, Vendor Name is required", null, User, "WebOrdering");
                return Json(0);
            }

            Web_VendorList newVendor = new Web_VendorList();
            newVendor.Vendor_ID = model.Vendor_ID;
            newVendor.Vendor_Short_Name = model.Vendor_Short_Name;
            newVendor.Vendor_Name = model.Vendor_Name;
            newVendor.Vendor_Email = model.Vendor_Email;
            newVendor.Vendor_CCEmail = model.Vendor_CCEmail;

            try
            {
                newVendor = await _vendorListService.CreateWebVendor(newVendor);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new vendor with ID: {newVendor.Vendor_ID} successfully");
                _logger.Information($"Added vendor successfully. Vendor Id: {newVendor.Vendor_ID}, Short Name: {newVendor.Vendor_Short_Name}", null, User, "WebOrdering");
                return Json(newVendor.Vendor_Short_Name);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding new vendor by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding vendor. Vendor Id: {newVendor.Vendor_ID}, Short Name: {newVendor.Vendor_Short_Name} : {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(0);
            }

        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> DeleteWebVendorAsync([DataSourceRequest] DataSourceRequest request, Web_VendorList model)
        {
            if (String.IsNullOrEmpty(model.Vendor_ID))
            {
                Log.Logger.Error($"Error deleting vendor by {User.Identity.Name.Substring(7).ToLower()}: Vendor Id is required");
                _logger.Error($"Error deleting vendor. Vendor Id is required", null, User, "WebOrdering");
                return Json(0);
            }

            try
            {
                await _vendorListService.DeleteWebVendor(model.Vendor_ID);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted vendor Id: {model.Vendor_ID} successfully");
                _logger.Information($"Deleted vendor successfully. Vendor Id: {model.Vendor_ID}, Short Name: {model.Vendor_Short_Name}", null, User, "WebOrdering");
                return Json(model.Vendor_Short_Name);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting vendor by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting vendor. Vendor Id: {model.Vendor_ID}, Short Name: {model.Vendor_Short_Name} : {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(0);
            }
        }



        public async Task<ActionResult> LogAsync(int? pageNumber, int pageSize = 5, string referrerUrl = null)
        {
            IPagedList<WebOrderingLog> results = await _logger.GetAllWebOrderingLogsAsync(referrerUrl: referrerUrl, pageSize: 10, pageIndex: pageNumber ?? 1);

            var ajaxResult = new AjaxResults()
            {
                ResultStatus = ResultStatus.Success,
                Results = results
                    .Select(l => new { Text = l.ShortMessage.Length > 200 ? l.ShortMessage.Substring(0, 200) : l.ShortMessage, l.User, Created = l.CreatedOnUtc.ToLocalTime().ToString() }),
                TotalItemCount = results.TotalItemCount,
            };
            return Json(ajaxResult);
        }

    }
}
