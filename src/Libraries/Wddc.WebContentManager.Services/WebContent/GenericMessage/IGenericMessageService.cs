using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.GenericMessage
{
    public interface IGenericMessageService
    {
        Task<List<GenericMessageDto>> GetGenericMessages();
        Task<GenericMessageDto> AddGenericMessage(GenericMessageDto genericMessage);
        Task<GenericMessageDto> GetGenericMessageById(int Id);
        Task<GenericMessageDto> UpdateGenericMessage(GenericMessageDto genericMessage, int Id);
    }
}
