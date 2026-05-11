using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.TiCatalogWebsrvr;

namespace Wddc.WebContentManager.Services.WebContent.Search
{
    public interface ISearchService
    {
        Task<List<TblSearch>> GetAllTblSearch();
        Task<TblSearch> GetTblSearchBySearchDescr(string SearchDescr);
        Task<TblSearch> CreateTblSearch(TblSearch tblSearch);
        Task<TblSearch> UpdateTblSearch(string SearchDescr, TblSearch tblSearch);
        Task DeleteTblSearch(string SearchDescr);
    }
}
