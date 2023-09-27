using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Media;

namespace Wddc.WebContentManager.Services.WebContent.ClientVantageBanners
{
    public class ClientVantageBannersService : IClientVantageBannersService
    {
        private readonly IWddcApiService _apiService;

        public ClientVantageBannersService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Banner>> GetBanners()
        {
            return await _apiService.GetAsync<List<Banner>>($"/api/Banners/Banner");
        }

        public async Task<List<Category>> GetCategories()
        {
            return await _apiService.GetAsync<List<Category>>($"/api/Banners/Category");
        }

        public async Task<List<Setting>> GetSettings()
        {
            return await _apiService.GetAsync<List<Setting>>($"/api/Banners/Setting");
        }

        public async Task<Banner> AddBanner(Banner banner)
        {
            return await _apiService.PostAsync<Banner>($"/api/Banners/Banner", banner);
        }

        public async Task<Banner_Category_Mapping> AddBannerCategoryMapping(Banner_Category_Mapping bannerCategoryMapping)
        {
            return await _apiService.PostAsync<Banner_Category_Mapping>($"/api/Banners/BannerCategoryMapping", bannerCategoryMapping);
        }

        public async Task<List<BannersOfCategory>> GetBannersOfCategory(int CategoryId)
        {
            return await _apiService.GetAsync<List<BannersOfCategory>>($"/api/Banners/BannersOfCategory/{CategoryId}");
        }

        public async Task<Banner> GetBannerById(int Id)
        {
            return await _apiService.GetAsync<Banner>($"/api/Banners/Banner/{Id}");
        }

        public async Task<Banner_Category_Mapping> GetBannerCategoryMappingByBannerId(int BannerId)
        {
            return await _apiService.GetAsync<Banner_Category_Mapping>($"/api/Banners/BannerCategoryMapping/{BannerId}");
        }

        public async Task UpdateBanner(Banner banner, int Id)
        {
            await _apiService.PostAsync($"/api/Banners/Banner/{Id}", banner);
        }

        public async Task UpdateBannerCategoryMapping(Banner_Category_Mapping bannerCategoryMapping, int BannerId)
        {
            await _apiService.PostAsync($"/api/Banners/BannerCategoryMapping/{BannerId}", bannerCategoryMapping);
        }

        public async Task DeleteBannerCategoryMapping(Banner_Category_Mapping bannerCategoryMapping)
        {
            await _apiService.PostAsync($"/api/Banners/BannerCategoryMapping/Delete", bannerCategoryMapping);
        }

        public async Task DeleteBanner(int Id)
        {
            await _apiService.PostAsync($"/api/Banners/Banner/Delete/{Id}");
        }

    }
}
