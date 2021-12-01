using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging.DataTypes
{
    [XmlInclude(typeof(ErrorDataType))]
    [XmlInclude(typeof(OrderDataType))]
    public abstract class DataType
    {
        public abstract string PrettyPrint();
        public abstract Attachment GetAttachment();

        public static Attachment GetAttachmentXml<T>(T obj)
        {
            var xsSubmit = new XmlSerializer(typeof(T));
            using (var sww = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, obj);
                    var byteArray = Encoding.UTF8.GetBytes(sww.ToString());
                    var stream = new MemoryStream(byteArray);
                    return new Attachment(stream, String.Format("Issue_{0}.xml", DateTime.Now.ToString("yyyyMMddHHmmss")));
                }
            }
        }

        public override bool Equals(object obj)
        {
            if ((DataType)obj == null)
                return false;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public abstract void Verify();
    }
}
