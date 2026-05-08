
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Api.Core.Models.Items;

namespace Wddc.WebContentManager.Services.WebContent.Newsletter
{
    public interface IItemInfoService
    {
        Task<List<ItemSummary>> GetItems();
        
    }
}
