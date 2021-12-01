using Serilog;

namespace Wddc.PurchasingOrderApp.Services
{
    public class LogManager
    {
        public void WriteInfo(string logMessage)
        {
            Log.Logger.Information(logMessage);

        }

        public void WriteDebug(string debugMessage)
        {
            Log.Logger.Debug(debugMessage);
            Log.Logger.Error(debugMessage);
            Log.Logger.Error(debugMessage);
        }
    }
}