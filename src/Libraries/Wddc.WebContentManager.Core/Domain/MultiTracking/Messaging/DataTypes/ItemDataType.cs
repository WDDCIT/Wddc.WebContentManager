using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging.DataTypes;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging.DataTypes
{
    [XmlRoot(ElementName = "itemdata", Namespace = "")]
    public class ItemDataType : DataType
    {
        public const string Name = "ItemData";

        [XmlElement(ElementName = "ItemNumber")]
        public int ItemNumber { get; set; }

        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }

        [XmlElement(ElementName = "Price")]
        public decimal Price { get; set; }

        [XmlElement(ElementName = "QuantityOnHand")]
        public int QuantityOnHand { get; set; }

        [XmlElement(ElementName = "VendorNumber")]
        public string VendorNumber { get; set; }

        public override Attachment GetAttachment()
        {
            return DataType.GetAttachmentXml<ItemDataType>(this);
        }

        public override void Verify()
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return (this.Description == null ? String.Empty.GetHashCode() : this.Description.GetHashCode() +
                this.ItemNumber.GetHashCode() + this.Price.GetHashCode() + this.QuantityOnHand.GetHashCode()) ^ 7;
        }

        public override bool Equals(object obj)
        {
            var c = obj as ItemDataType;
            if (c == null)
                return false;

            return this.Description == c.Description && this.ItemNumber == c.ItemNumber &&
                this.Price == c.Price && this.QuantityOnHand == c.QuantityOnHand;
        }

        public override string PrettyPrint()
        {
            throw new NotImplementedException();
        }
    }
}
