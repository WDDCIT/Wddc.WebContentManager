using System.Collections.Generic;
using Wddc.Api.Core.Domain.Entities.WebOrder;


namespace Wddc.WebContentManager.Models.WebContent.ClassifiedAds
{
    public class WebClassifiedAdsModel
    {
        public IEnumerable<Web_Classified_Ads> WebClassifiedAds { get; set; }
        public IEnumerable<Web_Classified_Ads> WebClassifiedAdsActive { get; set; }
        public IEnumerable<Web_Classified_Ads> WebClassifiedAdsExpired { get; set; }
    }
}