
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Affinity
{
    public class AffinityService : IAffinityService
    {
        private readonly IWddcApiService _apiService;

        public AffinityService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Web_AffinityPrograms>> GetAllWebAffinityPrograms()
        {
            return await _apiService.GetAsync<List<Web_AffinityPrograms>>($"/api/AffinityPrograms/Web_AffinityPrograms");
        }

        public async Task<Web_AffinityPrograms> GetWebAffinityProgramById(int ProgramID)
        {
            return await _apiService.GetAsync<Web_AffinityPrograms>($"/api/AffinityPrograms/Web_AffinityPrograms/{ProgramID}");
        }

        public async Task<Web_AffinityPrograms> CreateWebAffinityProgram(Web_AffinityPrograms Web_AffinityProgram)
        {
            return await _apiService.PostAsync<Web_AffinityPrograms>($"/api/AffinityPrograms/Web_AffinityPrograms", Web_AffinityProgram);
        }

        public async Task UpdateWebAffinityProgram(Web_AffinityPrograms Web_AffinityProgram, int ProgramID)
        {
            await _apiService.PostAsync($"/api/AffinityPrograms/Web_AffinityPrograms/{ProgramID}", Web_AffinityProgram);
        }

        public async Task DeleteWebAffinityProgram(int ProgramID)
        {
            await _apiService.PostAsync($"/api/AffinityPrograms/Web_AffinityPrograms/Delete/{ProgramID}");
        }

    }
}
