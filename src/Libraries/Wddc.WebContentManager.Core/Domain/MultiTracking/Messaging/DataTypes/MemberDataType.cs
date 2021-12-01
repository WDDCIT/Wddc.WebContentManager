using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging.DataTypes
{
    [XmlRoot(ElementName = "memberdata", Namespace = "")]
    public class MemberDataType : DataType
    {
        public const string Name = "MemberData";

        [XmlElement(ElementName = "MemberID")]
        public string MemberID { get; set; }

        [XmlElement(ElementName = "SiteID")]
        public int SiteID { get; set; }

        [XmlElement(ElementName = "MemberName")]
        public string MemberName { get; set; }

        [XmlElement(ElementName = "Address1")]
        public string Address1 { get; set; }

        // add all info

        public override Attachment GetAttachment()
        {
            return DataType.GetAttachmentXml<MemberDataType>(this);
        }

        public override void Verify()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            var c = obj as MemberDataType;
            if (c == null)
                return false;

            return this.Address1 == c.Address1 && this.MemberID == c.MemberID
                && this.MemberName == c.MemberName && this.SiteID == c.SiteID;
        }

        public override int GetHashCode()
        {
            return (this.MemberID == null ? String.Empty.GetHashCode() : this.MemberID.GetHashCode() +
                this.Address1 == null ? String.Empty.GetHashCode() : this.Address1.GetHashCode() +
                this.MemberName == null ? String.Empty.GetHashCode() : this.MemberName.GetHashCode() +
                this.SiteID.GetHashCode()) ^ 7;
        }

        public override string PrettyPrint()
        {
            throw new NotImplementedException();
        }
    }
}
