using PagedList;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.EdiOrdering.Finance;
using Wddc.Api.Core.Domain.Entities.EdiOrdering.Routes;
using Wddc.Api.Core.Domain.Entities.Orders.Purchasing;
using Wddc.Api.Core.Domain.Entities.WebOrder;

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
        Task<WebOrderingLog> InsertWebOrderingLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null);
    }
}