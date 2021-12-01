
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.ClassifiedAds
{
    public interface IClassifiedAdsService
    {
        Task<List<Web_Classified_Ads>> GetAllWebClassifiedAds();
        Task<Web_Classified_Ads> GetWebClassifiedAdById(int Ad_ID);
        Task<Web_Classified_Ads> CreateWebClassifiedAd(Web_Classified_Ads Web_Classified_Ad);
        Task DeleteWebClassifiedAd(int Ad_ID);
        Task UpdateWebClassifiedAd(Web_Classified_Ads Web_Classified_Ad, int Ad_ID);
        Task<string> GetMemberName(string MemberNbr);
    }
}
