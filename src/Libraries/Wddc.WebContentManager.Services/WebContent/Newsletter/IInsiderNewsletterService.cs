using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public interface IInsiderNewsletterService
    {
        Task<List<WebNewsDto>> GetWebInsiderNews();
        Task<WebNewsDto> GetWebInsiderNewsById(int id);
        Task<WebNewsDto> GetLastWebInsiderNews();
        Task<WebNewsDto> CreateWebInsiderNews(WebNewsDto webNews);
        Task<WebNewsDto> UpdateWebInsiderNews(int id, WebNewsDto webNews);
        Task DeleteWebInsiderNews(int id);
    }
}
