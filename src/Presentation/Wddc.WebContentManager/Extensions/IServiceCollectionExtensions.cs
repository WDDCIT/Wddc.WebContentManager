using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;
using Wddc.PurchasingOrderApp.Services;
using Wddc.WebContentManager.Core.Configuration;
using Wddc.WebContentManager.Services;
using Wddc.WebContentManager.Services.Logging;
using Wddc.WebContentManager.Core.Caching;
using Wddc.WebContentManager.Core.Infrastructure;
using Wddc.WebContentManager.Core.Infrastructure.Mapper;
using System.Collections.Generic;
using AutoMapper;
using System.Linq;
using Wddc.WebContentManager.Services.WebContent.ContinuingEducation;
using Wddc.WebContentManager.Services.WebContent.ClassifiedAds;
using Wddc.WebContentManager.Services.WebContent.Careers;
using Wddc.WebContentManager.Services.WebContent.Sales.LiquidationSale;
using Wddc.WebContentManager.Services.WebContent.Sales.RetailClearanceSale;
using Wddc.WebContentManager.Services.WebContent.Sales.EquipmentClearanceSale;
using Wddc.WebContentManager.Services.WebContent.Videos;
using Wddc.WebContentManager.Services.WebContent.Search;
using Wddc.WebContentManager.Services.WebUser;
using Wddc.WebContentManager.Services.WebContent.Affinity;
using Wddc.WebContentManager.Services.WebContent.Newsletter;
using Wddc.WebContentManager.Services.WebContent.ClientVantageBanners;
using Wddc.WebContentManager.Services.WebContent.GenericMessage;
using Wddc.WebContentManager.Services.WebContent.Vendors;

namespace Wddc.WebContentManager.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IDynamicsGPManager, DynamicsGPManager>();
            return services;
        }

        /// <summary>
        /// Configuration root configuration
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureRootConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<OpenIdConfiguration>(configuration.GetSection(ConfigurationConsts.OpenIdConfigurationKey));
            services.Configure<ResourceConfiguration>(configuration.GetSection(ConfigurationConsts.ResourceConfigurationKey));
            services.Configure<DynamicsGPConfiguration>(configuration.GetSection(ConfigurationConsts.DynamicsGPConfigurationKey));

            services.TryAddSingleton<IRootConfiguration, RootConfiguration>();
            services.TryAddSingleton<IWddcApiService, WddcApiService>();
            services.TryAddSingleton<ILogger, DefaultLogger>();
            services.TryAddSingleton<ICacheManager, MemoryCacheManager>();
            services.TryAddSingleton<IContinuingEducationService, ContinuingEducationService>();
            services.TryAddSingleton<IClassifiedAdsService, ClassifiedAdsService>();
            services.TryAddSingleton<ICareersService, CareersService>();
            services.TryAddSingleton<ILiquidationSaleService, LiquidationSaleService>();
            services.TryAddSingleton<IRetailClearanceSaleService, RetailClearanceSaleService>();
            services.TryAddSingleton<IEquipmentClearanceSaleService, EquipmentClearanceSaleService>();
            services.TryAddSingleton<IVideosService, VideosService>();
            services.TryAddSingleton<ISearchService, SearchService>();
            services.TryAddSingleton<IWebUserLookupService, WebUserLookupService>();
            services.TryAddSingleton<IAffinityService, AffinityService>();
            services.TryAddSingleton<INewsletterService, NewsletterService>();
            services.TryAddSingleton<IClientVantageBannersService, ClientVantageBannersService>();
            services.TryAddSingleton<IGenericMessageService, GenericMessageService>();
            services.TryAddSingleton<IItemInfoService, ItemInfoService>();
            services.TryAddSingleton<IVendorListService, VendorListService>();

            //dependencies
            var typeFinder = new WebAppTypeFinder();

            //register mapper configurations provided by other assemblies
            var mcTypes = typeFinder.FindClassesOfType<IMapperConfiguration>();
            var mcInstances = new List<IMapperConfiguration>();
            foreach (var mcType in mcTypes)
                mcInstances.Add((IMapperConfiguration)Activator.CreateInstance(mcType));
            //sort
            mcInstances = mcInstances.AsQueryable().OrderBy(t => t.Order).ToList();
            //get configurations
            var configurationActions = new List<Action<IMapperConfigurationExpression>>();
            foreach (var mc in mcInstances)
                configurationActions.Add(mc.GetConfiguration());
            //register
            AutoMapperConfiguration.Init(configurationActions);

            return services;
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services, IRootConfiguration rootConfiguration)
        {
            services.AddHttpClient("wddc_api", c =>
            {
                var client = new HttpClient();
                var openIdConfiguration = rootConfiguration.OpenIdConfiguration;
                var disco = client.GetDiscoveryDocumentAsync(openIdConfiguration.IdentityServerBaseUrl).Result;

                if (disco.IsError)
                    throw new Exception(disco.Error);

                var tokenResponse = client.RequestPasswordTokenAsync(new PasswordTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = openIdConfiguration.ClientId,
                    ClientSecret = openIdConfiguration.ClientSecret,

                    UserName = "admin",
                    Password = "X15XRTe$zqGS",
                    Scope = string.Join(" ", openIdConfiguration.Scopes)
                }).Result;

                c.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenResponse.AccessToken);

                // Other settings
                c.BaseAddress = new Uri(rootConfiguration.ResourceConfiguration.WddcApiBaseUrl);
            });

            return services;
        }
    }
}
