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
using Wddc.WebContentManager.Services.WebContent.Newsletter;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class FYINewsletterController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Services.Logging.ILogger _logger;
        private readonly INewsletterService _newsletterService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public FYINewsletterController(INewsletterService newsletterService, Services.Logging.ILogger logger, IHostingEnvironment hostingEnvironment)
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

        public async Task<JsonResult> GetWebFYINewsAsync()
        {
            Log.Logger.Information("The system is getting web fyi newsletters");
            List<Web_News> results = await _newsletterService.GetWebFYINews();
            return Json(results.OrderByDescending(_ => _.IssueDate));
        }

        public async Task<JsonResult> GetWebFYINewsByIdAsync(int ID)
        {
            Web_News webFYINews = await _newsletterService.GetWebFYINewsById(ID);

            string fileName = webFYINews.IssueNumber + ".jpg";
            string sourcePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Newsletter\\FYI";
            string targetPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Newsletter_Logos\\FYI");

            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            System.IO.Directory.CreateDirectory(targetPath);

            System.IO.File.Copy(sourceFile, destFile, true);

            return Json(webFYINews);
        }


        [HttpPost]
        public async Task<ActionResult> AddWebsiteNewsletterAsync(string newDescription, IFormFile newInfoFileUrl, DateTime newIssueDate, string newStatus)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new FYI newsletter with description: " + newDescription);

            List<Web_News> webFYINews = await _newsletterService.GetWebFYINews();

            foreach (Web_News webFY in webFYINews)
            {
                if (webFY.IssueDate == newIssueDate)
                    return RedirectToAction("Index", new { response = "Failure", message = "Issue date enetered " + newIssueDate.ToString("dd/M/yyyy") + " already exists! You must choose a different date." });
            }

            if (newInfoFileUrl.ContentType != "application/pdf")
                return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be PDF!" });

            Web_News lastWebFYINews = await _newsletterService.GetLastWebFYINews();
            int newIssueNumber = Int32.Parse(lastWebFYINews.IssueNumber) + 1;

            if (newInfoFileUrl != null)
            {
                string inputFileName = Path.GetFileName(newInfoFileUrl.FileName);
                string fyiPdfFileName = newIssueNumber.ToString() + ".pdf";
                string fyiPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Newsletter\\FYI";

                string tempInputPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Website_Newsletter_Temp\\Files");
                if (!Directory.Exists(tempInputPath))
                    Directory.CreateDirectory(tempInputPath);

                string tempOutputPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Website_Newsletter_Temp\\SplitedFiles");
                if (!Directory.Exists(tempOutputPath))
                    Directory.CreateDirectory(tempOutputPath);

                string tempFilePath = Path.Combine(tempInputPath, inputFileName);

                try
                {
                    // Save uploaded file to temp location
                    using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await newInfoFileUrl.CopyToAsync(stream);
                        await stream.FlushAsync();
                    }

                    // Copy to final destination
                    System.IO.File.Copy(tempFilePath, Path.Combine(fyiPath, fyiPdfFileName), true);

                    // Use IronPDF to load and extract first page
                    var pdfDoc = IronPdf.PdfDocument.FromFile(tempFilePath);

                    // Extract first page only
                    var firstPage = pdfDoc.CopyPage(0);
                    string firstPagePdfPath = Path.Combine(tempOutputPath, Path.GetFileNameWithoutExtension(inputFileName) + "_1.pdf");
                    firstPage.SaveAs(firstPagePdfPath);

                    // Generate JPG from first page
                    string fyiJpgFileName = newIssueNumber.ToString() + ".jpg";
                    firstPage.RasterizeToImageFiles(Path.Combine(fyiPath, fyiJpgFileName), 255, 320, IronPdf.Imaging.ImageType.Default, 300);

                    // Cleanup
                    pdfDoc.Dispose();
                    firstPage.Dispose();
                    Directory.Delete(tempInputPath, true);
                    Directory.Delete(tempOutputPath, true);

                    Web_News newWeb_News = new Web_News();
                    newWeb_News.Category = 1;
                    newWeb_News.IssueNumber = newIssueNumber.ToString();
                    newWeb_News.IssueDate = newIssueDate;
                    newWeb_News.Description = newDescription.Trim();
                    newWeb_News.Status = (byte?)(newStatus == "yes" ? 1 : 0);

                    Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new FYI newsletter: {@newWeb_News}", newWeb_News);

                    newWeb_News = await _newsletterService.CreateWebFYINews(newWeb_News);

                    Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new FYI newsletter id: {newWeb_News.ID} successfully");
                    _logger.Information($"Added FYI newsletter successfully: {newDescription}", null, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Success", message = "FYI newsletter was added successfully to the website! " });
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error adding FYI newsletter with description : {newDescription} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error adding FYI newsletter: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure adding FYI newsletter: " + ex.Message.Substring(0, 200) });
                }
            }
            return RedirectToAction("Index", new { response = "Failure", message = "No pdf file was selected" });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateWebsiteNewsletterAsync(short updatedNewsletterId, string updatedDescription, IFormFile updatedInfoFileUrl, DateTime updatedIssueDate, string updatedStatus)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is updating the FYI newsletter with ID: " + updatedNewsletterId);

            List<Web_News> webFYINews = await _newsletterService.GetWebFYINews();

            foreach (Web_News webFY in webFYINews)
            {
                if (webFY.IssueDate == updatedIssueDate && webFY.ID != updatedNewsletterId)
                    return RedirectToAction("Index", new { response = "Failure", message = "Issue date enetered " + updatedIssueDate.ToString("dd/M/yyyy") + " already exists! You must choose a different date." });
            }

            Web_News toUpdateWebFYINews = await _newsletterService.GetWebFYINewsById(updatedNewsletterId);

            int issueNumber = Int32.Parse(toUpdateWebFYINews.IssueNumber);

            if (updatedInfoFileUrl != null)
            {
                if (updatedInfoFileUrl.ContentType != "application/pdf")
                    return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be PDF!" });

                string inputFileName = Path.GetFileName(updatedInfoFileUrl.FileName);
                string fyiPdfFileName = issueNumber.ToString() + ".pdf";
                string fyiPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Newsletter\\FYI";

                string tempInputPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Website_Newsletter_Temp\\Files");
                if (!Directory.Exists(tempInputPath))
                    Directory.CreateDirectory(tempInputPath);

                string tempOutputPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Website_Newsletter_Temp\\SplitedFiles");
                if (!Directory.Exists(tempOutputPath))
                    Directory.CreateDirectory(tempOutputPath);

                string tempFilePath = Path.Combine(tempInputPath, inputFileName);

                try
                {
                    // Save uploaded file to temp location
                    using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await updatedInfoFileUrl.CopyToAsync(stream);
                        await stream.FlushAsync();
                    }

                    // Copy to final destination
                    System.IO.File.Copy(tempFilePath, Path.Combine(fyiPath, fyiPdfFileName), true);

                    // Use IronPDF to load and extract first page
                    var pdfDoc = IronPdf.PdfDocument.FromFile(tempFilePath);

                    // Extract first page only
                    var firstPage = pdfDoc.CopyPage(0);
                    string firstPagePdfPath = Path.Combine(tempOutputPath, Path.GetFileNameWithoutExtension(inputFileName) + "_1.pdf");
                    firstPage.SaveAs(firstPagePdfPath);

                    // Generate JPG from first page
                    string fyiJpgFileName = issueNumber.ToString() + ".jpg";
                    firstPage.RasterizeToImageFiles(Path.Combine(fyiPath, fyiJpgFileName), 255, 320, IronPdf.Imaging.ImageType.Default, 300);

                    // Cleanup
                    pdfDoc.Dispose();
                    firstPage.Dispose();
                    Directory.Delete(tempInputPath, true);
                    Directory.Delete(tempOutputPath, true);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error updating the pdf file of the FYI newsletter with description : {updatedDescription} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error updating pdf file of FYI newsletter: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure updating pdf file of FYI newsletter: " + ex.Message.Substring(0, 200) });
                }
            }

            toUpdateWebFYINews.Category = 1;
            toUpdateWebFYINews.IssueNumber = toUpdateWebFYINews.IssueNumber;
            toUpdateWebFYINews.IssueDate = updatedIssueDate;
            toUpdateWebFYINews.Description = updatedDescription.Trim();
            toUpdateWebFYINews.Status = (byte?)(updatedStatus == "yes" ? 1 : 0);

            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is updating the FYI newsletter: {@toUpdateWebFYINews}", toUpdateWebFYINews);

            try
            {
                await _newsletterService.UpdateWebFYINews(toUpdateWebFYINews, toUpdateWebFYINews.ID);

                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated the FYI newsletter id: {toUpdateWebFYINews.ID} successfully");
                _logger.Information($"Updated FYI newsletter successfully: {updatedDescription}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "FYI newsletter was updated successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating the FYI newsletter with description : {updatedDescription} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating the FYI newsletter: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure updating FYI newsletter: " + ex.Message.Substring(0, 200) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteWebsiteNewsletterAsync(short deletedNewsletterId, string deletedDescription, DateTime deletedIssueDate, string deletedStatus)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is deleting the new FYI newsletter with ID: " + deletedNewsletterId);

            Web_News toDeleteWebFYINews = await _newsletterService.GetWebFYINewsById(deletedNewsletterId);

            string fileName = toDeleteWebFYINews.IssueNumber.ToString() + ".pdf";
            string fyiPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Newsletter\\FYI";
            string toDeleteFile = System.IO.Path.Combine(fyiPath, fileName);

            System.IO.File.Delete(toDeleteFile);

            fileName = toDeleteWebFYINews.IssueNumber.ToString() + ".jpg";
            toDeleteFile = System.IO.Path.Combine(fyiPath, fileName);

            System.IO.File.Delete(toDeleteFile);

            try
            {
                await _newsletterService.DeleteWebFYINews(toDeleteWebFYINews.ID);

                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted the FYI newsletter id: {toDeleteWebFYINews.ID} successfully");
                _logger.Information($"Deleted FYI newsletter successfully: {toDeleteWebFYINews.Description}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "FYI newsletter was deleted successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting the FYI newsletter with description : {toDeleteWebFYINews.Description} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting the FYI newsletter: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure deleting FYI newsletter: " + ex.Message.Substring(0, 200) });
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