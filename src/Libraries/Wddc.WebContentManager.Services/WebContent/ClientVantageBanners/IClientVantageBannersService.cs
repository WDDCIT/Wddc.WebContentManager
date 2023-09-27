
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Media;

namespace Wddc.WebContentManager.Services.WebContent.ClientVantageBanners
{
    public interface IClientVantageBannersService
    {
        Task<List<Banner>> GetBanners();
        Task<List<Category>> GetCategories();
        Task<List<Setting>> GetSettings();
        Task<Banner> AddBanner(Banner banner);
        Task<Banner_Category_Mapping> AddBannerCategoryMapping(Banner_Category_Mapping bannerCategoryMapping);
        Task<List<BannersOfCategory>> GetBannersOfCategory(int CategoryId);
        Task<Banner> GetBannerById(int Id);
        Task<Banner_Category_Mapping> GetBannerCategoryMappingByBannerId(int BannerId);
        Task UpdateBanner(Banner banner, int Id);
        Task UpdateBannerCategoryMapping(Banner_Category_Mapping bannerCategoryMapping, int BannerId);
        Task DeleteBannerCategoryMapping(Banner_Category_Mapping bannerCategoryMapping);
        Task DeleteBanner(int Id);
    }
}
