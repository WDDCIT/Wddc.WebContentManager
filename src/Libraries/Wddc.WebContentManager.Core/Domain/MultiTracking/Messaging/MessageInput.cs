using System;
using System.Xml.Serialization;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging
{
    public class MessageInput
    {
        [XmlElement("class")]
        public string Class { get; set; }

        [XmlElement("spname")]
        public string StoredProcedureName { get; set; }

        [XmlElement("params")]
        public string Params { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

    }
}