
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.ClientVantageBanners
{
    public class ClientVantageBannersService : IClientVantageBannersService
    {
        private readonly IWddcApiService _apiService;

        public ClientVantageBannersService(IWddcApiService apiService)
        {
            this._apiService = apiService;
        }

        

    }
}
