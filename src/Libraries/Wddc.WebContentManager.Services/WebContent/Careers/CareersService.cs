
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Careers
{
    public class CareersService : ICareersService
    {
        private readonly IWddcAppsApiService _apiService;

        public CareersService(IWddcAppsApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Web_Careers>> GetAllWebCareers()
        {
            return await _apiService.GetAsync<List<Web_Careers>>("/api/Careers/Web_Careers");
        }

        public async Task<Web_Careers> GetWebCareerByIdAsync(int Ad_ID)
        {
            return await _apiService.GetAsync<Web_Careers>($"/api/Careers/Web_Careers/{Ad_ID}");
        }

        public async Task<Web_Careers> CreateWebCareer(Web_Careers Web_Career)
        {
            return await _apiService.PostAsync<Web_Careers>("/api/Careers/Web_Careers", Web_Career);
        }

        public async Task DeleteWebCareer(int Ad_ID)
        {
            await _apiService.SendAsync(HttpMethod.Post, $"/api/Careers/Web_Careers/Delete/{Ad_ID}");
        }

        public async Task UpdateWebCareer(Web_Careers Web_Career, int Ad_ID)
        {
            await _apiService.SendAsync(HttpMethod.Post, $"/api/Careers/Web_Careers/{Ad_ID}", Web_Career);
        }

        public async Task<string> GetMemberName(string MemberNbr)
        {
            return await _apiService.GetStringAsync($"/api/Careers/GetMemberName/{MemberNbr}");
        }
    }
}
