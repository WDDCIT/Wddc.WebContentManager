using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Videos
{
    public class VideosService : IVideosService
    {
        private readonly IWddcApiService _apiService;

        public VideosService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<VID001>> GetAllVID001()
        {
            return await _apiService.GetAsync<List<VID001>>($"/api/Videos/VID001");
        }

        public async Task<List<VID002>> GetAllVID002()
        {
            return await _apiService.GetAsync<List<VID002>>($"/api/Videos/VID002");
        }

        public async Task<VID001> GetVID001ByCTGY_NBR(int CTGY_NBR)
        {
            return await _apiService.GetAsync<VID001>($"/api/Videos/VID001/{CTGY_NBR}");
        }

        public async Task<VID002> GetVID002ByID(int ID)
        {
            return await _apiService.GetAsync<VID002>($"/api/Videos/VID002/{ID}");
        }

        public async Task<List<VID002>> GetVID002ByCTGY_NBR(int CTGY_NBR)
        {
            return await _apiService.GetAsync<List<VID002>>($"/api/Videos/VID002s/{CTGY_NBR}");
        }

        public async Task<VID001> CreateVID001(VID001 VID001)
        {
            return await _apiService.PostAsync<VID001>($"/api/Videos/VID001", VID001);
        }

        public async Task<VID002> CreateVID002(VID002 VID002)
        {
            return await _apiService.PostAsync<VID002>($"/api/Videos/VID002", VID002);
        }

        public async Task UpdateVID001(VID001 VID001, int CTGY_NBR)
        {
            await _apiService.PostAsync($"/api/Videos/VID001/{CTGY_NBR}", VID001);
        }

        public async Task UpdateVID002(VID002 VID002, int ID)
        {
            await _apiService.PostAsync($"/api/Videos/VID002/{ID}", VID002);
        }

        public async Task DeleteVID001(int CTGY_NBR)
        {
            await _apiService.DeleteAsync($"/api/Videos/VID001/{CTGY_NBR}");
        }

        public async Task DeleteVID002(int ID)
        {
            await _apiService.DeleteAsync($"/api/Videos/VID002/{ID}");
        }

        public async Task ReorderVID001s(IEnumerable<VID001> VID001s)
        {
            await _apiService.PostAsync($"/api/Videos/VID001s/Reorder", VID001s);
        }
    }
}
