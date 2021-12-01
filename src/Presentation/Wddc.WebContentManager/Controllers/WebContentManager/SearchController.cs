using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Wddc.WebContentManager.Services.WebContent.Search;
using Wddc.Core.Domain.Webserver.TICatalog;
using Serilog;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using PagedList;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class SearchController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _searchService = searchService;
        }

        public ActionResult Index(string response, string message)
        {
            ViewBag.response = response;
            ViewBag.Message = message;
            return View();
        }

        public async Task<JsonResult> GetAllTblSearchAsync()
        {
            Log.Logger.Information("The system is getting all records from table dbo.tblSearch to populate the grid");
            List<tblSearch> items = await _searchService.GetAllTblSearch();
            return Json(items.OrderBy(_ => _.SearchDescr));
        }

        public async Task<JsonResult> GetTblSearchBySearchDescrAsync(string SearchDescr)
        {
            Log.Logger.Information("The system is getting Search record from table dbo.tblSearch of SearchDescr: " + SearchDescr);
            tblSearch searchItem = null;
            List<tblSearch> items = await _searchService.GetAllTblSearch();
            if (items.Where(_ => _.SearchDescr.Trim() == SearchDescr.Trim()).Count() > 0)
                searchItem = await _searchService.GetTblSearchBySearchDescr(SearchDescr);
            return Json(searchItem);
        }

        public async Task<ActionResult> UpdateTblSearchAsync([DataSourceRequest] DataSourceRequest request, tblSearch model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating TblSearch of SearchDescr: {@model.SearchDescr}");

            var toUpdate = await _searchService.GetTblSearchBySearchDescr(model.SearchDescr);
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating TblSearch of SearchDescr: {@model.SearchDescr} from: " + "{@toUpdate}", toUpdate);
            toUpdate.SearchDescrModified = model.SearchDescrModified;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating TblSearch of SearchDescr: {@model.SearchDescr} to: " + "{@model}", model);

            try
            {
                await _searchService.UpdateTblSearch(toUpdate, model.SearchDescr);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated TblSearch successfully");
                _logger.Information($"Updated TblSearch successfully. SearchDescr: {toUpdate.SearchDescr}, SearchDescrModified: {toUpdate.SearchDescrModified}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating TblSearch by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating TblSearch. SearchDescr: {toUpdate.SearchDescr}, SearchDescrModified: {toUpdate.SearchDescrModified}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> CreateTblSearchAsync([DataSourceRequest] DataSourceRequest request, tblSearch model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new TblSearch record");

            tblSearch newtblSearch = new tblSearch();
            newtblSearch.SearchDescr = model.SearchDescr;
            newtblSearch.SearchDescrModified = model.SearchDescrModified;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new TblSearch: {@newtblSearch}", newtblSearch);

            try
            {
                newtblSearch = await _searchService.CreateTblSearch(newtblSearch);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new TblSearch with SearchDescr: {newtblSearch.SearchDescr} successfully");
                _logger.Information($"Added new TblSearch successfully. SearchDescr: {newtblSearch.SearchDescr}, SearchDescrModified: {newtblSearch.SearchDescrModified}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding new TblSearch by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding new TblSearch. SearchDescr: {newtblSearch.SearchDescr}, SearchDescrModified: {newtblSearch.SearchDescrModified}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
            }
        }

        [AcceptVerbs("Post")]
        public async Task<ActionResult> DeleteTblSearchAsync([DataSourceRequest] DataSourceRequest request, tblSearch model)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting TblSearch of SearchDescr: {@model.SearchDescr}");

            try
            {
                await _searchService.DeleteTblSearch(model.SearchDescr);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted TblSearch record successfully");
                _logger.Information($"Deleted TblSearch successfully. SearchDescr: {model.SearchDescr}, SearchDescrModified: {model.SearchDescrModified}", null, User, "WebOrdering");
                return Json(model);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting TblSearch by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting TblSearch. SearchDescr: {model.SearchDescr}, SearchDescrModified: {model.SearchDescrModified}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(model);
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
