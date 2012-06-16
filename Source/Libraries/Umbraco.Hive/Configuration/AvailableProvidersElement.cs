using System.Configuration;

namespace Umbraco.Hive.Configuration
{
    public class AvailableProvidersElement : ConfigurationElement
    {
        private TypeLoaderElementCollection _readerCollection;
        private TypeLoaderElementCollection _readWriterCollection;

        [ConfigurationProperty("readers")]
        public TypeLoaderElementCollection Readers
        {
            get { return this["readers"] as TypeLoaderElementCollection ?? _readerCollection ?? (_readerCollection = new TypeLoaderElementCollection()); }

            set { this["readers"] = value; }
        }

        [ConfigurationProperty("writers")]
        public TypeLoaderElementCollection ReadWriters
        {
            get { return this["writers"] as TypeLoaderElementCollection ?? _readWriterCollection ?? (_readWriterCollection = new TypeLoaderElementCollection()); }

            set { this["writers"] = value; }
        }
    }
}