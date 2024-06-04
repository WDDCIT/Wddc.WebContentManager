
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.AppData.Items;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public class ItemInfoService : IItemInfoService
    {
        private readonly IWddcApiService _apiService;

        public ItemInfoService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Items>> GetItems()
        {
            return await _apiService.GetAsync<List<Items>>($"/api/Items/GetItems");
        }



    }
}
