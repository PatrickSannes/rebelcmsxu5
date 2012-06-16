using System;
using System.Configuration;
using System.Web.Configuration;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public class MatchFilterElement : ConfigurationElement, IDashboardMatchFilter
    {
        public AuthorizationRuleAction Action { get; set; }

        public Type MatchFilterType
        {
            get { return Type.GetType(MatchFilterTypeName); }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string MatchFilterTypeName
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

        public string DataValue
        {
            get { return Data == null ? "" : Data.Value; }
        }

        [ConfigurationProperty("", IsRequired = false, IsDefaultCollection = true)]
        public ConfigurationTextElement<string> Data
        {
            get
            {
                return (ConfigurationTextElement<string>)base[""];
            }          
        }
    }
}