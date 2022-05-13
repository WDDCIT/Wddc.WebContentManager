
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.Customers;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebUser
{
    public interface IWebUserLookupService
    {
        Task<List<GetWebAccess_Result>> GetWebAccess(string memberNbr);
        Task<List<GetWebAccessMembers_Result>> GetWebAccessMembers();
        Task<List<GetContactInfo_Result>> GetContactInfo(string memberNbr);
    }
}
