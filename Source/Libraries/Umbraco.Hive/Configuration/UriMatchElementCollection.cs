using System.Configuration;
using Umbraco.Framework.Configuration;

namespace Umbraco.Hive.Configuration
{
    [ConfigurationCollection(typeof(UriMatchElement), AddItemName = "match", RemoveItemName = "remove-match")]
    public class UriMatchElementCollection : ConfigurationElementCollection<string, UriMatchElement>
    {
        public const string XmlElementName = "uri-matches";

        /// <summary>
        /// Gets the type of the <see cref="T:System.Configuration.ConfigurationElementCollection"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Configuration.ConfigurationElementCollectionType"/> of this collection.
        /// </returns>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

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

        protected override string GetElementKey(UriMatchElement element)
        {
            return element.UriPattern;
        }
    }
}