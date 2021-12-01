using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Wddc.WebContentManager.Core.Extensions
{
    public static class XElementExtensions
    {
        public static T FromXElement<T>(this XElement xElement)
        {
            var dcs = new XmlSerializer(typeof(T));
            return (T)dcs.Deserialize(xElement.CreateReader());
        }
    }
}
