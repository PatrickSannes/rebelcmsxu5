using System.Configuration;

namespace Umbraco.Hive.Configuration
{
    public class ProviderMapElement : ConfigurationElement
    {
        private const string XKeyKey = "key";

        [ConfigurationProperty(XKeyKey, IsKey = true)]
        public string Key
        {
            get { return (string)this[XKeyKey]; }

            set { this[XKeyKey] = value; }
        }

        private ReaderTypeReferenceElementCollection _readerCollection;
        private ReadWriterTypeReferenceElementCollection _readWriterCollection;

        [ConfigurationProperty(ReaderTypeReferenceElementCollection.XmlElementName)]
        public ReaderTypeReferenceElementCollection Readers
        {
            get { return this[ReaderTypeReferenceElementCollection.XmlElementName] as ReaderTypeReferenceElementCollection ?? _readerCollection ?? (_readerCollection = new ReaderTypeReferenceElementCollection()); }

            set { this[ReaderTypeReferenceElementCollection.XmlElementName] = value; }
        }

        [ConfigurationProperty(ReadWriterTypeReferenceElementCollection.XmlElementName)]
        public ReadWriterTypeReferenceElementCollection ReadWriters
        {
            get { return this[ReadWriterTypeReferenceElementCollection.XmlElementName] as ReadWriterTypeReferenceElementCollection ?? _readWriterCollection ?? (_readWriterCollection = new ReadWriterTypeReferenceElementCollection()); }

            set { this[ReadWriterTypeReferenceElementCollection.XmlElementName] = value; }
        }

        [ConfigurationProperty(UriMatchElementCollection.XmlElementName)]
        public UriMatchElementCollection UriMatches
        {
            get { return (UriMatchElementCollection)this[UriMatchElementCollection.XmlElementName]; }

            set { this[UriMatchElementCollection.XmlElementName] = value; }
        }
    }
}