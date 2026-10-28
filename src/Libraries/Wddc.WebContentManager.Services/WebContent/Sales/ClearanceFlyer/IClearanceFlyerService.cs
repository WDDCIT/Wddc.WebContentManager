using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.Sales.ClearanceFlyer
{
    /// <summary>The clearance flyers, through the Apps API (api/Clearance/Flyer*).</summary>
    public interface IClearanceFlyerService
    {
        /// <summary>Every flyer, with how many items it would have now (no cover bytes).</summary>
        Task<List<ClearanceFlyerDto>> GetFlyersAsync();

        /// <summary>A flyer's cover picture; null when it has none.</summary>
        Task<ClearanceFlyerCoverDto> GetCoverAsync(int id);

        /// <summary>Saves a flyer's look. A cover replaces the old one; removeCover takes it away; neither keeps it.</summary>
        Task SaveAsync(int id, string title, string subtitle, string intro, string note, string footer, string accentColor,
            byte[] cover, string coverType, bool removeCover, string user);

        /// <summary>Notes that a flyer was put on the website, with how many items.</summary>
        Task PublishedAsync(int id, int items, string user);

        /// <summary>The items of one website section, grouped by product class.</summary>
        Task<List<ClearanceFlyerItemDto>> GetItemsAsync(int categoryId);
    }
}
