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
using Wddc.WebContentManager.Services.WebContent.Affinity;
using PagedList;
using Wddc.WebContentManager.Services.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.WebContentManager.Models;
using Serilog;
using Wddc.WebContentManager.Models.WebContent.AffinityPrograms;
using Microsoft.AspNetCore.Hosting;
using System.Drawing;
using System.Drawing.Imaging;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class AffinityProgramsController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly Wddc.WebContentManager.Services.Logging.ILogger _logger;
        private readonly IAffinityService _affinityService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AffinityProgramsController(IAffinityService affinityService, Wddc.WebContentManager.Services.Logging.ILogger logger, IHostingEnvironment hostingEnvironment)
        {
            _loggerManager = new LogManager();
            _logger = logger;
            _affinityService = affinityService;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<ActionResult> IndexAsync(string response, string message)
        {
            Log.Logger.Information("The system is getting the list of Web Affinity Programs from table Web_AffinityPrograms");
            List<Web_AffinityPrograms> webAffinityPrograms = await _affinityService.GetAllWebAffinityPrograms();
            WebAffinityProgramsModel webAffinityProgramsModel = new WebAffinityProgramsModel
            {
                AffinityPrograms = webAffinityPrograms.Where(_ => _.ProgramName != null).OrderBy(_ => _.ProgramName)
            };

            ViewBag.response = response;
            ViewBag.Message = message;
            return View(webAffinityProgramsModel);
        }

        public async Task<JsonResult> GetFirstAffinityProgramAsync()
        {
            List<Web_AffinityPrograms> webAffinityPrograms = await _affinityService.GetAllWebAffinityPrograms();
            Web_AffinityPrograms result = webAffinityPrograms.Where(_ => _.ProgramName != null).OrderBy(_ => _.ProgramName).First();
            return Json(result);
        }

        public async Task<ActionResult> AffinityProgramsEditorPartialAsync(int programId)
        {
            Log.Logger.Information("The system is getting the Web Affinity Program from table dbo.Web_AffinityPrograms with Program ID: " + programId);
            var result = await _affinityService.GetWebAffinityProgramById(programId);
            return PartialView("_AffinityProgramsEditorPartial", result);
        }

        [HttpPost]
        public async Task<ActionResult> AddAffinityProgramAsync(string newProgramName, string newProgramDescription, string newBillingThrough, string newDiscount,
            string newComments, string newWebAddress, string newContact, string newLogoName, IFormFile newLogoUrl, string newLogoURLText, string newInfoFileName,
            IFormFile newInfoFileUrl, string newInfoFileUrlText, string newInfoFileDisplayName, string newInfoFileName2, IFormFile newInfoFileUrl2, string newInfoFileUrlText2, 
            string newInfoFileDisplayName2)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding an affinity program");

            if (newLogoUrl != null)
            {
                var path = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Affinity Programs\\Logos";
                path = Path.Combine(path, newLogoUrl.FileName);

                var path2 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\Affinity Programs\\Logos";
                path2 = Path.Combine(path2, newLogoUrl.FileName);

                var wwwrootLogosUpload = $"Affinity_Logos/{newLogoUrl.FileName}";
                var path3 = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(_hostingEnvironment.WebRootPath, wwwrootLogosUpload));
                path3 = path3 + "\\" + newLogoUrl.FileName;

                var image = Image.FromStream(newLogoUrl.OpenReadStream());

                int sourceWidth = image.Width;
                int sourceHeight = image.Height;
                float nPercent = 0;
                float nPercentW = 0;
                float nPercentH = 0;

                nPercentW = ((float)300 / (float)sourceWidth); 
                nPercentH = ((float)100 / (float)sourceHeight);

                if (nPercentH < nPercentW)
                    nPercent = nPercentH;
                else
                    nPercent = nPercentW;
 
                int destWidth = (int)(sourceWidth * nPercent);
                int destHeight = (int)(sourceHeight * nPercent);

                var resized = new Bitmap(image, new Size(destWidth, destHeight));
                using var imageStream = new MemoryStream();
                resized.Save(imageStream, ImageFormat.Gif);
                var imageBytes = imageStream.ToArray();

                try
                {
                    if (sourceWidth > 300 || sourceHeight > 100)
                    {
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            using (var writer = new BinaryWriter(stream))
                            {
                                writer.Write(imageBytes);
                            }
                        }

                        using (var stream = new FileStream(path2, FileMode.Create))
                        {
                            using (var writer = new BinaryWriter(stream))
                            {
                                writer.Write(imageBytes);
                            }
                        }

                        using (var stream = new FileStream(path3, FileMode.Create))
                        {
                            using (var writer = new BinaryWriter(stream))
                            {
                                writer.Write(imageBytes);
                            }
                        }
                    }
                    else
                    {
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await newLogoUrl.CopyToAsync(stream);
                        }

                        using (var stream = new FileStream(path2, FileMode.Create))
                        {
                            await newLogoUrl.CopyToAsync(stream);
                        }

                        using (var stream = new FileStream(path3, FileMode.Create))
                        {
                            await newLogoUrl.CopyToAsync(stream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error copying file: {newLogoUrl.FileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                }

            }

            if (newInfoFileUrl != null)
            {
                var path = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Affinity Programs";
                path = Path.Combine(path, newInfoFileUrl.FileName);

                var path2 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\Affinity Programs";
                path2 = Path.Combine(path2, newInfoFileUrl.FileName);

                try
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await newInfoFileUrl.CopyToAsync(stream);
                    }

                    using (var stream = new FileStream(path2, FileMode.Create))
                    {
                        await newInfoFileUrl.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error copying file: {newInfoFileUrl.FileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                }
            }

            if (newInfoFileUrl2 != null)
            {
                var path = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Affinity Programs";
                path = Path.Combine(path, newInfoFileUrl2.FileName);

                var path2 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\Affinity Programs";
                path2 = Path.Combine(path2, newInfoFileUrl2.FileName);

                try
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await newInfoFileUrl2.CopyToAsync(stream);
                    }

                    using (var stream = new FileStream(path2, FileMode.Create))
                    {
                        await newInfoFileUrl2.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error copying file: {newInfoFileUrl2.FileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                }
            }

            Web_AffinityPrograms newWeb_AffinityPrograms = new Web_AffinityPrograms();

            newWeb_AffinityPrograms.ProgramName = newProgramName;
            newWeb_AffinityPrograms.ProgramDescription = newProgramDescription;
            newWeb_AffinityPrograms.BillingThrough = newBillingThrough;
            newWeb_AffinityPrograms.Discount = newDiscount;
            newWeb_AffinityPrograms.Comments = newComments;
            newWeb_AffinityPrograms.WebAddress = newWebAddress;
            newWeb_AffinityPrograms.Contact = newContact;
            newWeb_AffinityPrograms.LogoName = newLogoName;
            newWeb_AffinityPrograms.LogoURL = newLogoURLText == null ? "" : newLogoURLText;
            newWeb_AffinityPrograms.ModifiedBy = User.Identity.Name.Substring(7).ToLower();
            newWeb_AffinityPrograms.InfoFileName = newInfoFileName;
            newWeb_AffinityPrograms.InfoFileURL = newInfoFileUrlText == null ? "" : newInfoFileUrlText;
            newWeb_AffinityPrograms.InfoFileDisplayName = newInfoFileDisplayName;
            newWeb_AffinityPrograms.InfoFileName2 = newInfoFileName2;
            newWeb_AffinityPrograms.InfoFileURL2 = newInfoFileUrlText2 == null ? "" : newInfoFileUrlText2;
            newWeb_AffinityPrograms.InfoFileDisplayName2 = newInfoFileDisplayName2;

            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is adding a new affinity program: {@newWeb_AffinityPrograms}", newWeb_AffinityPrograms);

            try
            {
                newWeb_AffinityPrograms = await _affinityService.CreateWebAffinityProgram(newWeb_AffinityPrograms);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} added new affinity program id: {newWeb_AffinityPrograms.ProgramID} successfully");
                _logger.Information($"Added new affinity program: {newWeb_AffinityPrograms.ProgramName} successfully", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Affinity program (" + newWeb_AffinityPrograms.ProgramName + ") was added successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error adding affinity program by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error adding affinity program: {newWeb_AffinityPrograms.ProgramName}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure adding affinity program (" + newWeb_AffinityPrograms.ProgramName + "): " + ex.Message.Substring(0, 200) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAffinityProgramAsync(int programId, string programName, string programDescription, string billingThrough, string discount,
            string comments, string webAddress, string contact, string logoName, IFormFile logoUrl, string logoURLText, string infoFileName,
            IFormFile infoFileUrl, string infoFileUrlText, string infoFileDisplayName, string infoFileName2, IFormFile infoFileUrl2, string infoFileUrlText2,
            string infoFileDisplayName2)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + " is updating the affinity program id" + programId);

            if (logoUrl != null)
            {
                var path = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Affinity Programs\\Logos";
                path = Path.Combine(path, logoUrl.FileName);

                var path2 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\Affinity Programs\\Logos";
                path2 = Path.Combine(path2, logoUrl.FileName);

                var wwwrootLogosUpload = $"Affinity_Logos/{logoUrl.FileName}";
                var path3 = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(_hostingEnvironment.WebRootPath, wwwrootLogosUpload));
                path3 = path3 + "\\" + logoUrl.FileName;

                var image = Image.FromStream(logoUrl.OpenReadStream());

                int sourceWidth = image.Width;
                int sourceHeight = image.Height;
                float nPercent = 0;
                float nPercentW = 0;
                float nPercentH = 0;

                nPercentW = ((float)300 / (float)sourceWidth);
                nPercentH = ((float)100 / (float)sourceHeight);

                if (nPercentH < nPercentW)
                    nPercent = nPercentH;
                else
                    nPercent = nPercentW;

                int destWidth = (int)(sourceWidth * nPercent);
                int destHeight = (int)(sourceHeight * nPercent);

                var resized = new Bitmap(image, new Size(destWidth, destHeight));
                using var imageStream = new MemoryStream();
                resized.Save(imageStream, ImageFormat.Gif);
                var imageBytes = imageStream.ToArray();

                try
                {
                    if (sourceWidth > 300 || sourceHeight > 100)
                    {
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            using (var writer = new BinaryWriter(stream))
                            {
                                writer.Write(imageBytes);
                            }
                        }

                        using (var stream = new FileStream(path2, FileMode.Create))
                        {
                            using (var writer = new BinaryWriter(stream))
                            {
                                writer.Write(imageBytes);
                            }
                        }

                        using (var stream = new FileStream(path3, FileMode.Create))
                        {
                            using (var writer = new BinaryWriter(stream))
                            {
                                writer.Write(imageBytes);
                            }
                        }
                    }
                    else
                    {
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await logoUrl.CopyToAsync(stream);
                        }

                        using (var stream = new FileStream(path2, FileMode.Create))
                        {
                            await logoUrl.CopyToAsync(stream);
                        }

                        using (var stream = new FileStream(path3, FileMode.Create))
                        {
                            await logoUrl.CopyToAsync(stream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error copying file: {logoUrl.FileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                }
            }

            if (infoFileUrl != null)
            {
                var path = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Affinity Programs";
                path = Path.Combine(path, infoFileUrl.FileName);

                var path2 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\Affinity Programs";
                path2 = Path.Combine(path2, infoFileUrl.FileName);

                try
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await infoFileUrl.CopyToAsync(stream);
                    }

                    using (var stream = new FileStream(path2, FileMode.Create))
                    {
                        await infoFileUrl.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error copying file: {infoFileUrl.FileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                }
            }

            if (infoFileUrl2 != null)
            {
                var path = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\CS\\Affinity Programs";
                path = Path.Combine(path, infoFileUrl2.FileName);

                var path2 = "\\\\WEBsrvr\\WDDCMembers\\WDDCWebPages\\wddc_members\\Affinity Programs";
                path2 = Path.Combine(path2, infoFileUrl2.FileName);

                try
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await infoFileUrl2.CopyToAsync(stream);
                    }

                    using (var stream = new FileStream(path2, FileMode.Create))
                    {
                        await infoFileUrl2.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"Error copying file: {infoFileUrl2.FileName} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                }
            }

            var toUpdateProgram = await _affinityService.GetWebAffinityProgramById(programId);

            toUpdateProgram.ProgramName = programName;
            toUpdateProgram.ProgramDescription = programDescription;
            toUpdateProgram.BillingThrough = billingThrough;
            toUpdateProgram.Discount = discount;
            toUpdateProgram.Comments = comments;
            toUpdateProgram.WebAddress = webAddress;
            toUpdateProgram.Contact = contact;
            toUpdateProgram.LogoName = logoName;
            toUpdateProgram.LogoURL = logoURLText == null ? "" : logoURLText;
            toUpdateProgram.ModifiedBy = User.Identity.Name.Substring(7).ToLower();
            toUpdateProgram.InfoFileName = infoFileName;
            toUpdateProgram.InfoFileURL = infoFileUrlText == null ? "" : infoFileUrlText;
            toUpdateProgram.InfoFileDisplayName = infoFileDisplayName;
            toUpdateProgram.InfoFileName2 = infoFileName2;
            toUpdateProgram.InfoFileURL2 = infoFileUrlText2 == null ? "" : infoFileUrlText2;
            toUpdateProgram.InfoFileDisplayName2 = infoFileDisplayName2;

            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is updating affinity program Id: {programId} to: " + "{@toUpdateProgram}", toUpdateProgram);

            try
            {
                await _affinityService.UpdateWebAffinityProgram(toUpdateProgram, programId);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} updated affinity program id: {programId} successfully");
                _logger.Information($"Updated affinity program: {toUpdateProgram.ProgramName} successfully", null, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Success", message = "Affinity program (" + toUpdateProgram.ProgramName + ") was updated successfully! " });
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error updating affinity program id {programId} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error updating affinity program: {toUpdateProgram.ProgramName}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
                return RedirectToAction("Index", new { response = "Failure", message = "Failure updating affinity program (" + toUpdateProgram.ProgramName + "): " + ex.Message.Substring(0, 200) });
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAffinityProgramAsync(int programId)
        {
            Log.Logger.Information(User.Identity.Name.Substring(7).ToLower() + $" is deleting affinity program Id: {programId}");
            var toDeleteProgram = await _affinityService.GetWebAffinityProgramById(programId);

            try
            {
                await _affinityService.DeleteWebAffinityProgram(programId);
                Log.Logger.Information($"{User.Identity.Name.Substring(7).ToLower()} deleted Web Affinity Program Id {programId} successfully");
                _logger.Information($"Deleted Web Affinity Program {toDeleteProgram.ProgramName} successfully", null, User, "WebOrdering");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Error deleting Web Affinity Program Id {programId} by {User.Identity.Name.Substring(7).ToLower()}: {ex.Message}");
                _logger.Error($"Error deleting Web Affinity Program {toDeleteProgram.ProgramName}: {ex.Message.Substring(0, 200)}", ex, User, "WebOrdering");
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
