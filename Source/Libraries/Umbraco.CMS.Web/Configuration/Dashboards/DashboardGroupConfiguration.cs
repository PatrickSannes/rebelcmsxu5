using System.Configuration;
using System.Linq;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public class DashboardGroupConfiguration : ConfigurationSection, IDashboardConfig
    {

        public const string ConfigXmlKey = UmbracoSettings.GroupXmlKey + "/dashboard-groups";

        [ConfigurationCollection(typeof(DashboardGroupCollection), AddItemName = "group")]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public DashboardGroupCollection DashboardGroups
        {
            get
            {
                return (DashboardGroupCollection)base[""];
            }
        }

        global::System.Collections.Generic.IEnumerable<IDashboardGroup> IDashboardConfig.Groups
        {
            get
            {
                return DashboardGroups.OnlyLocalConfig<DashboardGroupElement>();
            }
        }
    }
}
