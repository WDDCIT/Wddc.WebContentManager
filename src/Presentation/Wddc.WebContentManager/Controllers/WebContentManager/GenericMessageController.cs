using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Http;
using System.IO;
using PagedList;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using static System.Net.WebRequestMethods;
using Wddc.WebContentManager.Services.WebContent.GenericMessage;
using Wddc.Core.Domain.Media;
using System.Globalization;
using Wddc.Core.Domain.Webserver.WebOrdering;
using System.Reflection;
using EdiFabric.Templates.X12004010;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class GenericMessageController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Services.Logging.ILogger _logger;
        private readonly IGenericMessageService _genericMessageService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public GenericMessageController(IGenericMessageService genericMessageService, Services.Logging.ILogger logger, IHostingEnvironment hostingEnvironment)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _genericMessageService = genericMessageService;
            _hostingEnvironment = hostingEnvironment;
        }

        public ActionResult Index(string response, string message)
        {
            ViewBag.response = response;
            ViewBag.Message = message;
            return View();
        }

        public async Task<JsonResult> GetMessagesAsync()
        {
            List<Message_Generic> messages = await _genericMessageService.GetGenericMessages();

            return Json(messages.Where(_ => _.Status == 0).OrderBy(_ => _.CreateDate));
        }

        public async Task<JsonResult> GetMessageByIdAsync(int Id)
        {
            Message_Generic message = await _genericMessageService.GetGenericMessageById(Id);

            string sourcePath = "\\\\WEBSRVR\\WDDCWebPages\\wddc_members\\CS\\Ads";
            string destinationPath = Path.Combine(this._hostingEnvironment.WebRootPath, "Message_Generic_Temp");


            if (Directory.Exists(destinationPath))
                Directory.Delete(destinationPath, true);

            Directory.CreateDirectory(destinationPath);

            FileInfo pdfFile = new FileInfo(sourcePath + "\\" + message.FileName.Trim());

            if (pdfFile.Exists && pdfFile.Extension != ".db")
            {
                pdfFile.CopyTo(Path.Combine(destinationPath, message.FileName.Trim()), true);
            }

            return Json(message);
        }



        [HttpPost]
        public async Task<ActionResult> CreateMessageAsync(string subject, IFormFile pdfFile, DateTime expiryDate, int site, bool priority)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return RedirectToAction("Index", new { response = "Failure", message = "Message subject must be filled out!" });
            }

            if (pdfFile == null)
            {
                return RedirectToAction("Index", new { response = "Failure", message = "You must select a pdf file to upload!" });
            }

            if (expiryDate < DateTime.Today)
            {
                return RedirectToAction("Index", new { response = "Failure", message = "Expiry date is invalid!" });
            }

            if (site == -1)
                site = 0;

            string path = "\\\\WEBsrvr\\WDDCWebPages\\wddc_members\\CS\\Ads";
            string fileName = pdfFile.FileName;
            string pdfFilePath = path + "\\" + fileName;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            try
            {
                while (System.IO.File.Exists(pdfFilePath))
                {
                    fileName = Path.ChangeExtension(Path.GetRandomFileName(), "pdf");
                    pdfFilePath = path + "\\" + fileName;
                }

                using (var stream = new FileStream(pdfFilePath, FileMode.Create))
                {
                    await pdfFile.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error uploading generic message pdf file: {fileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error uploading generic message pdf file: {fileName}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure uploading generic message pdf file: " + fileName + ": " + ex.Message.Substring(0, 200) });
            }

            Message_Generic genericMessage = new Message_Generic();
            genericMessage.Subject = subject.Trim();
            genericMessage.GenericBody = null;
            genericMessage.FileName = fileName;
            genericMessage.CreateDate = DateTime.Now;
            genericMessage.ExpiryDate = expiryDate;
            genericMessage.Priority = (byte)(priority ? 1 : 0);
            genericMessage.Location = (short)site;
            genericMessage.Status = 0;

            try
            {
                genericMessage = await _genericMessageService.AddGenericMessage(genericMessage);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error inserting Message_Generic, subject: {subject} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error inserting Message_Generic, subject: {subject}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure inserting Message_Generic, subject:: " + subject + ": " + ex.Message.Substring(0, 200) });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} created generic message: {subject} successfully");
            _logger.Information($"Created generic message: {subject}, successfully", null, User, "WebOrdering");
            return RedirectToAction("Index", new { response = "Success", message = "Generic message: " + subject + " was created successfully! " });
        }

        [HttpPost]
        public async Task<ActionResult> UpdateMessageAsync(int messageId, string subject, IFormFile pdfFile, DateTime expiryDate, int site, bool priority)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return RedirectToAction("Index", new { response = "Failure", message = "Message subject must be filled out!" });
            }

            if (site == -1)
                site = 0;

            Message_Generic message = await _genericMessageService.GetGenericMessageById(messageId);

            if (pdfFile != null)
            {
                string path = "\\\\WEBsrvr\\WDDCWebPages\\wddc_members\\CS\\Ads";
                string fileName = pdfFile.FileName;
                string pdfFilePath = Path.Combine(@path, fileName);
                string pdfFileOldPath = Path.Combine(@path, message.FileName);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                FileInfo pdfFileOld = new FileInfo(pdfFileOldPath);

                try
                {
                    if (pdfFileOld.Exists)
                        pdfFileOld.Delete();
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error deleting old message pdf file {pdfFileOldPath} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error deleting old message pdf file {pdfFileOldPath}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure deleting old message pdf file: " + pdfFileOldPath + ": " + ex.Message.Substring(0, 200) });
                }

                try
                {
                    while (System.IO.File.Exists(pdfFilePath))
                    {
                        fileName = Path.ChangeExtension(Path.GetRandomFileName(), "pdf");
                        pdfFilePath = path + "\\" + fileName;
                    }

                    using (var stream = new FileStream(pdfFilePath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(stream);
                        message.FileName = fileName;
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error uploading generic message pdf file: {fileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                    _logger.Error($"Error uploading generic message pdf file: {fileName}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                    return RedirectToAction("Index", new { response = "Failure", message = "Failure uploading generic message pdf file: " + fileName + ": " + ex.Message.Substring(0, 200) });
                }
            }

            message.Subject = subject.Trim();
            message.GenericBody = null;
            message.CreateDate = message.CreateDate;
            message.ExpiryDate = expiryDate;
            message.Priority = (byte)(priority ? 1 : 0);
            message.Location = (short)site;
            message.Status = message.Status;

            try
            {
                await _genericMessageService.UpdateGenericMessage(message, messageId);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating Message_Generic, subject: {subject} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating Message_Generic, subject: {subject}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure updating Message_Generic, subject:: " + subject + ": " + ex.Message.Substring(0, 200) });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} edited generic message: {subject} successfully");
            _logger.Information($"Edited generic message: {subject}, successfully", null, User, "WebOrdering");
            return RedirectToAction("Index", new { response = "Success", message = "Generic message: " + subject + " was edited successfully! " });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteMessageAsync(int messageId)
        {
            Message_Generic message = await _genericMessageService.GetGenericMessageById(messageId);

            message.Subject = message.Subject;
            message.GenericBody = message.GenericBody;
            message.CreateDate = message.CreateDate;
            message.ExpiryDate = message.ExpiryDate;
            message.Priority = message.Priority;
            message.Location = message.Location;
            message.Status = 1;

            try
            {
                await _genericMessageService.UpdateGenericMessage(message, messageId);
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Message_Generic, subject: {message.Subject} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Message_Generic, subject: {message.Subject}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure deleting Message_Generic, subject:: " + message.Subject.Substring(0, 200) + ": " + ex.Message });
            }

            Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted generic message: {message.Subject} successfully");
            _logger.Information($"Deleted generic message: {message.Subject}, successfully", null, User, "WebOrdering");
            return RedirectToAction("Index", new { response = "Success", message = "Generic message: " + message.Subject + " was deleted successfully! " });
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
