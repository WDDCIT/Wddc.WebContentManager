
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.TiCatalogWebsrvr;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Search
{
    public class SearchService : ISearchService
    {
        private readonly IWddcAppsApiService _apiService;

        public SearchService(IWddcAppsApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<TblSearch>> GetAllTblSearch()
        {
            return await _apiService.GetAsync<List<TblSearch>>("/api/Search/tblSearch");
        }

        public async Task<TblSearch> GetTblSearchBySearchDescr(string SearchDescr)
        {
            return await _apiService.GetAsync<TblSearch>($"/api/Search/tblSearch/{SearchDescr}");
        }

        public async Task<TblSearch> CreateTblSearch(TblSearch tblSearch)
        {
            return await _apiService.PostAsync<TblSearch>("/api/Search/tblSearch", tblSearch);
        }

        public async Task<TblSearch> UpdateTblSearch(string SearchDescr, TblSearch tblSearch)
        {
            return await _apiService.PostAsync<TblSearch>($"/api/Search/tblSearch/{SearchDescr}", tblSearch);
        }

        public async Task DeleteTblSearch(string SearchDescr)
        {
            await _apiService.DeleteAsync<object>($"/api/Search/tblSearch/{SearchDescr}");
        }

        public async Task<GetItemInfo_Result> GetItemInfo(string ItemNumber)
        {
            return await _apiService.GetAsync<GetItemInfo_Result>($"/api/LiquidationSale/GetItemInfo/{ItemNumber}");
        }
    }
}
