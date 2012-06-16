using System;
using System.Configuration;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public class DashboardsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DashboardElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            //allow multiple elements, just means you can't reference by a key
            return Guid.NewGuid();
        }

        
    }
}