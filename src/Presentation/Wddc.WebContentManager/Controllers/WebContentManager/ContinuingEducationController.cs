using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.WebContentManager.Services.WebContent.ContinuingEducation;
using Wddc.WebContentManager.Models.WebContent.ContinuingEducation;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Serilog;
using Microsoft.AspNetCore.Http;
using System.IO;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using PagedList;
using System.Web;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class ContinuingEducationController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly IContinuingEducationService _continuingEducationService;

        public ContinuingEducationController(IContinuingEducationService continuingEducationService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _continuingEducationService = continuingEducationService;
        }

        public async Task<ActionResult> IndexAsync(string response, string message)
        {
            Log.Logger.Information("The system is getting the list of Web Continuing Education from table dbo.Web_CE_Ads");
            List<Web_CE_Ads> WebCEAds = await _continuingEducationService.GetAllWebCEAds();
            WebCEAdsModel WebCEAdsModel = new WebCEAdsModel
            {
                WebCEAds = WebCEAds.OrderByDescending(_ => _.Event_Date),
                WebCEAdsActive = WebCEAds.Where(_ => _.End_Date >= DateTime.Today).OrderByDescending(_ => _.Event_Date),
                WebCEAdsExpired = WebCEAds.Where(_ => _.End_Date < DateTime.Today).OrderByDescending(_ => _.Event_Date)
            };

            ViewBag.response = response;
            ViewBag.Message = message;
            return View(WebCEAdsModel);
        }

        public async Task<ActionResult> ContinuingEducationEditorPartialAsync(int adId)
        {
            Log.Logger.Information("The system is getting the Web Continuing Education ads from table dbo.Web_CE_Ads of Ad ID: " + adId);
            var result = await _continuingEducationService.GetWebCEAdsById(adId);
            return PartialView("_ContinuingEducationEditorPartial", result);
        }

        public async Task<JsonResult> GetFirstActiveAdAsync()
        {
            Log.Logger.Information("The system is getting the first active Web Continuing Education ad from table dbo.Web_CE_Ads");
            List<Web_CE_Ads> WebCEAds = await _continuingEducationService.GetAllWebCEAds();
            Web_CE_Ads result = WebCEAds.Where(_ => _.End_Date >= DateTime.Today).OrderByDescending(_ => _.Event_Date).First();
            return Json(result);
        }

        public async Task<JsonResult> GetFirstExpiredAdAsync()
        {
            Log.Logger.Information("The system is getting the first expired Web Continuing Education ad from table dbo.Web_CE_Ads");
            List<Web_CE_Ads> WebCEAds = await _continuingEducationService.GetAllWebCEAds();
            Web_CE_Ads result = WebCEAds.Where(_ => _.End_Date < DateTime.Today).OrderByDescending(_ => _.Event_Date).First();
            return Json(result);
        }

        [HttpPost]
        public async Task<ActionResult> AddContinuingEducationAdAsync(string newLongDesc, string newShortDesc, DateTime? newEventDate, string newLocation, string newApplicationInstructions, string newContactName, 
            string newContactTitle, string newContactPhone, string newContactPhoneExt, string newContactFax, string newContactEmail, IFormFile newInfoFileUrl, string newInfoFileUrlText, 
            string newWebsite, string newWebsiteLinkText, DateTime? newStartDate, DateTime? newEndDate)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Continuing Education ad");

            if (newInfoFileUrl != null)
            {
                var path = "\\\\WEBSRVR\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\CE";
                path = Path.Combine(
                  path, newInfoFileUrl.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await newInfoFileUrl.CopyToAsync(stream);
                }
            }

            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(newLongDesc, myWriter);
            newLongDesc = myWriter.ToString();

            Web_CE_Ads newWeb_CE_Ads = new Web_CE_Ads();

            newWeb_CE_Ads.Start_Date = newStartDate;
            newWeb_CE_Ads.End_Date = newEndDate;
            newWeb_CE_Ads.Event_Date = newEventDate;
            newWeb_CE_Ads.Location = newLocation  ==null ? "" : newLocation;
            newWeb_CE_Ads.LongDesc = newLongDesc == null ? "" : newLongDesc;
            newWeb_CE_Ads.ShortDesc = newShortDesc == null ? "" : newShortDesc;
            newWeb_CE_Ads.Application_Instructions = newApplicationInstructions == null ? "" : newApplicationInstructions;
            newWeb_CE_Ads.Contact_Name = newContactName == null ? "" : newContactName;
            newWeb_CE_Ads.Contact_Title = newContactTitle == null ? "" : newContactTitle;
            newWeb_CE_Ads.Contact_Phone = newContactPhone == null ? "" : newContactPhone;
            newWeb_CE_Ads.Contact_Phone_Ext = newContactPhoneExt == null ? "" : newContactPhoneExt;
            newWeb_CE_Ads.Contact_Email = newContactEmail == null ? "" : newContactEmail;
            newWeb_CE_Ads.Contact_Fax = newContactFax == null ? "" : newContactFax;
            newWeb_CE_Ads.InfoFileURL = newInfoFileUrlText == null ? "" : newInfoFileUrlText;
            newWeb_CE_Ads.Website = newWebsite == null ? "" : newWebsite;
            newWeb_CE_Ads.WebsiteLinkText = newWebsiteLinkText == null ? "" : newWebsiteLinkText;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Continuing Education ad: {@newWeb_CE_Ads}", newWeb_CE_Ads);

            try
            {
                newWeb_CE_Ads = await _continuingEducationService.CreateWebCEAd(newWeb_CE_Ads);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new Web Continuing Education Ad ID: {newWeb_CE_Ads.Ad_ID} successfully");
                _logger.Information($"Added new Web Continuing Education ad successfully. Event Date: {newWeb_CE_Ads.Event_Date.Value.ToString("yyyy-MM-dd")}, Location: {newWeb_CE_Ads.Location}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Continuing education ad, event date " + newEventDate.Value.ToString("yyyy-MM-dd") + " was added successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding Web Continuing Education ad by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding Web Continuing Education ad. Event Date: {newWeb_CE_Ads.Event_Date.Value.ToString("yyyy-MM-dd")}, Location: {newWeb_CE_Ads.Location}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure adding continuing education ad, event date " + newEventDate.Value.ToString("yyyy-MM-dd") + ex.Message.Substring(0, 200) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateContinuingEducationAdAsync(int adId, string longDesc, string shortDesc, DateTime? eventDate, string location, string applicationInstructions, string contactName,
            string contactTitle, string contactPhone, string contactPhoneExt, string contactFax, string contactEmail, IFormFile infoFileUrl, string infoFileUrlText,
            string website, string websiteLinkText, DateTime? startDate, DateTime? endDate)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Continuing Education Ad ID: {adId}");

            if (infoFileUrl != null)
            {
                var path = "\\\\WEBSRVR\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\CE";
                path = Path.Combine(
                  path, infoFileUrl.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await infoFileUrl.CopyToAsync(stream);
                }
            }

            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(longDesc, myWriter);
            longDesc = myWriter.ToString();

            var toUpdateAd = await _continuingEducationService.GetWebCEAdsById(adId);
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Continuing Education Ad ID: {adId} from: " + "{@toUpdateAd}", toUpdateAd);

            toUpdateAd.Start_Date = startDate;
            toUpdateAd.End_Date = endDate;
            toUpdateAd.Event_Date = eventDate;
            toUpdateAd.Location = location == null ? "" : location;
            toUpdateAd.LongDesc = longDesc == null? "" : longDesc;
            toUpdateAd.ShortDesc = shortDesc == null ? "" : shortDesc;
            toUpdateAd.Application_Instructions = applicationInstructions == null? "" : applicationInstructions;
            toUpdateAd.Contact_Name = contactName == null ? "" : contactName;
            toUpdateAd.Contact_Title = contactTitle == null ? "" : contactTitle;
            toUpdateAd.Contact_Phone = contactPhone == null ? "" : contactPhone;
            toUpdateAd.Contact_Phone_Ext = contactPhoneExt == null ? "" : contactPhoneExt;
            toUpdateAd.Contact_Email = contactEmail == null ? "" : contactEmail;
            toUpdateAd.Contact_Fax = contactFax == null ? "" : contactFax;
            toUpdateAd.InfoFileURL = infoFileUrlText == null ? "" : infoFileUrlText;
            toUpdateAd.Website = website == null ? "" : website;
            toUpdateAd.WebsiteLinkText = websiteLinkText == null ? "" : websiteLinkText;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Continuing Education Ad ID: {adId} to: " + "{@toUpdateAd}", toUpdateAd);

            try
            {
                await _continuingEducationService.UpdateWebCEAd(toUpdateAd, adId);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated Web Continuing Education Ad successfully");
                _logger.Information($"Updated Web Continuing Education Ad successfully. Event Date: {toUpdateAd.Event_Date.Value.ToString("yyyy-MM-dd")}, Location: {toUpdateAd.Location}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Continuing education ad, event date " + eventDate.Value.ToString("yyyy-MM-dd") + " was updated successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating Web Continuing Education Ad by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating Web Continuing Education Ad. Event Date: {toUpdateAd.Event_Date.Value.ToString("yyyy-MM-dd")}, Location: {toUpdateAd.Location}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure updating continuing education ad, event date " + eventDate.Value.ToString("yyyy-MM-dd") + ex.Message.Substring(0, 200) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteContinuingEducationAdAsync(int Id)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting Web Continuing Education Ad ID: {Id}");
            var toDeleteAd = await _continuingEducationService.GetWebCEAdsById(Id);

            try
            {
                await _continuingEducationService.DeleteWebCEAd(Id);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted Web Continuing Education Ad successfully");
                _logger.Information($"Deleted Web Continuing Education Ad successfully. Event Date: {toDeleteAd.Event_Date.Value.ToString("yyyy-MM-dd")}, Location: {toDeleteAd.Location}", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Web Continuing Education Ad by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Web Continuing Education Ad. Event Date: {toDeleteAd.Event_Date.Value.ToString("yyyy-MM-dd")}, Location: {toDeleteAd.Location}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
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
