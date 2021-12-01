
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Sales.EquipmentClearanceSale
{
    public interface IEquipmentClearanceSaleService
    {
        Task<List<Web_Clearance>> GetAllWebClearanceEquipment();
        Task<Web_Clearance> GetWebClearanceEquipmentByItemNumber(string ItemNumber);
        Task<Web_Clearance> CreateWebClearanceEquipment(Web_Clearance WebClearanceEquipment);
        Task UpdateWebClearanceEquipment(Web_Clearance WebClearanceEquipment, string ItemNumber);
        Task DeleteWebClearanceEquipment(string ItemNumber);
        Task<GetItemInfo_Result> GetItemInfo(string ItemNumber);
    }
}
