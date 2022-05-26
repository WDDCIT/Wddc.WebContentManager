
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Affinity
{
    public interface IAffinityService
    {
        Task<List<Web_AffinityPrograms>> GetAllWebAffinityPrograms();
        Task<Web_AffinityPrograms> GetWebAffinityProgramById(int ProgramID);
        Task<Web_AffinityPrograms> CreateWebAffinityProgram(Web_AffinityPrograms Web_AffinityProgram);
        Task UpdateWebAffinityProgram(Web_AffinityPrograms Web_AffinityProgram, int ProgramID);
        Task DeleteWebAffinityProgram(int ProgramID);
    }
}
