using System.Collections.Generic;
using Wddc.Api.Core.Domain.Entities.WebOrder;

namespace Wddc.WebContentManager.Models.WebContent.Careers
{
    public class WebCareersModel
    {
        public IEnumerable<Web_Careers> WebCareers { get; set; }
        public IEnumerable<Web_Careers> WebCareersActive { get; set; }
        public IEnumerable<Web_Careers> WebCareersExpired { get; set; }
    }
}