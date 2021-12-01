using System;
using System.Linq;
using System.Web;

namespace Wddc.WebContentManager.Core.Extensions
{
    public static class StringExtensions
    {
        public static string AppendQueryParams(this string self, object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return String.Join("&", properties.ToArray());
        }
    }
}
