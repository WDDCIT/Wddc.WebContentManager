using System;
using System.Net.Mail;
using System.Text;
using System.Xml.Serialization;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging.DataTypes
{
    [XmlRoot(ElementName = "errordata", Namespace = "")]
    public class ErrorDataType : DataType
    {
        public const string Name = "ErrorData";
        
        [XmlElement(ElementName = "number")]
        public int Number { get; set; }
        
        [XmlElement(ElementName = "severity")]
        public int Severity { get; set; }
        
        [XmlElement(ElementName = "state")]
        public int State { get; set; }
        
        [XmlElement(ElementName = "procedure")]
        public string Procedure { get; set; }
        
        [XmlElement(ElementName = "line")]
        public string Line { get; set; }
        
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        public override string PrettyPrint()
        {
            var sb = new StringBuilder();
            sb.Append(@"<dl class=""dl-horizontal"">");
            sb.Append(@"<dt>Message:</dt>");
            sb.Append(String.Format(@"<dd>{0}</dd>", Message));
            sb.Append(@"<dt>Number:</dt>");
            sb.Append(String.Format(@"<dd>{0}</dd>", Number));
            sb.Append(@"<dt>Severity:</dt>");
            sb.Append(String.Format(@"<dd>{0}</dd>", Severity));
            sb.Append(@"<dt>State:</dt>");
            sb.Append(String.Format(@"<dd>{0}</dd>", State));
            sb.Append(@"<dt>Procedure:</dt>");
            sb.Append(String.Format(@"<dd>{0}</dd>", Procedure));
            sb.Append(@"<dt>Line:</dt>");
            sb.Append(String.Format(@"<dd>{0}</dd>", Line));
            sb.Append(@"</dl>");
            return sb.ToString();
        }

        public override Attachment GetAttachment()
        {
            return DataType.GetAttachmentXml<ErrorDataType>(this);
        }

        public override bool Equals(object obj)
        {
            var c = (ErrorDataType)obj;
            if (c == null )
                return false;

            return (this.Message == c.Message) && (this.Line == c.Line)
                && (this.Number == c.Number) && (this.Procedure == c.Procedure)
                && (this.Severity == c.Severity) && (this.State == c.State);
        }

        public override int GetHashCode()
        {
            return 
                ((this.Message == null ? String.Empty.GetHashCode() : this.Message.GetHashCode()) 
                + (this.Line == null ? String.Empty.GetHashCode() : this.Line.GetHashCode())
                + (this.Number.GetHashCode() )
                + (this.Procedure == null ? 0 : this.Procedure.GetHashCode())
                + (this.Severity.GetHashCode()) 
                + (this.State.GetHashCode())) ^ 3; 
        }

        public override void Verify()
        {
            if (String.IsNullOrEmpty(this.Message))
                throw new XmlPropertyNullOrBlankException("Exception");
        }
    }
}
