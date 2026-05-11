
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.WebOrder;

namespace Wddc.WebContentManager.Services.WebContent.Sales.LiquidationSale
{
    public interface ILiquidationSaleService
    {
        Task<List<Web_Liquidation_RET>> GetAllWebLiquidationAsync();
        Task<Web_Liquidation_RET> GetWebLiquidationByIdAsync(int ID);
        Task<Web_Liquidation_RET> CreateWebLiquidation(Web_Liquidation_RET Web_Liquidation_RET);
        Task DeleteWebLiquidation(int ID);
        Task<Web_Liquidation_RET> UpdateWebLiquidation(int ID, Web_Liquidation_RET Web_Liquidation_RET);
        Task<GetItemInfo_Result> GetItemInfo(string ItemNumber);
    }
}
