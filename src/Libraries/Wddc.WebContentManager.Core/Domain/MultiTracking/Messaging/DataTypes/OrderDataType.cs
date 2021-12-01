using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Wddc.Core.Domain.Messaging.Orders;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging.DataTypes
{
    [XmlRoot(ElementName = "orderdata", Namespace = "")]
    public class OrderDataType : DataType
    {
        public const string Name = "OrderData";

        [XmlArray("orders")]
        [XmlArrayItem("order")]
        public List<Order> Orders { get; set; } = new List<Order>();

        public override Attachment GetAttachment()
        {
            return DataType.GetAttachmentXml<OrderDataType>(this);
        }

        public override bool Equals(object obj)
        {
            var c = (OrderDataType)obj;
            if (c == null || !base.Equals(obj))
                return false;

            return (this.Orders == c.Orders);
        }

        public override int GetHashCode()
        {
            return this.Orders.GetHashCode();
        }

        public override void Verify()
        {
            if (Orders.Any(x=>x.ID == ""))
                throw new XmlPropertyNullOrBlankException("Order.ID");
        }

        public override string PrettyPrint()
        {
            throw new NotImplementedException();
        }
    }
}
