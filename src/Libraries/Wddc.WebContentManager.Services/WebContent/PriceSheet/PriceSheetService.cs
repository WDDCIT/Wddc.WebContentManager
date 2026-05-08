using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.PriceSheet
{
    public class PriceSheetService : IPriceSheetService
    {
        private readonly IWddcAppsApiService _apiService;

        public PriceSheetService(IWddcAppsApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<PriceSheetDto>> GetPriceSheets()
        {
            return await _apiService.GetAsync<List<PriceSheetDto>>("/api/PriceSheet/PriceSheets");
        }

        public async Task<PriceSheetDto> GetPriceSheetById(int id)
        {
            return await _apiService.GetAsync<PriceSheetDto>($"/api/PriceSheet/PriceSheet/{id}");
        }

        public async Task<PriceSheetDto> CreatePriceSheet(PriceSheetDto priceSheet)
        {
            return await _apiService.PostAsync<PriceSheetDto>("/api/PriceSheet/PriceSheet", priceSheet);
        }

        public async Task<PriceSheetDto> UpdatePriceSheet(int id, PriceSheetDto priceSheet)
        {
            return await _apiService.PostAsync<PriceSheetDto>($"/api/PriceSheet/PriceSheet/{id}", priceSheet);
        }

        public async Task DeletePriceSheet(int id)
        {
            await _apiService.PostAsync<object>($"/api/PriceSheet/PriceSheet/Delete/{id}");
        }
    }
}
