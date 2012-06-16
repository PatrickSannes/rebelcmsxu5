using System.Collections.Generic;
using System.Configuration;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Configuration.ApplicationSettings
{
    public class ApplicationConfiguration : ConfigurationSection, IApplicationCollection
    {
        public const string ConfigXmlKey = UmbracoSettings.GroupXmlKey + "/applications";

        [ConfigurationCollection(typeof(ApplicationsCollection))]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public ApplicationsCollection Applications
        {
            get
            {
                return (ApplicationsCollection)base[""];
            }
        }

        IEnumerable<IApplication> IApplicationCollection.Applications
        {
            get { return Applications.OnlyLocalConfig<IApplication>(); }
        }
    }
}