
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Media;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.GenericMessage
{
    public interface IGenericMessageService
    {
        Task<List<Message_Generic>> GetGenericMessages();
        Task<Message_Generic> AddGenericMessage(Message_Generic genericMessage);
        Task<Message_Generic> GetGenericMessageById(int Id);
        Task UpdateGenericMessage(Message_Generic genericMessage, int Id);
    }
}
