
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebUser
{
    public class WebUserLookupService : IWebUserLookupService
    {
        private readonly IWddcAppsApiService _apiService;

        public WebUserLookupService(IWddcAppsApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<WebAccessDto>> GetWebAccess(string memberNbr)
        {
            return await _apiService.GetAsync<List<WebAccessDto>>($"/api/WebAccess/GetWebAccess/{memberNbr}");
        }

        public async Task<List<WebAccessMemberDto>> GetWebAccessMembers()
        {
            return await _apiService.GetAsync<List<WebAccessMemberDto>>($"/api/WebAccess/GetWebAccessMembers");
        }

        public async Task<ContactInfoDto> GetContactInfo(string memberNbr)
        {
            var results = await _apiService.GetAsync<List<ContactInfoDto>>($"/api/WebAccess/GetContactInfo/{memberNbr}");
            return results?.FirstOrDefault();
        }

        public async Task UpdateWebAccess(string memberNumber, string username, bool emailConfirmed)
        {
            await _apiService.PostAsync<object>($"/api/WebAccess", new { MemberNumber = memberNumber, Username = username, EmailConfirmed = emailConfirmed });
        }
    }
}
