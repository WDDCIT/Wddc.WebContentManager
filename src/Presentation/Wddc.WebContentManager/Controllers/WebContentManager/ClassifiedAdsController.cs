using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.WebContentManager.Models.WebContent.ClassifiedAds;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Serilog;
using Wddc.WebContentManager.Services.WebContent.ClassifiedAds;
using Microsoft.AspNetCore.Http;
using System.IO;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using PagedList;
using System.Web;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class ClassifiedAdsController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly IClassifiedAdsService _classifiedAdsService;

        public ClassifiedAdsController(IClassifiedAdsService classifiedAdsService, Wddc.WebContentManager.Services.Logging.ILogger logger)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _classifiedAdsService = classifiedAdsService;
        }

        public async Task<ActionResult> IndexAsync(string response, string message)
        {
            Log.Logger.Information("The system is getting the list of Web Classified Ads from table dbo.Web_Classified_Ads");
            List<Web_Classified_Ads> webClassifiedAds = await _classifiedAdsService.GetAllWebClassifiedAds();
            WebClassifiedAdsModel WebClassifiedAdsModel = new WebClassifiedAdsModel
            {
                WebClassifiedAds = webClassifiedAds.OrderBy(_ => _.MemberNbr),
                WebClassifiedAdsActive = webClassifiedAds.Where(_ => _.End_Date >= DateTime.Today).OrderBy(_ => _.MemberNbr).ThenBy(_ => _.Short_Description),
                WebClassifiedAdsExpired = webClassifiedAds.Where(_ => _.End_Date < DateTime.Today).OrderBy(_ => _.MemberNbr).ThenBy(_ => _.Short_Description)
            };

            ViewBag.response = response;
            ViewBag.Message = message;
            return View(WebClassifiedAdsModel);
        }

        public async Task<ActionResult> ClassifiedAdsEditorPartialAsync(int adId)
        {
            Log.Logger.Information("The system is getting Web Classified Ad from table dbo.Web_Classified_Ads with Ad ID: " + adId);
            var result = await _classifiedAdsService.GetWebClassifiedAdById(adId);
            return PartialView("_ClassifiedAdsEditorPartial", result);
        }

        public async Task<JsonResult> GetFirstActiveAdAsync()
        {
            Log.Logger.Information("The system is getting the first active Web Classified Ad from table dbo.Web_Classified_Ads");
            List<Web_Classified_Ads> webClassifiedAds = await _classifiedAdsService.GetAllWebClassifiedAds();
            Web_Classified_Ads result = webClassifiedAds.Where(_ => _.End_Date >= DateTime.Today).OrderBy(_ => _.MemberNbr).ThenBy(_ => _.Short_Description).First();
            return Json(result);
        }

        public async Task<JsonResult> GetFirstExpiredAdAsync()
        {
            Log.Logger.Information("The system is getting the first expired Web Classified Ad from table dbo.Web_Classified_Ads");
            List<Web_Classified_Ads> webClassifiedAds = await _classifiedAdsService.GetAllWebClassifiedAds();
            Web_Classified_Ads result = webClassifiedAds.Where(_ => _.End_Date < DateTime.Today).OrderBy(_ => _.MemberNbr).ThenBy(_ => _.Short_Description).First();
            return Json(result);
        }

        public async Task<string> GetMemberNameAsync(string memberNbr)
        {
            Log.Logger.Information("The system is getting the member name of member number: " + memberNbr);
            if (memberNbr == null || memberNbr.Trim().Length == 0)
                return "";
            return await _classifiedAdsService.GetMemberName(memberNbr);
        }

        [HttpPost]
        public async Task<ActionResult> AddClassifiedAdAsync(string newShortDescription, string newLongDescription, string newLocation, string newApplicationInstructions, string newMemberNbr,
            string newContactName, string newContactTitle, string newContactPhone, string newContactPhoneExt, string newContactFax, string newContactEmail, IFormFile newInfoFileUrl, 
            string newInfoFileUrlText, DateTime? newStartDate, DateTime? newEndDate)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Classified Ad");

            if (newInfoFileUrl != null)
            {
                var path = "\\\\WEBSRVR\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Classified Ads";
                //var path = "\\\\Pppsrvr1\\c\\TempEDI";
                path = Path.Combine(
                  path, newInfoFileUrl.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await newInfoFileUrl.CopyToAsync(stream);
                }
            }           

            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(newLongDescription, myWriter);
            newLongDescription = myWriter.ToString();

            Web_Classified_Ads newWeb_Classified_Ads = new Web_Classified_Ads();
            newWeb_Classified_Ads.Start_Date = newStartDate;
            newWeb_Classified_Ads.End_Date = newEndDate;
            newWeb_Classified_Ads.MemberNbr = newMemberNbr == null ? "" : newMemberNbr;
            newWeb_Classified_Ads.Location = newLocation == null ? "" : newLocation;
            newWeb_Classified_Ads.Long_Description = newLongDescription == null ? "" : newLongDescription;
            newWeb_Classified_Ads.Short_Description = newShortDescription == null ? "" : newShortDescription;
            newWeb_Classified_Ads.Application_Instructions = newApplicationInstructions == null ? "" : newApplicationInstructions;
            newWeb_Classified_Ads.Contact_Name = newContactName == null ? "" : newContactName;
            newWeb_Classified_Ads.Contact_Title = newContactTitle == null ? "" : newContactTitle;
            newWeb_Classified_Ads.Contact_Phone = newContactPhone == null ? "" : newContactPhone;
            newWeb_Classified_Ads.Contact_Phone_Ext = newContactPhoneExt == null ? "" : newContactPhoneExt;
            newWeb_Classified_Ads.Contact_Email = newContactEmail == null ? "" : newContactEmail;
            newWeb_Classified_Ads.Contact_Fax = newContactFax == null ? "" : newContactFax;
            newWeb_Classified_Ads.InfoFileURL = newInfoFileUrlText == null ? "" : newInfoFileUrlText;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new Web Classified Ad: {@newWeb_Classified_Ads}", newWeb_Classified_Ads);

            try
            {
                newWeb_Classified_Ads = await _classifiedAdsService.CreateWebClassifiedAd(newWeb_Classified_Ads);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new Web Classified Ad ID: {newWeb_Classified_Ads.Ad_ID} successfully");
                _logger.Information($"Added new Web Classified Ad successfully. Member Nbr: {newWeb_Classified_Ads.MemberNbr}, Location: {newWeb_Classified_Ads.Location}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Classified ad, member number: " + newMemberNbr + " was added successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding Web Classified Ad by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding Web Classified Ad. Member Nbr: {newWeb_Classified_Ads.MemberNbr}, Location: {newWeb_Classified_Ads.Location}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure adding classified ad, member number: " + newMemberNbr + ex.Message.Substring(0, 100) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateClassifiedAdAsync(int adId, string shortDescription, string longDescription, string location, string applicationInstructions, string memberNbr,
            string contactName, string contactTitle, string contactPhone, string contactPhoneExt, string contactFax, string contactEmail, IFormFile infoFileUrl, string infoFileUrlText,
            DateTime? startDate, DateTime? endDate)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Classified Ad ID: {adId}");

            if (infoFileUrl != null)
            {
                var path = "\\\\WEBSRVR\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Classified Ads";
                
                path = Path.Combine(
                  path, infoFileUrl.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await infoFileUrl.CopyToAsync(stream);
                }
            }

            StringWriter myWriter = new StringWriter();
            HttpUtility.HtmlDecode(longDescription, myWriter);
            longDescription = myWriter.ToString();

            var toUpdateAd = await _classifiedAdsService.GetWebClassifiedAdById(adId);
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Classified Ad ID: {adId} from: " + "{@toUpdateAd}", toUpdateAd);

            toUpdateAd.Start_Date = startDate;
            toUpdateAd.End_Date = endDate;
            toUpdateAd.MemberNbr = memberNbr == null ? "" : memberNbr;
            toUpdateAd.Location = location == null ? "" : location;
            toUpdateAd.Long_Description = longDescription == null ? "" : longDescription;
            toUpdateAd.Short_Description = shortDescription == null ? "" : shortDescription;
            toUpdateAd.Application_Instructions = applicationInstructions == null ? "" : applicationInstructions;
            toUpdateAd.Contact_Name = contactName == null ? "" : contactName;
            toUpdateAd.Contact_Title = contactTitle == null ? "" : contactTitle;
            toUpdateAd.Contact_Phone = contactPhone == null ? "" : contactPhone;
            toUpdateAd.Contact_Phone_Ext = contactPhoneExt == null ? "" : contactPhoneExt;
            toUpdateAd.Contact_Email = contactEmail == null ? "" : contactEmail;
            toUpdateAd.Contact_Fax = contactFax == null ? "" : contactFax;
            toUpdateAd.InfoFileURL = infoFileUrlText == null ? "" : infoFileUrlText;
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating Web Classified Ad ID: {adId} to: " + "{@toUpdateAd}", toUpdateAd);

            try
            {
                await _classifiedAdsService.UpdateWebClassifiedAd(toUpdateAd, adId);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated Web Classified Ad successfully");
                _logger.Information($"Updated Web Classified Ad successfully. Member Nbr: {toUpdateAd.MemberNbr}, Location: {toUpdateAd.Location}", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Classified ad, member number: " + memberNbr + " was updated successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating Web Classified Ad by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating Web Classified Ad. Member Nbr: {toUpdateAd.MemberNbr}, Location: {toUpdateAd.Location}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure updating classified ad, member number: " + memberNbr + ex.Message.Substring(0, 100) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteClassifiedAdAsync(int Id)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting Web Classified Ad ID: {Id}");
            var toDeleteAd = await _classifiedAdsService.GetWebClassifiedAdById(Id);

            try
            {
                await _classifiedAdsService.DeleteWebClassifiedAd(Id);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted Web Classified Ad successfully");
                _logger.Information($"Deleted Web Classified Ad successfully. Member Nbr: {toDeleteAd.MemberNbr}, Location: {toDeleteAd.Location}", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Web Classified Ad by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Web Classified Ad. Member Nbr: {toDeleteAd.MemberNbr}, Location: {toDeleteAd.Location}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
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

        public async Task<ActionResult> UploadFileAsync(IFormFile file)
        {
            try
            {
                var filePath = Path.GetTempFileName();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return View();
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }

    }
}
