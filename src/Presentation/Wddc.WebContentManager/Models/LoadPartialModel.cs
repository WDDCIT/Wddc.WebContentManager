using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Wddc.WebContentManager.Models
{
    public class LoadPartialModel
    {
        public LoadPartialModel()
        { }

        public LoadPartialModel(string id, string url, string loadingTitle)
        {
            Url = url;
            Id = id;
            LoadingTitle = loadingTitle;
        }
        public string Url { get; set; }
        public string Id { get; set; }
        public string LoadingTitle { get; set; }
    }
}