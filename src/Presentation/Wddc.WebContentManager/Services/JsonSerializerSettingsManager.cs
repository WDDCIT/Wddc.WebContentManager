using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wddc.PurchasingOrderApp.Services
{
    public class JsonSerializerSettingsManager
    {
        public static JsonSerializerSettings Settings => new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
    }
}