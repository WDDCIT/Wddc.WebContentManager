
using System.Collections.Generic;
using System.Threading.Tasks;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Services.WebContent.Careers
{
    public interface ICareersService
    {
        Task<List<Web_Careers>> GetAllWebCareers();
        Task<Web_Careers> GetWebCareerByIdAsync(int Ad_ID);
        Task<Web_Careers> CreateWebCareer(Web_Careers Web_Career);
        Task DeleteWebCareer(int Ad_ID);
        Task UpdateWebCareer(Web_Careers Web_Career, int Ad_ID);
        Task<string> GetMemberName(string MemberNbr);
    }
}
