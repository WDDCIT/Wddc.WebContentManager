
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Vendors
{
    public interface IVendorListService
    {
        Task<List<Web_VendorList>> GetWebVendorList();
        Task<Web_VendorList> GetWebVendorById(string VendorID);
        Task<Web_VendorList> CreateWebVendor(Web_VendorList webVendor);
        Task<Web_VendorList> UpdateWebVendor(Web_VendorList webVendor, string VendorID);
        Task DeleteWebVendor(string VendorID);
    }
}
