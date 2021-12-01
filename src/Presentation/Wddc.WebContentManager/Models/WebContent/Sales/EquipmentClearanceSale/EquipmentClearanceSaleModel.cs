using System.Collections.Generic;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Models.WebContent.Sales.EquipmentClearanceSale
{
    public class EquipmentClearanceSaleModel
    {
        public IEnumerable<Web_Clearance> WebEquipmentClearance { get; set; }
    }
}