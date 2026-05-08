
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.ClientVantageBanners
{
    public interface IClientVantageBannersService
    {
        Task<List<BannerDto>> GetBanners();
        Task<List<BannerCategoryDto>> GetCategories();
        Task<List<MediaSettingDto>> GetSettings();
        Task<BannerDto> AddBanner(BannerDto banner);
        Task<BannerCategoryMappingDto> AddBannerCategoryMapping(BannerCategoryMappingDto bannerCategoryMapping);
        Task<List<BannersOfCategoryDto>> GetBannersOfCategory(int CategoryId);
        Task<BannerDto> GetBannerById(int Id);
        Task<BannerCategoryMappingDto> GetBannerCategoryMappingByBannerId(int BannerId);
        Task UpdateBanner(BannerDto banner, int Id);
        Task UpdateBannerCategoryMapping(BannerCategoryMappingDto bannerCategoryMapping, int BannerId);
        Task DeleteBannerCategoryMapping(BannerCategoryMappingDto bannerCategoryMapping);
        Task DeleteBanner(int Id);
    }
}
