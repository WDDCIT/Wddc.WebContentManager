
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Services.WebUser
{
    public interface IWebUserLookupService
    {
        Task<List<WebAccessDto>> GetWebAccess(string memberNbr);
        Task<List<WebAccessMemberDto>> GetWebAccessMembers();
        Task<ContactInfoDto> GetContactInfo(string memberNbr);
        Task UpdateWebAccess(string memberNumber, string username, bool emailConfirmed);
    }
}
