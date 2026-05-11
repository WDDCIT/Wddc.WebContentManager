using System.Collections.Generic;
using Wddc.Api.Core.Domain.Entities.WebOrder;

namespace Wddc.WebContentManager.Models.WebContent.Sales.LiquidationSale
{
    public class LiquidationSaleModel
    {
        public IEnumerable<Web_Liquidation_RET> WebLiquidationRET { get; set; }
    }
}