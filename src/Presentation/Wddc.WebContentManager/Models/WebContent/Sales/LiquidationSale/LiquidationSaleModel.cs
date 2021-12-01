using System.Collections.Generic;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Models.WebContent.Sales.LiquidationSale
{
    public class LiquidationSaleModel
    {
        public IEnumerable<Web_Liquidation_RET> WebLiquidationRET { get; set; }
    }
}