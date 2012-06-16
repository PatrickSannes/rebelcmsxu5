using System.Configuration;

namespace Umbraco.Hive.Configuration
{
    public class ReadWriterTypeReferenceElement : ConfigurationElement
    {
        private const string XKeyKey = "provider";
        private const string XOrdinalKey = "ordinal";
        private const string XIsPathroughKey = "isPassthrough";

        [ConfigurationProperty(XKeyKey, IsKey = true)]
        public string ProviderKey
        {
            get { return (string)this[XKeyKey]; }

            set { this[XKeyKey] = value; }
        }

        [ConfigurationProperty(XOrdinalKey, DefaultValue = 0)]
        public int Ordinal
        {
            get { return (int)this[XOrdinalKey]; }

            set { this[XOrdinalKey] = value; }
        }

        public string InternalKey { get; set; }

        [ConfigurationProperty(XIsPathroughKey, DefaultValue = false)]
        public bool IsPassthrough
        {
            get { return (bool)this[XIsPathroughKey]; }

            set { this[XIsPathroughKey] = value; }
        }
    }
}