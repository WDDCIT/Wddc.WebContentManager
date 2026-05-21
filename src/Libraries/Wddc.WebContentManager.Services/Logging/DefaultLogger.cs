using Microsoft.AspNetCore.Http;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.WebOrder;


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
        private readonly IWddcAppsApiService _appsApiService;

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

        public DefaultLogger(IHttpContextAccessor httpContextAccessor, IWddcAppsApiService appsApiService)
        {
            _httpContextAccessor = httpContextAccessor;
            _appsApiService = appsApiService;
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

        public virtual async Task<IPagedList<WebOrderingLog>> GetAllWebOrderingLogsAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = "", LogLevel? logLevel = null,
            int pageIndex = 1, int pageSize = int.MaxValue,
            string referrerUrl = "")
        {
            int? logLevelId = null;

            if (logLevel.HasValue)
                logLevelId = (int)logLevel.Value;

            List<WebOrderingLog> query = await _appsApiService.GetAsync<List<WebOrderingLog>>($"/api/WebOrdering/Logs?fromUtc={fromUtc}&toUtc={toUtc}&message={message}&logLevelId={logLevelId}&pageSize=500");

            if (!string.IsNullOrEmpty(referrerUrl))
                query = query.Where(l => l.ReferrerUrl != null && l.ReferrerUrl.Contains(referrerUrl, StringComparison.OrdinalIgnoreCase)).ToList();

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


        public async Task<WebOrderingLog> InsertWebOrderingLog(LogLevel logLevel, string shortMessage, string fullMessage = "", IPrincipal user = null)
        {
            // Capture HttpContext values synchronously before any await — context may be unavailable after continuation resumes
            var httpContext = _httpContextAccessor.HttpContext;
            var log = new WebOrderingLog
            {
                LogLevelId = (int)logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "",
                User = user?.Identity?.Name?.Substring(7)?.ToLower(),
                PageUrl = httpContext?.Request?.Path.ToString() ?? "",
                ReferrerUrl = httpContext?.Request?.Headers["Referer"].ToString() ?? "",
                CreatedOnUtc = DateTime.UtcNow
            };

            return await _appsApiService.PostAsync<WebOrderingLog>($"/api/WebOrdering/Log/", log);
        }

        #endregion

    }
}
