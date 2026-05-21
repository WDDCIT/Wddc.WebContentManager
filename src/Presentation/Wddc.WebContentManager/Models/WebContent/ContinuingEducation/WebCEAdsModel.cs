using System.Collections.Generic;
using Wddc.Api.Core.Domain.Entities.WebOrder;


namespace Wddc.WebContentManager.Models.WebContent.ContinuingEducation
{
    public class WebCEAdsModel
    {
        public IEnumerable<Web_CE_Ads> WebCEAds { get; set; }
        public IEnumerable<Web_CE_Ads> WebCEAdsActive { get; set; }
        public IEnumerable<Web_CE_Ads> WebCEAdsExpired { get; set; }
    }
}