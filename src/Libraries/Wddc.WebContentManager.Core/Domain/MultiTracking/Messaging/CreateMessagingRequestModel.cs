using System;
using System.Collections.Generic;
using System.IO;
namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging
{
    public class CreateMessagingRequestModel
    {
        public string MessageBody { get; set; }
        public string MessageSubject { get; set; }
        public int ProcessId { get; set; }
    }

}
