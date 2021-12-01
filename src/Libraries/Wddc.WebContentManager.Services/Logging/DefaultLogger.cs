using Microsoft.AspNetCore.Http;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Wddc.Core.Domain.EdiOrdering.Finance.Logging;
using Wddc.Core.Domain.Purchasing.Logging;
using Wddc.Core.Domain.Shipping.Logging;
using Wddc.Core.Domain.Webserver.WebOrdering.Logging;
using Wddc.Web.Core;
using Wddc.WebContentManager.Services.Logging;


namespace Wddc.WebContentManager.Services.Logging
{
    /// <summary>
    /// Default logger
    /// </summary>
    public partial class DefaultLogger : ILogger
    {
        #region Fields

        //private readonly IEdiRepository<Log> _logRepository;
        //private readonly EdiObjectContext _dbContext;
        //private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWddcApiService _apiService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logRepository">Log repository</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="dbContext">DB context</param>
        /// <param name="dataProvider">WeData provider</param>
        /// <param name="commonSettings">Common settings</param>
        //public DefaultLogger(IEdiRepository<Log> logRepository, EdiObjectContext dbContext,
        //    IWebHelper webHelper)
        //{
        //    this._logRepository = logRepository;
        //    this._dbContext = dbContext;
        //    this._webHelper = webHelper;
        //}

        public DefaultLogger(IHttpContextAccessor httpContextAccessor, IWddcApiService apiService)
        {
            _httpContextAccessor = httpContextAccessor;
            _apiService = apiService;
        }

        #endregion

        #region Utitilities

        /// <summary>
        /// Gets a value indicating whether this message should not be logged
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        protected virtual bool IgnoreLog(string message)
        {
            if (String.IsNullOrWhiteSpace(message))
                return false;

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether a log level is enabled
        /// </summary>
        /// <param name="level">Log level</param>
        /// <returns>Result</returns>
        public virtual bool IsEnabled(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return true;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Deletes a log item
        /// </summary>
        /// <param name="log">Log item</param>
        //public virtual void DeleteLog(Log log)
        //{
        //    if (log == null)
        //        throw new ArgumentNullException("log");

        //    _logRepository.Delete(log);
        //}

        /// <summary>
        /// Deletes a log items
        /// </summary>
        /// <param name="logs">Log items</param>
        //public virtual void DeleteLogs(IList<Log> logs)
        //{
        //    if (logs == null)
        //        throw new ArgumentNullException("logs");

        //    _logRepository.Delete(logs);
        //}

        /// <summary>
        /// Clears a log
        /// </summary>
        //public virtual void ClearLog()
        //{
        //    _dbContext.Database.ExecuteSqlCommand(String.Format("TRUNCATE TABLE [{0}]", "Route.Log"));
        //}

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
        public virtual async Task<IPagedList<Log>> GetAllShippingLogsAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 1, int pageSize = int.MaxValue,
            string referrerUrl = "")
        {
            int? logLevelId = null;

            if (logLevel.HasValue)
                logLevelId = (int)logLevel.Value;

            List<Log> query = await _apiService.GetAsync<List<Log>>($"/api/Shipping/Logs?fromUtc={fromUtc}&toUtc={toUtc}&message={message}&logLevelId={logLevelId}&referrerUrl={referrerUrl}");
            var log = query.ToPagedList(pageIndex, pageSize);
            return log;
        }

        public virtual async Task<IPagedList<FinanceLog>> GetAllFinanceLogsAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 1, int pageSize = int.MaxValue,
            string referrerUrl = "")
        {
            int? logLevelId = null;

            if (logLevel.HasValue)
                logLevelId = (int)logLevel.Value;

            List<FinanceLog> query = await _apiService.GetAsync<List<FinanceLog>>($"/api/Finance/Logs?fromUtc={fromUtc}&toUtc={toUtc}&message={message}&logLevelId={logLevelId}&referrerUrl={referrerUrl}");
            var log = query.ToPagedList(pageIndex, pageSize);
            return log;
        }

        public virtual async Task<IPagedList<PurchasingLog>> GetAllPurchasingLogsAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 1, int pageSize = int.MaxValue,
            string referrerUrl = "")
        {
            int? logLevelId = null;

            if (logLevel.HasValue)
                logLevelId = (int)logLevel.Value;

            List<PurchasingLog> query = await _apiService.GetAsync<List<PurchasingLog>>($"/api/Purchasing/Logs?fromUtc={fromUtc}&toUtc={toUtc}&message={message}&logLevelId={logLevelId}&referrerUrl={referrerUrl}");
            var log = query.ToPagedList(pageIndex, pageSize);
            return log;
        }

        public virtual async Task<IPagedList<WebOrderingLog>> GetAllWebOrderingLogsAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 1, int pageSize = int.MaxValue,
            string referrerUrl = "")
        {
            int? logLevelId = null;

            if (logLevel.HasValue)
                logLevelId = (int)logLevel.Value;

            List<WebOrderingLog> query = await _apiService.GetAsync<List<WebOrderingLog>>($"/api/WebOrdering/Logs?fromUtc={fromUtc}&toUtc={toUtc}&message={message}&logLevelId={logLevelId}&referrerUrl={referrerUrl}");
            var log = query.ToPagedList(pageIndex, pageSize);
            return log;
        }

        /// <summary>
        /// Gets a log item
        /// </summary>
        /// <param name="logId">Log item identifier</param>
        /// <returns>Log item</returns>
        //public virtual Log GetLogById(int logId)
        //{
        //    if (logId == 0)
        //        return null;

        //    return _logRepository.GetById(logId);
        //}

        /// <summary>
        /// Get log items by identifiers
        /// </summary>
        /// <param name="logIds">Log item identifiers</param>
        /// <returns>Log items</returns>
        //public virtual IList<Log> GetLogByIds(int[] logIds)
        //{
        //    if (logIds == null || logIds.Length == 0)
        //        return new List<Log>();

        //    var query = from l in _logRepository.Table
        //                where logIds.Contains(l.Id)
        //                select l;
        //    var logItems = query.ToList();
        //    //sort by passed identifiers
        //    var sortedLogItems = new List<Log>();
        //    foreach (int id in logIds)
        //    {
        //        var log = logItems.Find(x => x.Id == id);
        //        if (log != null)
        //            sortedLogItems.Add(log);
        //    }
        //    return sortedLogItems;
        //}

        /// <summary>
        /// Inserts a log item
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="shortMessage">The short message</param>
        /// <param name="fullMessage">The full message</param>
        /// <param name="customer">The customer to associate log record with</param>
        /// <returns>A log item</returns>
        public async Task<Log> InsertShippingLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null)
        {
            var log = new Log
            {
                LogLevelId = (int)logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                User = user?.Identity.Name.Substring(7).ToLower(),
                PageUrl = _httpContextAccessor.HttpContext.Request.Path.ToString(),
                ReferrerUrl = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString(),
                CreatedOnUtc = DateTime.UtcNow
            };

            return await _apiService.PostAsync<Log>($"/api/Shipping/Log/", log);
        }

        public async Task<FinanceLog> InsertFinanceLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null)
        {
            var log = new FinanceLog
            {
                LogLevelId = (int)logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                User = user?.Identity.Name.Substring(7).ToLower(),
                PageUrl = _httpContextAccessor.HttpContext.Request.Path.ToString(),
                ReferrerUrl = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString(),
                CreatedOnUtc = DateTime.UtcNow
            };

            return await _apiService.PostAsync<FinanceLog>($"/api/Finance/Log/", log);
        }

        public async Task<PurchasingLog> InsertPurchasingLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null)
        {
            var log = new PurchasingLog
            {
                LogLevelId = (int)logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                User = user?.Identity.Name.Substring(7).ToLower(),
                PageUrl = _httpContextAccessor.HttpContext.Request.Path.ToString(),
                ReferrerUrl = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString(),
                CreatedOnUtc = DateTime.UtcNow
            };

            return await _apiService.PostAsync<PurchasingLog>($"/api/Purchasing/Log/", log);
        }

        public async Task<WebOrderingLog> InsertWebOrderingLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null)
        {
            var log = new FinanceLog
            {
                LogLevelId = (int)logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                User = user?.Identity.Name.Substring(7).ToLower(),
                PageUrl = _httpContextAccessor.HttpContext.Request.Path.ToString(),
                ReferrerUrl = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString(),
                CreatedOnUtc = DateTime.UtcNow
            };

            return await _apiService.PostAsync<WebOrderingLog>($"/api/WebOrdering/Log/", log);
        }

        #endregion

    }
}
