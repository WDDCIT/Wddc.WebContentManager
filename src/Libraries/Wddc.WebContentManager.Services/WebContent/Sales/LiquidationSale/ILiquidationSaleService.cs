
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Sales.LiquidationSale
{
    public interface ILiquidationSaleService
    {
        Task<List<Web_Liquidation_RET>> GetAllWebLiquidationAsync();
        Task<Web_Liquidation_RET> GetWebLiquidationByIdAsync(int ID);
        Task<Web_Liquidation_RET> CreateWebLiquidation(Web_Liquidation_RET Web_Liquidation_RET);
        Task DeleteWebLiquidation(int ID);
        Task UpdateWebLiquidation(Web_Liquidation_RET Web_Liquidation_RET, int ID);
        Task<GetItemInfo_Result> GetItemInfo(string ItemNumber);
    }
}
