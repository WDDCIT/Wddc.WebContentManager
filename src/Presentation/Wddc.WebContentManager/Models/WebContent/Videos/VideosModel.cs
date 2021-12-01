using System.Collections.Generic;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Models.WebContent.Videos
{
    public class VideosModel
    {
        public IEnumerable<VID001> VID001s { get; set; }
        public IEnumerable<VID002> VID002s { get; set; }
    }
}