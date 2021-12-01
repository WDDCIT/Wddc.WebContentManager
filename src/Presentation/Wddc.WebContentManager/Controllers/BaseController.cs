using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.RegularExpressions;
using Wddc.Core;

namespace Wddc.WebContentManager.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        public JsonResult FormattedJson(object data, bool success, string message = null)
        {
            var obj = new
            {
                data,
                success,
                message
            };

            return Json(obj);
        }

        public string CleanCustomerId(string customerId)
        {
            return Regex.Replace(customerId, "-MAIN", String.Empty, RegexOptions.IgnoreCase)
                .Trim();
        }

    }
}