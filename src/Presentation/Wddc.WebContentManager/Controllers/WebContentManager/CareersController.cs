using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.WebContentManager.Models.WebContent.Careers;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Serilog;
using Wddc.WebContentManager.Services.WebContent.Careers;
using Microsoft.AspNetCore.Http;
using System.IO;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using PagedList;
using System.Web;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class CareersController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly ICareersService _careersService;

        public CareersController(ICareersService careersService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _careersService = careersService;
        }

        public async Task<ActionResult> IndexAsync(string response, string message)
        {
            Log.Logger.Information("The system is getting the list of Web Careers from table dbo.Web_Careers");
            List<Web_Careers> webCareers = await _careersService.GetAllWebCareers();
            WebCareersModel webCareersModel = new WebCareersModel
            {
                WebCareers = webCareers.OrderBy(_ => _.MemberNbr),
                WebCareersActive = webCareers.Where(_ => _.End_Date >= DateTime.Today).OrderBy(_ => _.MemberNbr).ThenBy(_ => _.Short_Description),
                WebCareersExpired = webCareers.Where(_ => _.End_Date < DateTime.Today).OrderBy(_ => _.MemberNbr).ThenBy(_ => _.Short_Description)
            };

            ViewBag.response = response;
            ViewBag.Message = message;
            return View(webCareersModel);
        }

        public async Task<ActionResult> CareersEditorPartialAsync(int adId)
        {
            Log.Logger.Information("The system is getting the Web Careers from table dbo.Web_Careers of Ad ID: " + adId);
            var result = await _careersService.GetWebCareerByIdAsync(adId);
            return PartialView("_CareersEditorPartial", result);
        }

        public async Task<JsonResult> GetFirstActiveAdAsync()
        {
            Log.Logger.Information("The system is getting the first active Web Careers ad from table dbo.Web_Careers");
            List<Web_Careers> webCareers = await _careersService.GetAllWebCareers();
            Web_Careers result = webCareers.Where(_ => _.End_Date >= DateTime.Today).OrderBy(_ => _.MemberNbr).ThenBy(_ => _.Short_Description).First();
            return Json(result);
        }

        public async Task<JsonResult> GetFirstExpiredAdAsync()
        {
            Log.Logger.Information("The system is getting the first expired Web Careers ad from table dbo.Web_Careers");
            List<Web_Careers> webCareers = await _careersService.GetAllWebCareers();
            Web_Careers result = webCareers.Where(_ => _.End_Date < DateTime.Today).OrderBy(_ => _.MemberNbr).ThenBy(_ => _.Short_Description).First();
            return Json(result);
        }

        public async Task<string> GetMemberNameAsync(string memberNbr)
        {
            Log.Logger.Information("The system is getting the member name of member number: " + memberNbr);
            if (memberNbr == null || memberNbr.Trim().Length == 0)
                return "";
            return await _careersService.GetMemberName(memberNbr);
        }

        [HttpPost]
        public async Task<ActionResult> AddCareerAsync(string newShortDescription, string newLongDescription, string newApplicationInstructions, string newMemberNbr,
            string newContactName, string newContactTitle, string newContactPhone, string newContactFax, string newContactEmail, IFormFile newInfoFileUrl, 
            string newInfoFileUrlText, DateTime? newStartDate, DateTime? newEndDate)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Career ad");

            if (newInfoFileUrl != null)
            {
                var path = "\\\\WEBSRVR\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Careers";
                path = Path.Combine(
                  path, newInfoFileUrl.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await newInfoFileUrl.CopyToAsync(stream);
                }
            }

            //System.Text.RegularExpressions.Regex rxLeft = new System.Text.RegularExpressions.Regex("&lt;");
            //System.Text.RegularExpressions.Regex rxRight = new System.Text.RegularExpressions.Regex("&gt;");
            //newLongDescription = rxLeft.Replace(newLongDescription, "<");
            //newLongDescription = rxRight.Replace(newLongDescription, ">");

            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(newLongDescription, myWriter);
            newLongDescription = myWriter.ToString();

            Web_Careers newWeb_Careers = new Web_Careers();
            newWeb_Careers.Start_Date = newStartDate;
            newWeb_Careers.End_Date = newEndDate;
            newWeb_Careers.MemberNbr = newMemberNbr == null ? "" : newMemberNbr;
            newWeb_Careers.Long_Description = newLongDescription == null ? "" : newLongDescription;
            newWeb_Careers.Short_Description = newShortDescription == null ? "" : newShortDescription;
            newWeb_Careers.Application_Instructions = newApplicationInstructions == null ? "" : newApplicationInstructions;
            newWeb_Careers.Contact_Name = newContactName == null ? "" : newContactName;
            newWeb_Careers.Contact_Title = newContactTitle == null ? "" : newContactTitle;
            newWeb_Careers.Contact_Phone = newContactPhone == null ? "" : newContactPhone;
            newWeb_Careers.Contact_Email = newContactEmail == null ? "" : newContactEmail;
            newWeb_Careers.Contact_Fax = newContactFax == null ? "" : newContactFax;
            newWeb_Careers.InfoFileURL = newInfoFileUrlText == null ? "" : newInfoFileUrlText;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Career Ad: {@newWeb_Careers}", newWeb_Careers);

            try
            {
                newWeb_Careers = await _careersService.CreateWebCareer(newWeb_Careers);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new Web Career Ad ID: {newWeb_Careers.Ad_ID} successfully");
                _logger.Information($"Added new Web Classified Ad successfully. Short Description: {newWeb_Careers.Short_Description}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Wddc web career was added successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding Web Career Ad by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding Web Career Ad. Short Description: {newWeb_Careers.Short_Description}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure adding Wddc web career: " + ex.Message.Substring(0, 100) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCareerAsync(int adId, string shortDescription, string longDescription, string applicationInstructions, string memberNbr,
            string contactName, string contactTitle, string contactPhone, string contactFax, string contactEmail, IFormFile infoFileUrl, string infoFileUrlText,
            DateTime? startDate, DateTime? endDate)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Career Ad ID: {adId}");

            if (infoFileUrl != null)
            {
                var path = "\\\\WEBSRVR\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Careers";
                path = Path.Combine(
                  path, infoFileUrl.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await infoFileUrl.CopyToAsync(stream);
                }
            }

            //System.Text.RegularExpressions.Regex rxLeft = new System.Text.RegularExpressions.Regex("&lt;");
            //System.Text.RegularExpressions.Regex rxRight = new System.Text.RegularExpressions.Regex("&gt;");
            //longDescription = rxLeft.Replace(longDescription, "<");
            //longDescription = rxRight.Replace(longDescription, ">");

            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(longDescription, myWriter);
            longDescription = myWriter.ToString();

            var toUpdateAd = await _careersService.GetWebCareerByIdAsync(adId);
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Career Ad ID: {adId} from: " + "{@toUpdateAd}", toUpdateAd);

            toUpdateAd.Start_Date = startDate;
            toUpdateAd.End_Date = endDate;
            toUpdateAd.MemberNbr = memberNbr == null ? "" : memberNbr;
            toUpdateAd.Long_Description = longDescription == null ? "" : longDescription;
            toUpdateAd.Short_Description = shortDescription == null ? "" : shortDescription;
            toUpdateAd.Application_Instructions = applicationInstructions == null ? "" : applicationInstructions;
            toUpdateAd.Contact_Name = contactName == null ? "" : contactName;
            toUpdateAd.Contact_Title = contactTitle == null ? "" : contactTitle;
            toUpdateAd.Contact_Phone = contactPhone == null ? "" : contactPhone;
            toUpdateAd.Contact_Email = contactEmail == null ? "" : contactEmail;
            toUpdateAd.Contact_Fax = contactFax == null ? "" : contactFax;
            toUpdateAd.InfoFileURL = infoFileUrlText == null ? "" : infoFileUrlText;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Career Ad ID: {adId} to: " + "{@toUpdateAd}", toUpdateAd);

            try
            {
                await _careersService.UpdateWebCareer(toUpdateAd, adId);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated Web Career Ad successfully");
                _logger.Information($"Updated Web Career Ad successfully. Short Description: {toUpdateAd.Short_Description}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Web career was updated successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating Web Career Ad by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating Web Career Ad. Short Description: {toUpdateAd.Short_Description}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure updating web career: " + ex.Message.Substring(0, 100) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCareerAsync(int Id)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting Web Career Ad ID: {Id}");
            var toDeleteAd = await _careersService.GetWebCareerByIdAsync(Id);

            try
            {
                await _careersService.DeleteWebCareer(Id);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted Web Career Ad successfully");
                _logger.Information($"Deleted Web Career Ad successfully. Short Description: {toDeleteAd.Short_Description}", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Web Career Ad by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Web Career Ad. Short Description: {toDeleteAd.Short_Description}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return Json(new { success = false });
            }

            return Json(new { success = true });
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
