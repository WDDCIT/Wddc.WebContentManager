
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Sales.RetailClearanceSale
{
    public class RetailClearanceSaleService : IRetailClearanceSaleService
    {
        private readonly IWddcApiService _apiService;

        public RetailClearanceSaleService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Web_Clearance>> GetAllWebClearanceRetail()
        {
            return await _apiService.GetAsync<List<Web_Clearance>>($"/api/RetailClearanceSale/Web_Clearance/Retail");
        }

        public async Task<Web_Clearance> GetWebClearanceRetailByItemNumber(string ItemNumber)
        {
            return await _apiService.GetAsync<Web_Clearance>($"/api/RetailClearanceSale/Web_Clearance/Retail/{ItemNumber}");
        }

        public async Task<Web_Clearance> CreateWebClearanceRetail(Web_Clearance WebClearanceRetail)
        {
            return await _apiService.PostAsync<Web_Clearance>($"/api/RetailClearanceSale/Web_Clearance/Retail", WebClearanceRetail);
        }

        public async Task UpdateWebClearanceRetail(Web_Clearance WebClearanceRetail, string ItemNumber)
        {
            await _apiService.PostAsync($"/api/RetailClearanceSale/Web_Clearance/Retail/{ItemNumber}", WebClearanceRetail);
        }

        public async Task DeleteWebClearanceRetail(string ItemNumber)
        {
            await _apiService.DeleteAsync($"/api/RetailClearanceSale/Web_Clearance/Retail/{ItemNumber}");
        }

        public async Task<GetItemInfo_Result> GetItemInfo(string ItemNumber)
        {
            return await _apiService.GetAsync<GetItemInfo_Result>($"/api/LiquidationSale/GetItemInfo/{ItemNumber}");
        }

    }
}
