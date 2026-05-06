using System;

namespace Wddc.WebContentManager.Services.WebContent.GenericMessage
{
    public class GenericMessageDto
    {
        public int MsgGenericID { get; set; }
        public string Subject { get; set; }
        public string GenericBody { get; set; }
        public string FileName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public byte Priority { get; set; }
        public short Location { get; set; }
        public byte Status { get; set; }
    }
}
