using PagedList;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Wddc.Core.Domain.EdiOrdering.Finance.Logging;
using Wddc.Core.Domain.Purchasing.Logging;
using Wddc.Core.Domain.Shipping.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;

namespace Wddc.WebContentManager.Services.Logging
{

    /// <summary>
    /// Logger interface
    /// </summary>
    public partial interface ILogger
    {
        /// <summary>
        /// Determines whether a log level is enabled
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Result</returns>
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// Deletes a log item
        /// </summary>
        /// <param name="log">Log item</param>
        //void DeleteLog(Log log);

        /// <summary>
        /// Deletes a log items
        /// </summary>
        /// <param name="logs">Log items</param>
        //void DeleteLogs(IList<Log> logs);

        /// <summary>
        /// Clears a log
        /// </summary>
        //void ClearLog();

        /// <summary>
        /// Gets all log items
        /// </summary>
        /// <param name="fromUtc">Log item creation from; null to load all records</param>
        /// <param name="toUtc">Log item creation to; null to load all records</param>
        /// <param name="message">Message</param>
        /// <param name="logLevel">Log level; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Log item items</returns>
        Task<IPagedList<Log>> GetAllShippingLogsAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 1, int pageSize = int.MaxValue,
            string referrerUrl = "");

        Task<IPagedList<FinanceLog>> GetAllFinanceLogsAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 1, int pageSize = int.MaxValue,
            string referrerUrl = "");

        Task<IPagedList<PurchasingLog>> GetAllPurchasingLogsAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 1, int pageSize = int.MaxValue,
            string referrerUrl = "");

        Task<IPagedList<WebOrderingLog>> GetAllWebOrderingLogsAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 1, int pageSize = int.MaxValue,
            string referrerUrl = "");

        /// <summary>
        /// Gets a log item
        /// </summary>
        /// <param name="logId">Log item identifier</param>
        /// <returns>Log item</returns>
        //Log GetLogById(int logId);

        /// <summary>
        /// Get log items by identifiers
        /// </summary>
        /// <param name="logIds">Log item identifiers</param>
        /// <returns>Log items</returns>
        //IList<Log> GetLogByIds(int[] logIds);

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="shortMessage">The short message</param>
        /// <param name="fullMessage">The full message</param>
        /// <param name="customer">The customer to associate log record with</param>
        /// <returns>A log item</returns>
        Task<Log> InsertShippingLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null);
        Task<FinanceLog> InsertFinanceLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null);
        Task<PurchasingLog> InsertPurchasingLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null);
        Task<WebOrderingLog> InsertWebOrderingLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null);
    }
}