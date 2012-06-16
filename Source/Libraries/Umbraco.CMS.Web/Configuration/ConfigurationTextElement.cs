using System.Configuration;
using System.Xml;

namespace Umbraco.Cms.Web.Configuration
{
    public class ConfigurationTextElement<T> : ConfigurationElement
    {
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            Value = (T) reader.ReadElementContentAs(typeof (T), null);
            
        }

        public T Value { get; private set; }
    }
}