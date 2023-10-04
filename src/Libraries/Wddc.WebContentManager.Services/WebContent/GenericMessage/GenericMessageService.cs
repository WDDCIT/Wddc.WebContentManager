using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.GenericMessage
{
    public class GenericMessageService : IGenericMessageService
    {
        private readonly IWddcApiService _apiService;

        public GenericMessageService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task<List<Message_Generic>> GetGenericMessages()
        {
            return await _apiService.GetAsync<List<Message_Generic>>($"/api/GenericMessage/MessageGeneric");
        }

        public async Task<Message_Generic> GetGenericMessageById(int Id)
        {
            return await _apiService.GetAsync<Message_Generic>($"/api/GenericMessage/MessageGeneric/{Id}");
        }

        public async Task<Message_Generic> AddGenericMessage(Message_Generic genericMessage)
        {
            return await _apiService.PostAsync<Message_Generic>($"/api/GenericMessage/MessageGeneric", genericMessage);
        }

        public async Task UpdateGenericMessage(Message_Generic genericMessage, int Id)
        {
            await _apiService.PostAsync($"/api/GenericMessage/MessageGeneric/{Id}", genericMessage);
        }

    }
}
