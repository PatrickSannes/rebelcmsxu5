using System.Configuration;

namespace Umbraco.Hive.Providers.Membership.Config
{
    public class ProviderElement : ConfigurationElement
    {

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Each membership provider might have a different wildcard character specified to use when using FindByName, etc...
        /// </summary>
        [ConfigurationProperty("wildcardChar", IsRequired = true)]
        public string WildcardCharacter
        {
            get { return (string)this["wildcardChar"]; }
            set { this["wildcardChar"] = value; }
        }

    }
}