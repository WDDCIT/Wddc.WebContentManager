using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Videos
{
    public interface IVideosService
    {
        Task<List<VID001>> GetAllVID001();
        Task<List<VID002>> GetAllVID002();
        Task<VID001> GetVID001ByCTGY_NBR(int CTGY_NBR);
        Task<VID002> GetVID002ByID(int ID);
        Task<List<VID002>> GetVID002ByCTGY_NBR(int CTGY_NBR);
        Task<VID001> CreateVID001(VID001 VID001);
        Task<VID002> CreateVID002(VID002 VID002);
        Task UpdateVID001(VID001 VID001, int CTGY_NBR);
        Task UpdateVID002(VID002 VID002, int ID);
        Task DeleteVID001(int CTGY_NBR);
        Task DeleteVID002(int ID);
        Task ReorderVID001s(IEnumerable<VID001> VID001s);
    }
}
