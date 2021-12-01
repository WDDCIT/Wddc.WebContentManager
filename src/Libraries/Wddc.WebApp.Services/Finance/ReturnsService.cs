using System.Threading.Tasks;

namespace Wddc.WebApp.Services.Finance
{
    public class ReturnsService : IReturnsService
    {
        private readonly IWddcApiService _apiService;

        public ReturnsService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task DistributeDailyReturns(string salesBatch)
        {
            await _apiService.PostAsync($"/api/Returns/DistributeDailyReturns?SalesBatch={salesBatch}");
        }

        public async Task ExcelImportReturns()
        {
            await _apiService.PostAsync($"/api/Returns/ExcelImportReturns");
        }

    }
}
