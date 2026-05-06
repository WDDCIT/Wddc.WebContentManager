using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.GenericMessage
{
    public class GenericMessageService : IGenericMessageService
    {
        private readonly IWddcAppsApiService _apiService;

        public GenericMessageService(IWddcAppsApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<GenericMessageDto>> GetGenericMessages()
        {
            return await _apiService.GetAsync<List<GenericMessageDto>>($"/api/GenericMessage/MessageGeneric");
        }

        public async Task<GenericMessageDto> GetGenericMessageById(int Id)
        {
            return await _apiService.GetAsync<GenericMessageDto>($"/api/GenericMessage/MessageGeneric/{Id}");
        }

        public async Task<GenericMessageDto> AddGenericMessage(GenericMessageDto genericMessage)
        {
            return await _apiService.PostAsync<GenericMessageDto>($"/api/GenericMessage/MessageGeneric", genericMessage);
        }

        public async Task<GenericMessageDto> UpdateGenericMessage(GenericMessageDto genericMessage, int Id)
        {
            return await _apiService.PostAsync<GenericMessageDto>($"/api/GenericMessage/MessageGeneric/{Id}", genericMessage);
        }

    }
}
