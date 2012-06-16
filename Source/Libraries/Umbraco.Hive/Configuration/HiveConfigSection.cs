using System.Configuration;

namespace Umbraco.Hive.Configuration
{
    public class HiveConfigSection : ConfigurationSection
    {
        public const string ConfigXmlKey = "umbraco/hive";


        private AvailableProvidersElement _providers;
        private ProviderMappingElementCollection _mappings;

        [ConfigurationProperty("available-providers")]
        public AvailableProvidersElement Providers
        {
            get
            {
                return base["available-providers"] as AvailableProvidersElement ?? _providers ?? (_providers = new AvailableProvidersElement());
            }
            set { base["available-providers"] = value; }
        }

        /// <summary>
        /// Gets or sets the provider groups.
        /// </summary>
        /// <value>The provider groups.</value>
        [ConfigurationProperty(ProviderMappingElementCollection.XmlElementName)]
        public ProviderMappingElementCollection ProviderMappings
        {
            get { return this[ProviderMappingElementCollection.XmlElementName] as ProviderMappingElementCollection ?? _mappings ?? (_mappings = new ProviderMappingElementCollection()); }

            set { this[ProviderMappingElementCollection.XmlElementName] = value; }
        }

        public string GetXml()
        {
            return base.SerializeSection(this, "test-output", ConfigurationSaveMode.Full);
        }
    }
}
