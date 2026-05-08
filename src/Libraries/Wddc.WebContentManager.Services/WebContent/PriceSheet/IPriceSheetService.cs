using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.PriceSheet
{
    public interface IPriceSheetService
    {
        Task<List<PriceSheetDto>> GetPriceSheets();
        Task<PriceSheetDto> GetPriceSheetById(int id);
        Task<PriceSheetDto> CreatePriceSheet(PriceSheetDto priceSheet);
        Task<PriceSheetDto> UpdatePriceSheet(int id, PriceSheetDto priceSheet);
        Task DeletePriceSheet(int id);
    }
}
