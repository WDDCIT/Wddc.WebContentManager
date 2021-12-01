using System;

namespace Wddc.WebContentManager.Core.Helpers
{
    public static class FontAwesomeHelper
    {
        public static string GetIcon(string className)
        {
            return String.Format("<i class=\"{0}\"></i>", className);

        }
        public static string GetIcon(string className, string tooltipTitle)
        {
            return String.Format("<i class=\"{0}\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"{1}\"></i>", className, tooltipTitle);
        }
    }
}
