using System;
using System.Collections.Generic;
using System.Text;

namespace Wddc.WebContentManager.Core.Configuration
{
    public class DynamicsGPConfiguration : IDynamicsGPConfiguration
    {
        public string Url { get; set; }

        public IServiceConfiguration ServiceConfiguration { get; set; }

        public IWindowsConfiguration WindowsConfiguration { get; set; }
    }
}
