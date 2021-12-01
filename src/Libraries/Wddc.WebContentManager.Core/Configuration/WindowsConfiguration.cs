using System;
using System.Collections.Generic;
using System.Text;

namespace Wddc.WebContentManager.Core.Configuration
{
    public class WindowsConfiguration : IWindowsConfiguration
    {
        public string Domain { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
