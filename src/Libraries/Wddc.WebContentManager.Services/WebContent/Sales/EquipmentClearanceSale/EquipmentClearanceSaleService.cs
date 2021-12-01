
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Sales.EquipmentClearanceSale
{
    public class EquipmentClearanceSaleService : IEquipmentClearanceSaleService
    {
        private readonly IWddcApiService _apiService;

        public EquipmentClearanceSaleService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Web_Clearance>> GetAllWebClearanceEquipment()
        {
            return await _apiService.GetAsync<List<Web_Clearance>>($"/api/EquipmentClearanceSale/Web_Clearance/Equipment");
        }

        public async Task<Web_Clearance> GetWebClearanceEquipmentByItemNumber(string ItemNumber)
        {
            return await _apiService.GetAsync<Web_Clearance>($"/api/EquipmentClearanceSale/Web_Clearance/Equipment/{ItemNumber}");
        }

        public async Task<Web_Clearance> CreateWebClearanceEquipment(Web_Clearance WebClearanceEquipment)
        {
            return await _apiService.PostAsync<Web_Clearance>($"/api/EquipmentClearanceSale/Web_Clearance/Equipment", WebClearanceEquipment);
        }

        public async Task UpdateWebClearanceEquipment(Web_Clearance WebClearanceEquipment, string ItemNumber)
        {
            await _apiService.PostAsync($"/api/EquipmentClearanceSale/Web_Clearance/Equipment/{ItemNumber}", WebClearanceEquipment);
        }

        public async Task DeleteWebClearanceEquipment(string ItemNumber)
        {
            await _apiService.DeleteAsync($"/api/EquipmentClearanceSale/Web_Clearance/Equipment/{ItemNumber}");
        }

        public async Task<GetItemInfo_Result> GetItemInfo(string ItemNumber)
        {
            return await _apiService.GetAsync<GetItemInfo_Result>($"/api/LiquidationSale/GetItemInfo/{ItemNumber}");
        }

    }
}
