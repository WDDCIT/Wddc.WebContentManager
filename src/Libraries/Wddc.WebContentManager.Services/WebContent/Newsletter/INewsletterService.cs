
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public interface INewsletterService
    {
        Task<List<Web_News>> GetWebFYINews();
        Task<Web_News> GetWebFYINewsById(int ID);
        Task<Web_News> GetLastWebFYINews();
        Task<Web_News> CreateWebFYINews(Web_News Web_News);
        Task UpdateWebFYINews(Web_News Web_News, int ID);
        Task DeleteWebFYINews(int ID);
        Task<List<Web_News>> GetWebInsiderNews();
        Task<Web_News> GetWebInsiderNewsById(int ID);
        Task<Web_News> GetLastWebInsiderNews();
        Task<Web_News> CreateWebInsiderNews(Web_News Web_News);
        Task UpdateWebInsiderNews(Web_News Web_News, int ID);
        Task DeleteWebInsiderNews(int ID);
        Task<List<Web_News>> GetPriceSheets();
        Task<Web_News> GetPriceSheetById(int ID);
        Task<Web_News> CreatePriceSheet(Web_News Web_News);
        Task UpdatePriceSheet(Web_News Web_News, int ID);
        Task DeletePriceSheet(int ID);

    }
}
