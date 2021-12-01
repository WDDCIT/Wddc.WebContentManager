using System.Collections.Generic;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Models.WebContent.Sales.RetailClearanceSale
{
    public class RetailClearanceSaleModel
    {
        public IEnumerable<Web_Clearance> WebRetailClearance { get; set; }
    }
}