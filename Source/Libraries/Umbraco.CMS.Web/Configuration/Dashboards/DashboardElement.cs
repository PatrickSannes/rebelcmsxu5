using System;
using System.Configuration;
using Umbraco.Cms.Web.Model.BackOffice;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public class DashboardElement : ConfigurationElement, IDashboard
    {

        [ConfigurationProperty("tab", IsRequired = true)]
        public string TabName
        {
            get
            {
                return (string)this["tab"];
            }
            set
            {
                this["tab"] = value;
            }
        }

        public DashboardType DashboardType
        {
            get { return (DashboardType)Enum.Parse(typeof(DashboardType), DashboardTypeName, true); }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string DashboardTypeName
        {
            get
            {
                return (string)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }
    }
}