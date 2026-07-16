using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Text;
using Wddc.WebContentManager.Core.Configuration;
using Wddc.WebContentManager.Extensions;

namespace Wddc.WebContentManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Apply SafeContractResolver globally so that JsonConvert calls in
            // WddcApiService (deserialization) also resolve the Id / ID collision.
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new SafeContractResolver()
            };

            // For loading files with Windows-1252 encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = int.MaxValue;
            });

            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.AddAuthentication(IISDefaults.AuthenticationScheme);

            // Required for Kendo Grids to populate
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new SafeContractResolver());

            // Setup configuration
            services.ConfigureRootConfiguration(Configuration);


            var rootConfiguration = services.BuildServiceProvider().GetService<IRootConfiguration>();

            services.AddHttpClients(rootConfiguration);
            
            // register services
            services.RegisterServices();

            // Add the Kendo UI services to the services container.
            services.AddKendo();

            services.AddMvc();
            services.AddHttpContextAccessor();

            services.AddRazorPages()
                    .AddSessionStateTempDataProvider();
            services.AddControllersWithViews()
                    .AddSessionStateTempDataProvider();

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.Name = ".ContentManager.Session";
                options.IdleTimeout = TimeSpan.FromSeconds(30);
                options.Cookie.IsEssential = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == Environments.Development)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseSession();
            app.UseCors();
            app.UseAuthorization();

            // Identity convergence after a sign-out/log-in switch: Windows auth is
            // per-connection, so right after logging in as a different account, requests
            // can still ride keep-alive connections authenticated as the PREVIOUS
            // account. While the pin cookie set by AccountController.LoginChallenge is
            // present, re-challenge any request arriving as a different account — the
            // browser silently retries with the credentials typed at the login prompt,
            // converting the connection. Account endpoints are exempt (they manage the
            // login dance themselves).
            app.Use(async (context, next) =>
            {
                var expected = context.Request.Cookies[Controllers.AccountController.ExpectedUserCookie];
                if (expected != null
                    && !context.Request.Path.StartsWithSegments("/Account")
                    && context.User.Identity?.IsAuthenticated == true
                    && Controllers.AccountController.Hash(context.User.Identity.Name) != expected)
                {
                    await context.ChallengeAsync(IISDefaults.AuthenticationScheme);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                await next();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });


        }
    }
}
