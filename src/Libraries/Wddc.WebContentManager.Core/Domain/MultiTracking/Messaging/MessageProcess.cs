using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging
{
    public class MessageProcess
    {
        [XmlAttribute(AttributeName = "id")]
        public int id { get; set; }

        public MessageProcess(int id)
        {
            this.id = id;
        }

        public MessageProcess() { }
    }
}
