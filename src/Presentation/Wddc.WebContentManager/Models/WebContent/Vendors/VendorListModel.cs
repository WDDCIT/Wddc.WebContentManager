using System.Collections.Generic;
using Wddc.Api.Core.Domain.Entities.WebOrder;

namespace Wddc.WebContentManager.Models.WebContent.Vendors
{
    public class VendorListModel
    {
        public IEnumerable<Web_VendorList> WebVendorList { get; set; }
    }
}