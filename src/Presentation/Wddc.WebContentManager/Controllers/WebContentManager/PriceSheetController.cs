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

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class PriceSheetController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Services.Logging.ILogger _logger;
        private readonly INewsletterService _newsletterService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public PriceSheetController(INewsletterService newsletterService, Services.Logging.ILogger logger, IHostingEnvironment hostingEnvironment)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _newsletterService = newsletterService;
            _hostingEnvironment = hostingEnvironment;
        }

        public ActionResult Index(string response, string message)
        {
            ViewBag.response = response;
            ViewBag.Message = message;
            return View();
        }

        public async Task<JsonResult> GetPriceSheetsAsync()
        {
            Log.Logger.Information("The system is getting the price sheets");
            List<Web_News> results = await _newsletterService.GetPriceSheets();
            return Json(results.OrderByDescending(_ => _.Description));
        }

        public async Task<JsonResult> GetPriceSheetByIdAsync(int ID)
        {
            Web_News priceSheet = await _newsletterService.GetPriceSheetById(ID);

            string fileName = priceSheet.IssueNumber + ".pdf";
            string sourcePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Price Lists";
            string targetPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Price_Lists_Temp");

            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            System.IO.Directory.CreateDirectory(targetPath);

            System.IO.File.Copy(sourceFile, destFile, true);

            return Json(priceSheet);
        }


        [HttpPost]
        public async Task<ActionResult> AddPriceSheetAsync(string newDescription, IFormFile newInfoFileUrl, DateTime newIssueDate, string newStatus)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new price list sheet with description: " + newDescription);

            if (newInfoFileUrl.ContentType != "application/pdf")
                return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be PDF!" });

            if (newInfoFileUrl != null)
            {
                string priceListsPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Price Lists";

                try
                {
                    using (var stream = new FileStream(Path.Combine(priceListsPath, Path.GetFileName(newInfoFileUrl.FileName)), FileMode.Create))
                    {
                        await newInfoFileUrl.CopyToAsync(stream);
                    }

                    Web_News newWeb_News = new Web_News();
                    newWeb_News.Category = 3;
                    newWeb_News.IssueNumber = Path.GetFileNameWithoutExtension(newInfoFileUrl.FileName).Trim();
                    newWeb_News.IssueDate = newIssueDate;
                    newWeb_News.Description = newDescription.Trim();
                    newWeb_News.Status = (byte?)(newStatus == "yes" ? 1 : 0);

                    Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new price sheet: {@newWeb_News}", newWeb_News);

                    newWeb_News = await _newsletterService.CreatePriceSheet(newWeb_News);

                    Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new price sheet id: {newWeb_News.ID} successfully");
                    _logger.Information($"Added price sheet successfully: {newDescription}", null, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Success", message = "Price sheet was added successfully to the website! " });
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error adding price sheet, description: {newDescription} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error adding price sheet, description: {newDescription}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure adding price sheet: " + ex.Message.Substring(0, 300) });
                }
            }
            return RedirectToAction("Index", new { response = "Failure", message = "No pdf file was selected" });
        }

        [HttpPost]
        public async Task<ActionResult> UpdatePriceSheetAsync(short updatedPriceSheetId, string updatedDescription, IFormFile updatedInfoFileUrl, DateTime updatedIssueDate, string updatedStatus)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is updating the price sheet with ID: " + updatedPriceSheetId);

            if (updatedInfoFileUrl != null)
            {
                if (updatedInfoFileUrl.ContentType != "application/pdf")
                    return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be PDF!" });

                string priceListPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Price Lists";

                try
                {
                    using (var stream = new FileStream(Path.Combine(priceListPath, Path.GetFileName(updatedInfoFileUrl.FileName)), FileMode.Create))
                    {
                        await updatedInfoFileUrl.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error updating the pdf file of price sheet, description: {updatedDescription} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error updating pdf file of price sheet, description: {updatedDescription}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure updating pdf file of price sheet: " + ex.Message.Substring(0, 300) });
                }
            }

            Web_News toUpdatePriceSheet = await _newsletterService.GetWebFYINewsById(updatedPriceSheetId);

            toUpdatePriceSheet.Category = 3;
            toUpdatePriceSheet.IssueNumber = updatedInfoFileUrl == null ? toUpdatePriceSheet.IssueNumber : Path.GetFileNameWithoutExtension(updatedInfoFileUrl.FileName).Trim();
            toUpdatePriceSheet.IssueDate = updatedIssueDate;
            toUpdatePriceSheet.Description = updatedDescription.Trim();
            toUpdatePriceSheet.Status = (byte?)(updatedStatus == "yes" ? 1 : 0);

            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is updating price sheet: {@toUpdatePriceSheet}", toUpdatePriceSheet);

            try
            {
                await _newsletterService.UpdatePriceSheet(toUpdatePriceSheet, toUpdatePriceSheet.ID);

                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated price sheet, ID: {updatedPriceSheetId} successfully");
                _logger.Information($"Updated price sheet successfully: {updatedDescription}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Price sheet was updated successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating price sheet, ID: {updatedPriceSheetId} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating price sheet, description: {updatedDescription}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure updating price sheet: " + ex.Message.Substring(0, 300) });
            }

        }

        [HttpPost]
        public async Task<ActionResult> DeletePriceSheetAsync(short deletedPriceSheetId, string deletedDescription, DateTime deletedIssueDate, string deletedStatus)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is deleting the price sheet with ID: " + deletedPriceSheetId);

            Web_News toDeletePriceSheet = await _newsletterService.GetPriceSheetById(deletedPriceSheetId);

            string fileName = toDeletePriceSheet.IssueNumber.ToString() + ".pdf";
            string priceListPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Price Lists";
            string toDeleteFile = System.IO.Path.Combine(priceListPath, fileName);

            System.IO.File.Delete(toDeleteFile);

            fileName = toDeletePriceSheet.IssueNumber.ToString() + ".jpg";
            toDeleteFile = System.IO.Path.Combine(priceListPath, fileName);

            System.IO.File.Delete(toDeleteFile);

            try
            {
                await _newsletterService.DeletePriceSheet(toDeletePriceSheet.ID);

                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted price sheet Id: {toDeletePriceSheet.ID} successfully");
                _logger.Information($"Deleted price sheet successfully, description: {toDeletePriceSheet.Description}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Price sheet was deleted successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting price sheet, description: {toDeletePriceSheet.Description} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting price sheet, description: {toDeletePriceSheet.Description}: {ex.Message.Substring(0, 300)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure deleting price sheet: " + ex.Message.Substring(0, 300) });
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
