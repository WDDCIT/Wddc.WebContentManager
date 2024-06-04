
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.AppData.Items;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public interface IItemInfoService
    {
        Task<List<Items>> GetItems();
        
    }
}
