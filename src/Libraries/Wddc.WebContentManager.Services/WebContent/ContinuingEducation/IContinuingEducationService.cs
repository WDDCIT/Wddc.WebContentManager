
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.ContinuingEducation
{
    public interface IContinuingEducationService
    {
        Task<List<Web_CE_Ads>> GetAllWebCEAds();
        Task<Web_CE_Ads> GetWebCEAdsById(int Ad_ID);
        Task<Web_CE_Ads> CreateWebCEAd(Web_CE_Ads Web_CE_Ads);
        Task DeleteWebCEAd(int Ad_ID);
        Task UpdateWebCEAd(Web_CE_Ads Web_CE_Ad, int Ad_ID);
    }
}
