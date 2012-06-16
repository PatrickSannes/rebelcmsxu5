using System.Configuration;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Hive.Providers.IO
{
    public class ConfigSection : AbstractProviderConfigurationSection
    {
        private const string SupportedExtensionsKey = "supportedExtensions";
        private const string RootPathKey = "rootPath";
        private const string ExcludedExtensionsKey = "excludedExtensions";
        private const string ExcludedDirectoriesKey = "excludedDirectories";
        private const string RootPublicDomainKey = "rootPublicDomain";

        [ConfigurationProperty(SupportedExtensionsKey, IsRequired = true)]
        public string SupportedExtensions
        {
            get { return (string)this[SupportedExtensionsKey]; }

            set { this[SupportedExtensionsKey] = value; }
        }

        [ConfigurationProperty(RootPathKey, IsRequired = true)]
        public string RootPath
        {
            get { return (string)this[RootPathKey]; }
            set { this[RootPathKey] = value; }
        }

        [ConfigurationProperty(ExcludedExtensionsKey, IsRequired = false)]
        public string ExcludedExetensions
        {
            get
            {
                return (string)this[ExcludedExtensionsKey];
            }
        }

        [ConfigurationProperty(ExcludedDirectoriesKey, IsRequired = false)]
        public string ExcludedDirectories
        {
            get
            {
                return (string)this[ExcludedDirectoriesKey];
            }
        }

        [ConfigurationProperty(RootPublicDomainKey, IsRequired = false, DefaultValue = "~/")]
        public string RootPublicDomain
        {
            get
            {
                return (string)this[RootPublicDomainKey];
            }
        }
    }
}
