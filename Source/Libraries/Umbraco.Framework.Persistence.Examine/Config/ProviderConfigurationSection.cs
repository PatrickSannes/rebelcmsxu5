using System.Configuration;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Framework.Persistence.Examine.Config
{
    /// <summary>
    /// Defines the Examine searchers/indexers/index set to be used for the Umbraco internal searching
    /// </summary>
    public class ProviderConfigurationSection : AbstractProviderConfigurationSection
    {
        private const string InternalIndexSetKey = "internalIndexSet";
        private const string InternalSearcherKey = "internalSearcher";
        private const string InternalIndexerKey = "internalIndexer";

        [ConfigurationProperty(InternalIndexerKey, IsRequired = true)]
        public string InternalIndexer
        {
            get { return (string)this[InternalIndexerKey]; }

            set { this[InternalIndexerKey] = value; }
        }

        [ConfigurationProperty(InternalSearcherKey, IsRequired = true)]
        public string InternalSearcher
        {
            get { return (string)this[InternalSearcherKey]; }

            set { this[InternalSearcherKey] = value; }
        }

        [ConfigurationProperty(InternalIndexSetKey, IsRequired = true)]
        public string InternalIndexSet
        {
            get { return (string)this[InternalIndexSetKey]; }

            set { this[InternalIndexSetKey] = value; }
        }        
    }
}
