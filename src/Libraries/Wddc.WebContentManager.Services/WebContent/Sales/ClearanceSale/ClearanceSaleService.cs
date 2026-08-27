using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.WebOrder;

namespace Wddc.WebContentManager.Services.WebContent.Sales.ClearanceSale
{
    public class ClearanceSaleService : IClearanceSaleService
    {
        private readonly IWddcAppsApiService _apiService;

        public ClearanceSaleService(IWddcAppsApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<WebClearanceDto>> GetAllWebClearanceAsync()
        {
            return await _apiService.GetAsync<List<WebClearanceDto>>("/api/ClearanceSale/Web_Clearance");
        }

        public async Task<WebClearanceDto> GetWebClearanceByItemNumberAsync(string itemNumber)
        {
            return await _apiService.GetAsync<WebClearanceDto>($"/api/ClearanceSale/Web_Clearance/{Uri.EscapeDataString(itemNumber)}");
        }

        public async Task<WebClearanceDto> CreateWebClearance(WebClearanceDto item)
        {
            return await _apiService.PostAsync<WebClearanceDto>("/api/ClearanceSale/Web_Clearance", item);
        }

        public async Task<WebClearanceDto> UpdateWebClearance(string originalItemNumber, WebClearanceDto item)
        {
            return await _apiService.PostAsync<WebClearanceDto>($"/api/ClearanceSale/Web_Clearance/Update/{Uri.EscapeDataString(originalItemNumber)}", item);
        }

        public async Task DeleteWebClearance(string itemNumber)
        {
            await _apiService.PostAsync<object>($"/api/ClearanceSale/Web_Clearance/Delete/{Uri.EscapeDataString(itemNumber)}");
        }

        public async Task<GetItemInfo_Result> GetItemInfo(string itemNumber)
        {
            return await _apiService.GetAsync<GetItemInfo_Result>($"/api/ClearanceSale/GetItemInfo/{Uri.EscapeDataString(itemNumber)}");
        }

        public async Task<List<ClearanceCatalogInfoDto>> ValidateItems(List<string> itemNumbers)
        {
            return await _apiService.PostAsync<List<ClearanceCatalogInfoDto>>("/api/ClearanceSale/ValidateItems", itemNumbers);
        }

        public async Task<ClearanceReplaceResultDto> ReplaceAll(ClearanceReplaceRequestDto request)
        {
            return await _apiService.PostAsync<ClearanceReplaceResultDto>("/api/ClearanceSale/ReplaceAll", request);
        }

        public async Task<List<ClearanceBackupInfoDto>> GetBackups()
        {
            return await _apiService.GetAsync<List<ClearanceBackupInfoDto>>("/api/ClearanceSale/Backups");
        }

        public async Task<ClearanceRestoreResultDto> Restore(ClearanceRestoreRequestDto request)
        {
            return await _apiService.PostAsync<ClearanceRestoreResultDto>("/api/ClearanceSale/Restore", request);
        }
    }
}
