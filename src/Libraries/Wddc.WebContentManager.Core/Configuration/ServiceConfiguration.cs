using System;
using System.Collections.Generic;
using System.Text;

namespace Wddc.WebContentManager.Core.Configuration
{
    public class ServiceConfiguration : IServiceConfiguration
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
