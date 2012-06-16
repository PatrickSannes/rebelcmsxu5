using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public class DashboardGroupElement : ConfigurationElement, IDashboardGroup
    {
        [ConfigurationCollection(typeof(DashboardGroupCollection))]
        [ConfigurationProperty("applications", IsRequired = true)]
        public DashboardApplicationsCollection Applications
        {
            get
            {
                return (DashboardApplicationsCollection)base["applications"];
            }
        }

        [ConfigurationCollection(typeof(DashboardGroupCollection))]
        [ConfigurationProperty("dashboards", IsRequired = true)]
        public DashboardsCollection Dashboards
        {
            get
            {
                return (DashboardsCollection)base["dashboards"];
            }
        }

        [ConfigurationCollection(typeof(DashboardGroupCollection), AddItemName = "match")]
        [ConfigurationProperty("matches", IsRequired = false)]
        public MatchesCollection Matches
        {
            get
            {
                return (MatchesCollection)base["matches"];
            }
        }

        IEnumerable<IDashboardApplication> IDashboardGroup.Applications
        {
            get { return Applications.OnlyLocalConfig<DashboardApplicationElement>(); }
        }

        IEnumerable<IDashboard> IDashboardGroup.Dashboards
        {
            get { return Dashboards.OnlyLocalConfig<DashboardElement>(); }
        }

        IEnumerable<IDashboardMatch> IDashboardGroup.Matches
        {
            get { return Matches.OnlyLocalConfig<MatchElement>(); }
        }
    }
}