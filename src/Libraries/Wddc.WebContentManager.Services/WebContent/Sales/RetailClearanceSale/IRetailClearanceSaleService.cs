
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Sales.RetailClearanceSale
{
    public interface IRetailClearanceSaleService
    {
        Task<List<Web_Clearance>> GetAllWebClearanceRetail();
        Task<Web_Clearance> GetWebClearanceRetailByItemNumber(string ItemNumber);
        Task<Web_Clearance> CreateWebClearanceRetail(Web_Clearance WebClearanceRetail);
        Task UpdateWebClearanceRetail(Web_Clearance WebClearanceRetail, string ItemNumber);
        Task DeleteWebClearanceRetail(string ItemNumber);
        Task<GetItemInfo_Result> GetItemInfo(string ItemNumber);
    }
}
