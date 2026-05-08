using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public class InsiderNewsletterService : IInsiderNewsletterService
    {
        private readonly IWddcAppsApiService _apiService;

        public InsiderNewsletterService(IWddcAppsApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<WebNewsDto>> GetWebInsiderNews()
        {
            return await _apiService.GetAsync<List<WebNewsDto>>("/api/InsiderNewsletter/WebInsiderNews");
        }

        public async Task<WebNewsDto> GetWebInsiderNewsById(int id)
        {
            return await _apiService.GetAsync<WebNewsDto>($"/api/InsiderNewsletter/WebInsiderNews/{id}");
        }

        public async Task<WebNewsDto> GetLastWebInsiderNews()
        {
            return await _apiService.GetAsync<WebNewsDto>("/api/InsiderNewsletter/WebInsiderNewsLast");
        }

        public async Task<WebNewsDto> CreateWebInsiderNews(WebNewsDto webNews)
        {
            return await _apiService.PostAsync<WebNewsDto>("/api/InsiderNewsletter/WebInsiderNews", webNews);
        }

        public async Task<WebNewsDto> UpdateWebInsiderNews(int id, WebNewsDto webNews)
        {
            return await _apiService.PostAsync<WebNewsDto>($"/api/InsiderNewsletter/WebInsiderNews/{id}", webNews);
        }

        public async Task DeleteWebInsiderNews(int id)
        {
            await _apiService.PostAsync<object>($"/api/InsiderNewsletter/WebInsiderNews/Delete/{id}");
        }
    }
}
