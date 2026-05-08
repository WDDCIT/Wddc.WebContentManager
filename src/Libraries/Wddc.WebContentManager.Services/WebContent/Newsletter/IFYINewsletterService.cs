using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public interface IFYINewsletterService
    {
        Task<List<WebNewsDto>> GetWebFYINews();
        Task<WebNewsDto> GetWebFYINewsById(int id);
        Task<WebNewsDto> GetLastWebFYINews();
        Task<WebNewsDto> CreateWebFYINews(WebNewsDto webNews);
        Task<WebNewsDto> UpdateWebFYINews(int id, WebNewsDto webNews);
        Task DeleteWebFYINews(int id);
    }
}
