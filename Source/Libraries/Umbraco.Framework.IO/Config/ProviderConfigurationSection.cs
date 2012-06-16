using System.Configuration;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Framework.IO.Config
{
    public class ProviderConfigurationSection : AbstractProviderConfigurationSection
    {
        private const string SupportedExtensionsKey = "supportedExtensions";
        private const string RootPathKey = "rootPath";
        private const string ExcludeExtensionsKey = "excludeProperties";

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

        [ConfigurationProperty(ExcludeExtensionsKey, IsRequired = false)]
        public string ExcludeExetensions
        {
            get
            {
                return (string)this[ExcludeExtensionsKey];
            }
        }
    }
}
