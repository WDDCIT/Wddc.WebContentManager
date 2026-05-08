using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public class FYINewsletterService : IFYINewsletterService
    {
        private readonly IWddcAppsApiService _apiService;

        public FYINewsletterService(IWddcAppsApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<WebNewsDto>> GetWebFYINews()
        {
            return await _apiService.GetAsync<List<WebNewsDto>>("/api/FYINewsletter/WebFYINews");
        }

        public async Task<WebNewsDto> GetWebFYINewsById(int id)
        {
            return await _apiService.GetAsync<WebNewsDto>($"/api/FYINewsletter/WebFYINews/{id}");
        }

        public async Task<WebNewsDto> GetLastWebFYINews()
        {
            return await _apiService.GetAsync<WebNewsDto>("/api/FYINewsletter/WebFYINewsLast");
        }

        public async Task<WebNewsDto> CreateWebFYINews(WebNewsDto webNews)
        {
            return await _apiService.PostAsync<WebNewsDto>("/api/FYINewsletter/WebFYINews", webNews);
        }

        public async Task<WebNewsDto> UpdateWebFYINews(int id, WebNewsDto webNews)
        {
            return await _apiService.PostAsync<WebNewsDto>($"/api/FYINewsletter/WebFYINews/{id}", webNews);
        }

        public async Task DeleteWebFYINews(int id)
        {
            await _apiService.PostAsync<object>($"/api/FYINewsletter/WebFYINews/Delete/{id}");
        }
    }
}
