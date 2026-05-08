using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.ClientVantageBanners
{
    public class ClientVantageBannersService : IClientVantageBannersService
    {
        private readonly IWddcAppsApiService _apiService;

        public ClientVantageBannersService(IWddcAppsApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<BannerDto>> GetBanners()
        {
            return await _apiService.GetAsync<List<BannerDto>>("/api/Banners/Banner");
        }

        public async Task<List<BannerCategoryDto>> GetCategories()
        {
            return await _apiService.GetAsync<List<BannerCategoryDto>>("/api/Banners/Category");
        }

        public async Task<List<MediaSettingDto>> GetSettings()
        {
            return await _apiService.GetAsync<List<MediaSettingDto>>("/api/Banners/Setting");
        }

        public async Task<BannerDto> AddBanner(BannerDto banner)
        {
            return await _apiService.PostAsync<BannerDto>("/api/Banners/Banner", banner);
        }

        public async Task<BannerCategoryMappingDto> AddBannerCategoryMapping(BannerCategoryMappingDto bannerCategoryMapping)
        {
            return await _apiService.PostAsync<BannerCategoryMappingDto>("/api/Banners/BannerCategoryMapping", bannerCategoryMapping);
        }

        public async Task<List<BannersOfCategoryDto>> GetBannersOfCategory(int CategoryId)
        {
            return await _apiService.GetAsync<List<BannersOfCategoryDto>>($"/api/Banners/BannersOfCategory/{CategoryId}");
        }

        public async Task<BannerDto> GetBannerById(int Id)
        {
            return await _apiService.GetAsync<BannerDto>($"/api/Banners/Banner/{Id}");
        }

        public async Task<BannerCategoryMappingDto> GetBannerCategoryMappingByBannerId(int BannerId)
        {
            return await _apiService.GetAsync<BannerCategoryMappingDto>($"/api/Banners/BannerCategoryMapping/{BannerId}");
        }

        public async Task UpdateBanner(BannerDto banner, int Id)
        {
            await _apiService.PostAsync<BannerDto>($"/api/Banners/Banner/{Id}", banner);
        }

        public async Task UpdateBannerCategoryMapping(BannerCategoryMappingDto bannerCategoryMapping, int BannerId)
        {
            await _apiService.PostAsync<BannerCategoryMappingDto>($"/api/Banners/BannerCategoryMapping/{BannerId}", bannerCategoryMapping);
        }

        public async Task DeleteBannerCategoryMapping(BannerCategoryMappingDto bannerCategoryMapping)
        {
            await _apiService.PostAsync<BannerCategoryMappingDto>("/api/Banners/BannerCategoryMapping/Delete", bannerCategoryMapping);
        }

        public async Task DeleteBanner(int Id)
        {
            await _apiService.PostAsync<object>($"/api/Banners/Banner/Delete/{Id}");
        }
    }
}
