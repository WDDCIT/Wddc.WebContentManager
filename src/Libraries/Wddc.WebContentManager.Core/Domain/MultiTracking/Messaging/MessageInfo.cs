using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging
{
    /// <summary>
    /// Basically same data type as DataContext.Message but contains a few decorators
    /// and methods to help with the XML parsing.
    /// </summary>
    [XmlRoot("message", Namespace = "")]
    public class MessageInfo
    {
        public int MessageID { get; set; }

        [XmlElement("errordata", Type = typeof(DataTypes.ErrorDataType))]
        [XmlElement("orderdata", Type = typeof(DataTypes.OrderDataType))]
        public DataTypes.DataType Data { get; set; }
        
        [XmlElement("servername")]
        public string ServerName { get; set; }
        
        [XmlElement("username")]
        public string UserName { get; set; }
        
        [XmlElement("applicationname")]
        public string ApplicationName { get; set; }

        [XmlElement("messagesubject")]
        public string Subject { get; set; }
                
        [XmlElement("messagebody")]
        public string Body { get; set; }
        
        [XmlElement("process")]
        public MessageProcess Process { get; set; }

        [XmlArray("inputs")]
        [XmlArrayItem("input")]
        public MessageInput[] Inputs { get; set; }

        public MessageInfo() { }

        public Attachment FormatAsAttachment()
        {
            var xsSubmit = new XmlSerializer(typeof(MessageInfo));
            using (var sww = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, this);
                    var byteArray = Encoding.UTF8.GetBytes(sww.ToString());
                    var stream = new MemoryStream(byteArray);
                    return new Attachment(stream, String.Format("Issue_{0}.xml", DateTime.Now.ToString("yyyyMMddHHmmss")));
                }
            }
        }
    }
}
