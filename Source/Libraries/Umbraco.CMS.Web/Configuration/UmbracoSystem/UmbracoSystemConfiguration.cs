using System.Configuration;

namespace Umbraco.Cms.Web.Configuration.UmbracoSystem
{
    public class UmbracoSystemConfiguration : ConfigurationSection
    {
        public const string ConfigXmlKey = UmbracoSettings.GroupXmlKey + "/system";

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("public-packages", IsRequired = true)]
        public PublicPackageRepositoryElement PublicPackageRepository
        {
            get
            {
                return (PublicPackageRepositoryElement)base["public-packages"];
            }
        }

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("folders", IsRequired = true)]
        public UmbracoFoldersElement Folders
        {
            get
            {
                return (UmbracoFoldersElement)base["folders"];
            }
        }

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("paths", IsRequired = true)]
        public UmbracoPathsElement Paths
        {
            get
            {
                return (UmbracoPathsElement)base["paths"];
            }
        }

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("urls", IsRequired = true)]
        public UmbracoUrlsElement Urls
        {
            get
            {
                return (UmbracoUrlsElement)base["urls"];
            }
        }

        [ConfigurationProperty(RouteMatchElementCollection.CollectionXmlKey, IsRequired = true)]
        public RouteMatchElementCollection RouteMatches
        {
            get { return this[RouteMatchElementCollection.CollectionXmlKey] as RouteMatchElementCollection; }
            set
            {
                this[RouteMatchElementCollection.CollectionXmlKey] = value;
            }
        }
    }
}
