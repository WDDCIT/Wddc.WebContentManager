using System;
using System.Collections.Generic;
using System.Text;

namespace Wddc.WebContentManager.Core.Configuration
{
    public interface IRootConfiguration
    {
        IOpenIdConfiguration OpenIdConfiguration { get; }
        IResourceConfiguration ResourceConfiguration { get; }
        IDynamicsGPConfiguration DynamicsGPConfiguration { get; }
    }
}
