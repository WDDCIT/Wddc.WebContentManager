
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.TICatalog;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Search
{
    public class SearchService : ISearchService
    {
        private readonly IWddcApiService _apiService;

        public SearchService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<tblSearch>> GetAllTblSearch()
        {
            return await _apiService.GetAsync<List<tblSearch>>($"/api/Search/tblSearch");
        }

        public async Task<tblSearch> GetTblSearchBySearchDescr(string SearchDescr)
        {
            return await _apiService.GetAsync<tblSearch>($"/api/Search/tblSearch/{SearchDescr}");
        }

        public async Task<tblSearch> CreateTblSearch(tblSearch tblSearch)
        {
            return await _apiService.PostAsync<tblSearch>($"/api/Search/tblSearch", tblSearch);
        }

        public async Task UpdateTblSearch(tblSearch tblSearch, string SearchDescr)
        {
            await _apiService.PostAsync($"/api/Search/tblSearch/{SearchDescr}", tblSearch);
        }

        public async Task DeleteTblSearch(string SearchDescr)
        {
            await _apiService.DeleteAsync($"/api/Search/tblSearch/{SearchDescr}");
        }

        

        public async Task<GetItemInfo_Result> GetItemInfo(string ItemNumber)
        {
            return await _apiService.GetAsync<GetItemInfo_Result>($"/api/LiquidationSale/GetItemInfo/{ItemNumber}");
        }

    }
}
