using System.Configuration;

namespace Umbraco.Hive.Configuration
{
    [ConfigurationCollection(typeof(ReaderTypeReferenceElement), AddItemName = "use")]
    public class ReaderTypeReferenceElementCollection : AbstractTypeReferenceElementCollection<ReaderTypeReferenceElement>
    {
        public const string XmlElementName = "readers";

        /// <summary>
        /// Gets the name used to identify this collection of elements in the configuration file when overridden in a derived class.
        /// </summary>
        /// <returns>
        /// The name of the collection; otherwise, an empty string. The default is an empty string.
        /// </returns>
        protected override string ElementName
        {
            get { return XmlElementName; }
        }
    }
}