﻿using Microsoft.AspNetCore.Mvc;
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
    public class InsiderNewsletterController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Services.Logging.ILogger _logger;
        private readonly INewsletterService _newsletterService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public InsiderNewsletterController(INewsletterService newsletterService, Services.Logging.ILogger logger, IHostingEnvironment hostingEnvironment)
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

        public async Task<JsonResult> GetWebInsiderNewsAsync()
        {
            Log.Logger.Information("The system is getting web Insider newsletters");
            List<Web_News> results = await _newsletterService.GetWebInsiderNews();
            return Json(results.OrderByDescending(_ => _.IssueDate));
        }

        public async Task<JsonResult> GetWebInsiderNewsByIdAsync(int ID)
        {
            Web_News webInsiderNews = await _newsletterService.GetWebInsiderNewsById(ID);

            string fileName = webInsiderNews.IssueNumber + ".jpg";
            string sourcePath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Newsletter\\INSIDER";
            string targetPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Newsletter_Logos\\INSIDER");

            string sourceFile = Path.Combine(sourcePath, fileName);
            string destFile = Path.Combine(targetPath, fileName);

            Directory.CreateDirectory(targetPath);

            System.IO.File.Copy(sourceFile, destFile, true);

            return Json(webInsiderNews);
        }


        [HttpPost]
        public async Task<ActionResult> AddWebsiteNewsletterAsync(string newDescription, IFormFile newInfoFileUrl, DateTime newIssueDate, string newStatus)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Insider newsletter with description: " + newDescription);

            List<Web_News> webInsiderNews = await _newsletterService.GetWebInsiderNews();

            foreach(Web_News webInsider in webInsiderNews)
            {
                if (webInsider.IssueDate == newIssueDate)
                    return RedirectToAction("Index", new { response = "Failure", message = "Issue date enetered " + newIssueDate.ToString("dd/M/yyyy") + " already exists! You must choose a different date." });
            }

            if (newInfoFileUrl.ContentType != "application/pdf")
                return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be PDF!" });

            Web_News lastWebInsiderNews = await _newsletterService.GetLastWebInsiderNews();
            int newIssueNumber = Int32.Parse(lastWebInsiderNews.IssueNumber) + 1;

            if (newInfoFileUrl != null)
            {
                string inputFileName = Path.GetFileName(newInfoFileUrl.FileName);
                string insiderPdfFileName = newIssueNumber.ToString() + ".pdf";
                string insiderPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Newsletter\\INSIDER";

                try
                {
                    using (var stream = new FileStream(Path.Combine(insiderPath, insiderPdfFileName), FileMode.Create))
                    {
                        await newInfoFileUrl.CopyToAsync(stream);
                    }

                    string tempInputPath = Path.Combine(_hostingEnvironment.WebRootPath, "Website_Newsletter_Temp\\Files");
                    if (!Directory.Exists(tempInputPath))
                        Directory.CreateDirectory(tempInputPath);

                    string tempOutputPath = Path.Combine(_hostingEnvironment.WebRootPath, "Website_Newsletter_Temp\\SplitedFiles");
                    if (!Directory.Exists(tempOutputPath))
                        Directory.CreateDirectory(tempOutputPath);

                    using (FileStream stream = new FileStream(Path.Combine(tempInputPath, inputFileName), FileMode.Create))
                    {
                        await newInfoFileUrl.CopyToAsync(stream);
                    }

                    var firstPagePdfName = "";

                    using (iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(Path.Combine(tempInputPath, inputFileName)))
                    {
                        for (int pageNumber = 1; pageNumber <= pdfReader.NumberOfPages; pageNumber++)
                        {
                            string outputFileName = string.Format("{0}_{1}.pdf", Path.GetFileNameWithoutExtension(inputFileName), pageNumber);

                            if (pageNumber == 1)
                                firstPagePdfName = outputFileName;

                            iTextSharp.text.Document document = new iTextSharp.text.Document();
                            iTextSharp.text.pdf.PdfCopy pdfCopy = new iTextSharp.text.pdf.PdfCopy(document, new FileStream(Path.Combine(tempOutputPath, outputFileName), FileMode.Create));
                            document.Open();
                            pdfCopy.AddPage(pdfCopy.GetImportedPage(pdfReader, pageNumber));
                            document.Close();
                        }
                    }

                    string firstPagePdfPath = Path.Combine(tempOutputPath, firstPagePdfName);
                    var firstPagePdfFile = PdfDocument.FromFile(firstPagePdfPath);

                    string insiderJpgFileName = newIssueNumber.ToString() + ".jpg";
                    //firstPagePdfFile.RasterizeToImageFiles(Path.Combine(insiderPath, insiderJpgFileName), 255, 320, IronPdf.Imaging.ImageType.Default, 300);
                    firstPagePdfFile.RasterizeToImageFiles(Path.Combine(insiderPath, insiderJpgFileName), 239, 300, IronPdf.Imaging.ImageType.Default, 300);

                    Directory.Delete(tempInputPath, true);
                    Directory.Delete(tempOutputPath, true);

                    Web_News newWeb_News = new Web_News();
                    newWeb_News.Category = 2;
                    newWeb_News.IssueNumber = newIssueNumber.ToString();
                    newWeb_News.IssueDate = newIssueDate;
                    newWeb_News.Description = newDescription.Trim();
                    newWeb_News.Status = (byte?)(newStatus == "yes" ? 1 : 0);

                    Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Insider newsletter: {@newWeb_News}", newWeb_News);

                    newWeb_News = await _newsletterService.CreateWebInsiderNews(newWeb_News);

                    Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new Insider newsletter id: {newWeb_News.ID} successfully");
                    _logger.Information($"Added Insider newsletter successfully: {newDescription}", null, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Success", message = "Insider newsletter was added successfully to the website! " });
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error adding Insider newsletter with description : {newDescription} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error adding Insider newsletter: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure adding Insider newsletter: " + ex.Message.Substring(0, 200) });
                }
            }
            return RedirectToAction("Index", new { response = "Failure", message = "No pdf file was selected" });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateWebsiteNewsletterAsync(short updatedNewsletterId, string updatedDescription, IFormFile updatedInfoFileUrl, DateTime updatedIssueDate, string updatedStatus)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is updating the Insider newsletter with ID: " + updatedNewsletterId);

            List<Web_News> webInsiderNews = await _newsletterService.GetWebInsiderNews();

            foreach (Web_News webInsider in webInsiderNews)
            {
                if (webInsider.IssueDate == updatedIssueDate && webInsider.ID != updatedNewsletterId)
                    return RedirectToAction("Index", new { response = "Failure", message = "Issue date enetered " + updatedIssueDate.ToString("dd/M/yyyy") + " already exists! You must choose a different date." });
            }

            Web_News toUpdateWebInsiderNews = await _newsletterService.GetWebInsiderNewsById(updatedNewsletterId);

            int issueNumber = Int32.Parse(toUpdateWebInsiderNews.IssueNumber);

            if (updatedInfoFileUrl != null)
            {
                if (updatedInfoFileUrl.ContentType != "application/pdf")
                    return RedirectToAction("Index", new { response = "Failure", message = "File uploaded must be PDF!" });

                string inputFileName = Path.GetFileName(updatedInfoFileUrl.FileName);
                string insiderPdfFileName = issueNumber.ToString() + ".pdf";
                string insiderPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Newsletter\\INSIDER";

                try
                {
                    using (var stream = new FileStream(Path.Combine(insiderPath, insiderPdfFileName), FileMode.Create))
                    {
                        await updatedInfoFileUrl.CopyToAsync(stream);
                    }

                    string tempInputPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Website_Newsletter_Temp\\Files");
                    if (!Directory.Exists(tempInputPath))
                        Directory.CreateDirectory(tempInputPath);

                    string tempOutputPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Website_Newsletter_Temp\\SplitedFiles");
                    if (!Directory.Exists(tempOutputPath))
                        Directory.CreateDirectory(tempOutputPath);

                    using (FileStream stream = new FileStream(Path.Combine(tempInputPath, inputFileName), FileMode.Create))
                    {
                        await updatedInfoFileUrl.CopyToAsync(stream);
                    }

                    var firstPagePdfName = "";

                    using (iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(Path.Combine(tempInputPath, inputFileName)))
                    {
                        for (int pageNumber = 1; pageNumber <= pdfReader.NumberOfPages; pageNumber++)
                        {
                            string outputFileName = string.Format("{0}_{1}.pdf", System.IO.Path.GetFileNameWithoutExtension(inputFileName), pageNumber);

                            if (pageNumber == 1)
                                firstPagePdfName = outputFileName;

                            iTextSharp.text.Document document = new iTextSharp.text.Document();
                            iTextSharp.text.pdf.PdfCopy pdfCopy = new iTextSharp.text.pdf.PdfCopy(document, new FileStream(Path.Combine(tempOutputPath, outputFileName), FileMode.Create));
                            document.Open();
                            pdfCopy.AddPage(pdfCopy.GetImportedPage(pdfReader, pageNumber));
                            document.Close();
                        }
                    }

                    string firstPagePdfPath = Path.Combine(tempOutputPath, firstPagePdfName);
                    var firstPagePdfFile = PdfDocument.FromFile(firstPagePdfPath);

                    string insiderJpgFileName = issueNumber.ToString() + ".jpg";
                    //firstPagePdfFile.RasterizeToImageFiles(Path.Combine(insiderPath, insiderJpgFileName), 255, 320, IronPdf.Imaging.ImageType.Default, 300);
                    firstPagePdfFile.RasterizeToImageFiles(Path.Combine(insiderPath, insiderJpgFileName), 239, 300, IronPdf.Imaging.ImageType.Default, 300);

                    Directory.Delete(tempInputPath, true);
                    Directory.Delete(tempOutputPath, true);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error updating the pdf file of the Insider newsletter with description : {updatedDescription} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error updating pdf file of Insider newsletter: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure updating pdf file of Insider newsletter: " + ex.Message.Substring(0, 200) });
                }
            }

            toUpdateWebInsiderNews.Category = 2;
            toUpdateWebInsiderNews.IssueNumber = toUpdateWebInsiderNews.IssueNumber;
            toUpdateWebInsiderNews.IssueDate = updatedIssueDate;
            toUpdateWebInsiderNews.Description = updatedDescription.Trim();
            toUpdateWebInsiderNews.Status = (byte?)(updatedStatus == "yes" ? 1 : 0);

            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is updating the Insider newsletter: {@toUpdateWebInsiderNews}", toUpdateWebInsiderNews);
            
            try
            {
                await _newsletterService.UpdateWebInsiderNews(toUpdateWebInsiderNews, toUpdateWebInsiderNews.ID);

                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated the Insider newsletter id: {toUpdateWebInsiderNews.ID} successfully");
                _logger.Information($"Updated Insider newsletter successfully: {updatedDescription}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Insider newsletter was updated successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating the Insider newsletter with description : {updatedDescription} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating the Insider newsletter: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure updating Insider newsletter: " + ex.Message.Substring(0, 200) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteWebsiteNewsletterAsync(short deletedNewsletterId, string deletedDescription, DateTime deletedIssueDate, string deletedStatus)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is deleting the new Insider newsletter with ID: " + deletedNewsletterId);

            Web_News toDeleteWebInsiderNews = await _newsletterService.GetWebInsiderNewsById(deletedNewsletterId);

            string fileName = toDeleteWebInsiderNews.IssueNumber.ToString() + ".pdf";
            string insiderPath = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Newsletter\\INSIDER";
            string toDeleteFile = System.IO.Path.Combine(insiderPath, fileName);

            System.IO.File.Delete(toDeleteFile);

            fileName = toDeleteWebInsiderNews.IssueNumber.ToString() + ".jpg";
            toDeleteFile = Path.Combine(insiderPath, fileName);

            System.IO.File.Delete(toDeleteFile);

            try
            {
                await _newsletterService.DeleteWebInsiderNews(toDeleteWebInsiderNews.ID);

                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted the Insider newsletter id: {toDeleteWebInsiderNews.ID} successfully");
                _logger.Information($"Deleted Insider newsletter successfully: {toDeleteWebInsiderNews.Description}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Insider newsletter was deleted successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting the Insider newsletter with description : {toDeleteWebInsiderNews.Description} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting the Insider newsletter: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure deleting Insider newsletter: " + ex.Message.Substring(0, 200) });
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
