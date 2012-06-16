using System.Configuration;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public class DashboardApplicationsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DashboardApplicationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DashboardApplicationElement) element).ApplicationAlias;
        }


    }
}