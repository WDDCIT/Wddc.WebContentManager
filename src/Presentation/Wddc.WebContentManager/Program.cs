using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Sentry;
using Serilog;
using Serilog.Events;
using System;


namespace Wddc.WebContentManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.SqlServer", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Sentry(o =>
                {
                    o.Dsn = new Dsn("https://bfeda7e727a94353be97a07357f2fa2e@sentry.io/1540220");
                    o.MinimumEventLevel = LogEventLevel.Error;
                })
                .WriteTo.File($"\\logs\\{System.Reflection.Assembly.GetEntryAssembly().GetName().Name}_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.log.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
