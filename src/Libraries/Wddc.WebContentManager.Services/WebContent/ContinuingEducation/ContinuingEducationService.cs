
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.ContinuingEducation
{
    public class ContinuingEducationService : IContinuingEducationService
    {
        private readonly IWddcApiService _apiService;

        public ContinuingEducationService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Web_CE_Ads>> GetAllWebCEAds()
        {
            return await _apiService.GetAsync<List<Web_CE_Ads>>($"/api/ContinuingEducation/Web_CE_Ads");
        }

        public async Task<Web_CE_Ads> GetWebCEAdsById(int Ad_ID)
        {
            return await _apiService.GetAsync<Web_CE_Ads>($"/api/ContinuingEducation/Web_CE_Ads/{Ad_ID}");
        }

        public async Task<Web_CE_Ads> CreateWebCEAd(Web_CE_Ads Web_CE_Ad)
        {
            Web_CE_Ads newWeb_CE_Ad = await _apiService.PostAsync<Web_CE_Ads>($"/api/ContinuingEducation/Web_CE_Ads", Web_CE_Ad);
            return newWeb_CE_Ad;
        }

        public async Task DeleteWebCEAd(int Ad_ID)
        {
            await _apiService.PostAsync($"/api/ContinuingEducation/Web_CE_Ads/Delete/{Ad_ID}");
        }

        public async Task UpdateWebCEAd(Web_CE_Ads Web_CE_Ad, int Ad_ID)
        {
            await _apiService.PostAsync($"/api/ContinuingEducation/Web_CE_Ads/{Ad_ID}", Web_CE_Ad);
        }
    }
}
