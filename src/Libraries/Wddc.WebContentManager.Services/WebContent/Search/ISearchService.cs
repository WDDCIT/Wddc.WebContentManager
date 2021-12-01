using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.TICatalog;

namespace Wddc.WebContentManager.Services.WebContent.Search
{
    public interface ISearchService
    {
        Task<List<tblSearch>> GetAllTblSearch();
        Task<tblSearch> GetTblSearchBySearchDescr(string SearchDescr);
        Task<tblSearch> CreateTblSearch(tblSearch tblSearch);
        Task UpdateTblSearch(tblSearch tblSearch, string SearchDescr);
        Task DeleteTblSearch(string SearchDescr);
    }
}
