using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Web;
using DynamicsGPServiceReference;
using Microsoft.Extensions.Configuration;
using Wddc.WebContentManager.Core.Configuration;

namespace Wddc.PurchasingOrderApp.Services
{
    public class DynamicsGPManager : IDynamicsGPManager
    {
        private readonly IConfiguration configuration;
        private readonly IRootConfiguration rootConfiguration;

        public DynamicsGPManager(IRootConfiguration rootConfiguration, IConfiguration configuration)
        {
            this.configuration = configuration;
            this.rootConfiguration = rootConfiguration;
        }

        public DynamicsGPClient GetClient()
        {
            var wsDynamicsGP = new DynamicsGPClient();

            var section = configuration.GetSection("DynamicsGP");
            wsDynamicsGP.Endpoint.Address = new EndpointAddress(section.GetValue<string>("Url"));

            var serviceSection = section.GetSection("Service");
            wsDynamicsGP.ClientCredentials.UserName.UserName = serviceSection.GetValue<string>("Username");
            wsDynamicsGP.ClientCredentials.UserName.Password = serviceSection.GetValue<string>("Password");

            var windowsSection = section.GetSection("Windows");
            wsDynamicsGP.ClientCredentials.Windows.ClientCredential.Domain
                = windowsSection.GetValue<string>("Domain");
            wsDynamicsGP.ClientCredentials.Windows.ClientCredential.UserName = windowsSection.GetValue<string>("Username");
            wsDynamicsGP.ClientCredentials.Windows.ClientCredential.Password = windowsSection.GetValue<string>("Password");

            return wsDynamicsGP;
        }
    }
}