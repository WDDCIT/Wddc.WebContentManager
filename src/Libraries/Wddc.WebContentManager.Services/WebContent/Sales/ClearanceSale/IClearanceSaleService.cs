using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.WebOrder;

namespace Wddc.WebContentManager.Services.WebContent.Sales.ClearanceSale
{
    public interface IClearanceSaleService
    {
        Task<List<WebClearanceDto>> GetAllWebClearanceAsync();
        Task<WebClearanceDto> GetWebClearanceByItemNumberAsync(string itemNumber);
        Task<WebClearanceDto> CreateWebClearance(WebClearanceDto item);
        Task<WebClearanceDto> UpdateWebClearance(string originalItemNumber, WebClearanceDto item);
        Task DeleteWebClearance(string itemNumber);
        Task<GetItemInfo_Result> GetItemInfo(string itemNumber);
        Task<List<ClearanceCatalogInfoDto>> ValidateItems(List<string> itemNumbers);
        Task<ClearanceReplaceResultDto> ReplaceAll(ClearanceReplaceRequestDto request);
        Task<List<ClearanceBackupInfoDto>> GetBackups();
        Task<ClearanceRestoreResultDto> Restore(ClearanceRestoreRequestDto request);
    }
}
