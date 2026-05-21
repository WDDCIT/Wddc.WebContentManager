
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Domain.Entities.WebOrder;


namespace Wddc.WebContentManager.Services.WebContent.Affinity
{
    public class AffinityService : IAffinityService
    {
        private readonly IWddcAppsApiService _apiService;

        public AffinityService(IWddcAppsApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Web_AffinityPrograms>> GetAllWebAffinityPrograms()
        {
            return await _apiService.GetAsync<List<Web_AffinityPrograms>>("/api/AffinityPrograms/Web_AffinityPrograms");
        }

        public async Task<Web_AffinityPrograms> GetWebAffinityProgramById(int ProgramID)
        {
            return await _apiService.GetAsync<Web_AffinityPrograms>($"/api/AffinityPrograms/Web_AffinityPrograms/{ProgramID}");
        }

        public async Task<Web_AffinityPrograms> CreateWebAffinityProgram(Web_AffinityPrograms Web_AffinityProgram)
        {
            return await _apiService.PostAsync<Web_AffinityPrograms>("/api/AffinityPrograms/Web_AffinityPrograms", Web_AffinityProgram);
        }

        public async Task<Web_AffinityPrograms> UpdateWebAffinityProgram(int ProgramID, Web_AffinityPrograms Web_AffinityProgram)
        {
            return await _apiService.PostAsync<Web_AffinityPrograms>($"/api/AffinityPrograms/Web_AffinityPrograms/{ProgramID}", Web_AffinityProgram);
        }

        public async Task DeleteWebAffinityProgram(int ProgramID)
        {
            await _apiService.PostAsync<object>($"/api/AffinityPrograms/Web_AffinityPrograms/Delete/{ProgramID}");
        }
    }
}
