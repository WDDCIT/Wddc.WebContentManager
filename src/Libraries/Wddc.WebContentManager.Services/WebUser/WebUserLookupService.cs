
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.Customers;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebUser
{
    public class WebUserLookupService : IWebUserLookupService
    {
        private readonly IWddcApiService _apiService;

        public WebUserLookupService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<GetWebAccess_Result>> GetWebAccess(string memberNbr)
        {
            return await _apiService.GetAsync<List<GetWebAccess_Result>>($"/api/WebAccess/GetWebAccess/{memberNbr}");
        }

        public async Task<List<GetWebAccessMembers_Result>> GetWebAccessMembers()
        {
            return await _apiService.GetAsync<List<GetWebAccessMembers_Result>>($"/api/WebAccess/GetWebAccessMembers");
        }

        public async Task<List<GetContactInfo_Result>> GetContactInfo(string memberNbr)
        {
            return await _apiService.GetAsync<List<GetContactInfo_Result>>($"/api/WebAccess/GetContactInfo/{memberNbr}");
        }

    }
}
