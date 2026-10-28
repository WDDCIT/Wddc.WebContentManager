using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebContent.Sales.ClearanceFlyer
{
    /// <summary>
    /// The clearance flyers (2026-10-27), kept in AppData's Inventory.ClearanceFlyer and read through the Apps API -
    /// the Apps API's docs/sql/Create_AppData_ClearanceFlyer.sql.
    /// </summary>
    public class ClearanceFlyerService : IClearanceFlyerService
    {
        private readonly IWddcAppsApiService _api;

        public ClearanceFlyerService(IWddcAppsApiService api)
        {
            _api = api;
        }

        public async Task<List<ClearanceFlyerDto>> GetFlyersAsync()
        {
            return await _api.GetAsync<List<ClearanceFlyerDto>>("api/Clearance/Flyers") ?? new List<ClearanceFlyerDto>();
        }

        public async Task<ClearanceFlyerCoverDto> GetCoverAsync(int id)
        {
            try
            {
                return await _api.GetAsync<ClearanceFlyerCoverDto>("api/Clearance/FlyerCover", new { id });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // No cover picture: the flyer's cover is drawn in its colour.
                return null;
            }
        }

        public async Task SaveAsync(int id, string title, string subtitle, string intro, string note, string footer, string accentColor,
            byte[] cover, string coverType, bool removeCover, string user)
        {
            await _api.PostAsync<object>("api/Clearance/FlyerSave", new
            {
                ID = id,
                Title = title,
                Subtitle = subtitle,
                Intro = intro,
                Note = note,
                Footer = footer,
                AccentColor = accentColor,
                CoverImage = cover,
                CoverImageType = coverType,
                RemoveCover = removeCover,
                User = user
            });
        }

        public async Task PublishedAsync(int id, int items, string user)
        {
            await _api.PostAsync<object>("api/Clearance/FlyerPublished", new { ID = id, Items = items, User = user });
        }

        public async Task<List<ClearanceFlyerItemDto>> GetItemsAsync(int categoryId)
        {
            return await _api.GetAsync<List<ClearanceFlyerItemDto>>("api/Clearance/FlyerItems", new { categoryId }) ?? new List<ClearanceFlyerItemDto>();
        }
    }
}
