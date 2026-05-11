
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.WebOrder;

namespace Wddc.WebContentManager.Services.WebContent.Sales.LiquidationSale
{
    public class LiquidationSaleService : ILiquidationSaleService
    {
        private readonly IWddcApiService _apiService;

        public LiquidationSaleService(IWddcApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Web_Liquidation_RET>> GetAllWebLiquidationAsync()
        {
            return await _apiService.GetAsync<List<Web_Liquidation_RET>>("/api/LiquidationSale/Web_Liquidation_RET");
        }

        public async Task<Web_Liquidation_RET> GetWebLiquidationByIdAsync(int ID)
        {
            return await _apiService.GetAsync<Web_Liquidation_RET>($"/api/LiquidationSale/Web_Liquidation_RET/{ID}");
        }

        public async Task<Web_Liquidation_RET> CreateWebLiquidation(Web_Liquidation_RET Web_Liquidation_RET)
        {
            return await _apiService.PostAsync<Web_Liquidation_RET>("/api/LiquidationSale/Web_Liquidation_RET", Web_Liquidation_RET);
        }

        public async Task DeleteWebLiquidation(int ID)
        {
            await _apiService.PostAsync($"/api/LiquidationSale/Web_Liquidation_RET/Delete/{ID}");
        }

        public async Task<Web_Liquidation_RET> UpdateWebLiquidation(int ID, Web_Liquidation_RET Web_Liquidation_RET)
        {
            return await _apiService.PostAsync<Web_Liquidation_RET>($"/api/LiquidationSale/Web_Liquidation_RET/{ID}", Web_Liquidation_RET);
        }

        public async Task<GetItemInfo_Result> GetItemInfo(string ItemNumber)
        {
            return await _apiService.GetAsync<GetItemInfo_Result>($"/api/LiquidationSale/GetItemInfo/{ItemNumber}");
        }
    }
}
