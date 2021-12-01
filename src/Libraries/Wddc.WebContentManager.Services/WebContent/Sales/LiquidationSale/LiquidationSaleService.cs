
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Sales.LiquidationSale
{
    public class LiquidationSaleService : ILiquidationSaleService
    {
        private readonly IWddcApiService _apiService;

        public LiquidationSaleService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Web_Liquidation_RET>> GetAllWebLiquidationAsync()
        {
            return await _apiService.GetAsync<List<Web_Liquidation_RET>>($"/api/LiquidationSale/Web_Liquidation_RET");
        }

        public async Task<Web_Liquidation_RET> GetWebLiquidationByIdAsync(int ID)
        {
            return await _apiService.GetAsync<Web_Liquidation_RET>($"/api/LiquidationSale/Web_Liquidation_RET/{ID}");
        }

        public async Task<Web_Liquidation_RET> CreateWebLiquidation(Web_Liquidation_RET Web_Liquidation_RET)
        {
            Web_Liquidation_RET newWeb_Liquidation_RET = await _apiService.PostAsync<Web_Liquidation_RET>($"/api/LiquidationSale/Web_Liquidation_RET", Web_Liquidation_RET);
            return newWeb_Liquidation_RET;
        }

        public async Task DeleteWebLiquidation(int ID)
        {
            await _apiService.PostAsync($"/api/LiquidationSale/Web_Liquidation_RET/Delete/{ID}");
        }

        public async Task UpdateWebLiquidation(Web_Liquidation_RET Web_Liquidation_RET, int ID)
        {
            await _apiService.PostAsync($"/api/LiquidationSale/Web_Liquidation_RET/{ID}", Web_Liquidation_RET);
        }

        public async Task<GetItemInfo_Result> GetItemInfo(string ItemNumber)
        {
            return await _apiService.GetAsync<GetItemInfo_Result>($"/api/LiquidationSale/GetItemInfo/{ItemNumber}");
        }

    }
}
