using System.Configuration;
using Umbraco.Framework.Configuration;

namespace Umbraco.Hive.Configuration
{
    [ConfigurationCollection(typeof(TypeLoaderElement), AddItemName = "add")]
    public class TypeLoaderElementCollection : ConfigurationElementCollection<string, TypeLoaderElement>
    {
        public const string XmlElementName = "readers";

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

        protected override string GetElementKey(TypeLoaderElement element)
        {
            return element.Key;
        }
    }
}