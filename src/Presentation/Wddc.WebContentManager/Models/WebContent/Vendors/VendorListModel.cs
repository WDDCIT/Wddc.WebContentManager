using System.Collections.Generic;
using Wddc.Core.Domain.Webserver.WebOrdering;

namespace Wddc.WebContentManager.Models.WebContent.Vendors
{
    public class VendorListModel
    {
        public IEnumerable<Web_VendorList> WebVendorList { get; set; }
    }
}