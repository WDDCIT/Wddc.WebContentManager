using System.Collections.Generic;
using Wddc.Api.Core.Domain.Entities.WebOrder;


namespace Wddc.WebContentManager.Models.WebContent.Videos
{
    public class VideosModel
    {
        public IEnumerable<VID001> VID001s { get; set; }
        public IEnumerable<VID002> VID002s { get; set; }
    }
}