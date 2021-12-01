using System;
using System.Runtime.Serialization;

namespace Wddc.WebContentManager.Core.Domain.MultiTracking.Messaging
{
    [Serializable]
    public class XmlPropertyNullOrBlankException : Exception
    {
        public XmlPropertyNullOrBlankException()
        {
        }

        public XmlPropertyNullOrBlankException(string message) : 
            base(String.Format("Property '{0}' is required to be set", message))
        {
        }

        public XmlPropertyNullOrBlankException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected XmlPropertyNullOrBlankException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}