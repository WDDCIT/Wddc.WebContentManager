
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.WebOrder;

namespace Wddc.WebContentManager.Services.WebContent.ClassifiedAds
{
    public class ClassifiedAdsService : IClassifiedAdsService
    {
        private readonly IWddcAppsApiService _apiService;

        public ClassifiedAdsService(IWddcAppsApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Web_Classified_Ads>> GetAllWebClassifiedAds()
        {
            return await _apiService.GetAsync<List<Web_Classified_Ads>>("/api/ClassifiedAds/Web_Classified_Ads");
        }

        public async Task<Web_Classified_Ads> GetWebClassifiedAdById(int Ad_ID)
        {
            return await _apiService.GetAsync<Web_Classified_Ads>($"/api/ClassifiedAds/Web_Classified_Ads/{Ad_ID}");
        }

        public async Task<Web_Classified_Ads> CreateWebClassifiedAd(Web_Classified_Ads Web_Classified_Ad)
        {
            return await _apiService.PostAsync<Web_Classified_Ads>("/api/ClassifiedAds/Web_Classified_Ads", Web_Classified_Ad);
        }

        public async Task DeleteWebClassifiedAd(int Ad_ID)
        {
            await _apiService.SendAsync(HttpMethod.Post, $"/api/ClassifiedAds/Web_Classified_Ads/Delete/{Ad_ID}");
        }

        public async Task UpdateWebClassifiedAd(Web_Classified_Ads Web_Classified_Ad, int Ad_ID)
        {
            await _apiService.SendAsync(HttpMethod.Post, $"/api/ClassifiedAds/Web_Classified_Ads/{Ad_ID}", Web_Classified_Ad);
        }

        public async Task<string> GetMemberName(string MemberNbr)
        {
            return await _apiService.GetStringAsync($"/api/ClassifiedAds/GetMemberName/{MemberNbr}");
        }
    }
}
