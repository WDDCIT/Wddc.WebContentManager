
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.WebOrder;

namespace Wddc.WebContentManager.Services.WebContent.Vendors
{
    public class VendorListService : IVendorListService
    {
        private readonly IWddcAppsApiService _apiService;

        public VendorListService(IWddcAppsApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Web_VendorList>> GetWebVendorList()
        {
            return await _apiService.GetAsync<List<Web_VendorList>>($"/api/WebVendorList/Web_VendorList");
        }

        public async Task<Web_VendorList> GetWebVendorById(string VendorID)
        {
            return await _apiService.GetAsync<Web_VendorList>($"/api/WebVendorList/Web_VendorList/{VendorID}");
        }

        public async Task<Web_VendorList> CreateWebVendor(Web_VendorList webVendor)
        {
            return await _apiService.PostAsync<Web_VendorList>($"/api/WebVendorList/Web_VendorList", webVendor);
        }

        public async Task DeleteWebVendor(string VendorID)
        {
            await _apiService.PostAsync<object>($"/api/WebVendorList/Web_VendorList/Delete/{VendorID}");
        }

        public async Task<Web_VendorList> UpdateWebVendor(Web_VendorList webVendor, string VendorID)
        {
            return await _apiService.PostAsync<Web_VendorList>($"/api/WebVendorList/Web_VendorList/{VendorID}", webVendor);
        }


    }
}
