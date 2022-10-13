
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public class NewsletterService : INewsletterService
    {
        private readonly IWddcApiService _apiService;

        public NewsletterService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Web_News>> GetWebFYINews()
        {
            return await _apiService.GetAsync<List<Web_News>>($"/api/FYINewsletter/WebFYINews");
        }

        public async Task<Web_News> GetWebFYINewsById(int ID)
        {
            return await _apiService.GetAsync<Web_News>($"/api/FYINewsletter/WebFYINews/{ID}");
        }

        public async Task<Web_News> GetLastWebFYINews()
        {
            return await _apiService.GetAsync<Web_News>($"/api/FYINewsletter/WebFYINewsLast");
        }

        public async Task<Web_News> CreateWebFYINews(Web_News Web_News)
        {
            return await _apiService.PostAsync<Web_News>($"/api/FYINewsletter/WebFYINews", Web_News);
        }

        public async Task UpdateWebFYINews(Web_News Web_News, int ID)
        {
            await _apiService.PostAsync($"/api/FYINewsletter/WebFYINews/{ID}", Web_News);
        }

        public async Task DeleteWebFYINews(int ID)
        {
            await _apiService.PostAsync($"/api/FYINewsletter/WebFYINews/Delete/{ID}");
        }

        /*-----------------------------------------------------------------------------------*/

        public async Task<List<Web_News>> GetWebInsiderNews()
        {
            return await _apiService.GetAsync<List<Web_News>>($"/api/InsiderNewsletter/WebInsiderNews");
        }

        public async Task<Web_News> GetWebInsiderNewsById(int ID)
        {
            return await _apiService.GetAsync<Web_News>($"/api/InsiderNewsletter/WebInsiderNews/{ID}");
        }

        public async Task<Web_News> GetLastWebInsiderNews()
        {
            return await _apiService.GetAsync<Web_News>($"/api/InsiderNewsletter/WebInsiderNewsLast");
        }

        public async Task<Web_News> CreateWebInsiderNews(Web_News Web_News)
        {
            return await _apiService.PostAsync<Web_News>($"/api/InsiderNewsletter/WebInsiderNews", Web_News);
        }

        public async Task UpdateWebInsiderNews(Web_News Web_News, int ID)
        {
            await _apiService.PostAsync($"/api/InsiderNewsletter/WebInsiderNews/{ID}", Web_News);
        }

        public async Task DeleteWebInsiderNews(int ID)
        {
            await _apiService.PostAsync($"/api/InsiderNewsletter/WebInsiderNews/Delete/{ID}");
        }


    }
}
