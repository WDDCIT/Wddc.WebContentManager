using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Kendo.Mvc.Extensions;
using Serilog;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using PagedList;
using Wddc.WebContentManager.Services.WebUser;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Wddc.Core.Domain.Webserver.Customers;

namespace Wddc.WebContentManager.Controllers.WebUser
{
    public class WebUserLookupController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly IWebUserLookupService _webUserLookupService;

        public WebUserLookupController(IWebUserLookupService webUserLookupService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _webUserLookupService = webUserLookupService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetMemberInfoAsync(string memberNbr)
        {
            List<GetWebAccess_Result> results = await _webUserLookupService.GetWebAccess(memberNbr);
            return Json(results.FirstOrDefault());
        }

        public async Task<JsonResult> GetWebAccessAsync(string memberNbr)
        {
            Log.Logger.Information("The system is getting web access records of member number: " + memberNbr);
            List<GetWebAccess_Result> results = await _webUserLookupService.GetWebAccess(memberNbr);
            return Json(results.OrderBy(_ => _.UserName));
        }

        public async Task<JsonResult> GetWebAccessMembersAsync()
        {
            Log.Logger.Information("The system is getting web access members");
            List<GetWebAccessMembers_Result> results = await _webUserLookupService.GetWebAccessMembers();
            return Json(results.OrderBy(_ => _.MemberNbr));
        }

        public async Task<ActionResult> AccountPreferencesPartialAsync(string memberNbr)
        {
            Log.Logger.Information("The system is getting contact info of member number: " + memberNbr);
            List<GetContactInfo_Result> results = await _webUserLookupService.GetContactInfo(memberNbr);
            if(results.Count == 0)
            {
                GetContactInfo_Result nullResults = new GetContactInfo_Result();
                return PartialView("_AccountPreferencesPartial", nullResults);
            }  
            else
            {
                return PartialView("_AccountPreferencesPartial", results.FirstOrDefault());
            }        
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
