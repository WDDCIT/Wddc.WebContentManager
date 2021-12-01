using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Wddc.Core.Domain.Shipping.Logging;

namespace Wddc.WebContentManager.Services.Logging
{
    public static class LoggingExtensions
    {

        public static void Debug(this ILogger logger, string message, Exception exception = null, IPrincipal user = null, string controller = "Shipping")
        {
            FilteredLog(logger, LogLevel.Debug, message, exception, user, controller);
        }
        public static void Information(this ILogger logger, string message, Exception exception = null, IPrincipal user = null, string controller = "Shipping")
        {
            FilteredLog(logger, LogLevel.Information, message, exception, user, controller);
        }
        public static void Warning(this ILogger logger, string message, Exception exception = null, IPrincipal user = null, string controller = "Shipping")
        {
            FilteredLog(logger, LogLevel.Warning, message, exception, user, controller);
        }
        public static void Error(this ILogger logger, string message, Exception exception = null, IPrincipal user = null, string controller = "Shipping")
        {
            FilteredLog(logger, LogLevel.Error, message, exception, user, controller);
        }
        public static void Fatal(this ILogger logger, string message, Exception exception = null, IPrincipal user = null, string controller = "Shipping")
        {
            FilteredLog(logger, LogLevel.Fatal, message, exception, user, controller);
        }

        private static void FilteredLog(ILogger logger, LogLevel level, string message, Exception exception = null, IPrincipal user = null, string controller = "Shipping")
        {
            if (exception is System.Threading.ThreadAbortException)
                return;

            if (logger.IsEnabled(level))
            {
                string fullMessage = exception == null ? string.Empty : exception.ToString();

                if (controller == "Shipping")
                    logger.InsertShippingLog(level, message, fullMessage, user);
                else
                    if (controller == "Invoicing" || controller == "Returns")
                        logger.InsertFinanceLog(level, message, fullMessage, user);
                    else
                        if (controller == "Purchasing")
                            logger.InsertPurchasingLog(level, message, fullMessage, user);
                        else
                            if (controller == "WebOrdering")
                                logger.InsertWebOrderingLog(level, message, fullMessage, user);
            }
        }
    }
}
