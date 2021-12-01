using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wddc.WebContentManager.Core.Configuration
{
    public class RootConfiguration : IRootConfiguration
    {
        public IOpenIdConfiguration OpenIdConfiguration { get; set; }
        public IResourceConfiguration ResourceConfiguration { get; set; }
        public IDynamicsGPConfiguration DynamicsGPConfiguration { get; set; }

        public RootConfiguration(IOptions<OpenIdConfiguration> openIdConfiguration, IOptions<ResourceConfiguration> resourceConfiguration, IOptions<DynamicsGPConfiguration> dynamicsOptions)
        {
            OpenIdConfiguration = openIdConfiguration.Value;
            ResourceConfiguration = resourceConfiguration.Value;
            DynamicsGPConfiguration = dynamicsOptions.Value;
        }
    }
}
