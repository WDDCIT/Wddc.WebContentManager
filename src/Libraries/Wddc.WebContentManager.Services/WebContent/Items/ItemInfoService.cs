
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Models.Items;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public class ItemInfoService : IItemInfoService
    {
        private readonly IWddcAppsApiService _apiService;

        public ItemInfoService(IWddcAppsApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<ItemSummary>> GetItems()
        {
            return await _apiService.GetAsync<List<ItemSummary>>("/api/Items/GetItems");
        }
    }
}
