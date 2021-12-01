using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wddc.Web.Framework.Models
{
    public class BaseModel
    {
        public bool ShowSideBar { get; set; }
        public ICollection<TabListItem> TabListItems { get; set; }

        public BaseModel()
        {
            ShowSideBar = true;
            TabListItems = new List<TabListItem>();
        }
    }

    public class TabListItem
    {
        public string Name { get; set; }
        public string Id { get; set; }

        /// <summary>
        /// Class to use for the tab list items icon
        /// </summary>
        public string ImageClass { get; set; }
    }
}