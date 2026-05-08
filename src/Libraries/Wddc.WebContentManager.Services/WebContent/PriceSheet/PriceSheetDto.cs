using System;

namespace Wddc.WebContentManager.Services.WebContent.PriceSheet
{
    public class PriceSheetDto
    {
        public short ID { get; set; }
        public int? Category { get; set; }
        public string IssueNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public string Description { get; set; }
        public byte? Status { get; set; }
    }
}
