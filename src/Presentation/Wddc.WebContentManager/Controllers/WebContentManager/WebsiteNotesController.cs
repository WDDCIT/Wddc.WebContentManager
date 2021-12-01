using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wddc.PurchasingOrderApp.Services;
using Wddc.Core.Domain.Webserver.WebOrdering;
using Kendo.Mvc.Extensions;
using Wddc.WebContentManager.Services.WebContent.Videos;
using Wddc.WebContentManager.Models.WebContent.Videos;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Wddc.WebContentManager.Controllers.WebContentManager
{
    public class WebsiteNotesController : BaseController
    {
        private readonly LogManager _loggerManager;
        private readonly IVideosService _videosService;

        public WebsiteNotesController(IVideosService videosService)
        {
            _loggerManager = new LogManager();
            _videosService = videosService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public void WriteInfoToLog(string logMessage)
        {
            _loggerManager.WriteInfo(logMessage.Replace("\\n", System.Environment.NewLine));
        }

        public void WriteErrorToLog(string logMessage)
        {
            _loggerManager.WriteDebug(logMessage.Replace("\\n", System.Environment.NewLine));
        }
    }
}
