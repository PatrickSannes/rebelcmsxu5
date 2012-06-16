using System;
using System.Configuration;
using System.Linq;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public class MatchElement : ConfigurationElement, IDashboardMatch
    {

        /// <summary>
        /// Return the type for the match type
        /// </summary>
        public Type MatchType
        {
            get { return Type.GetType(MatchTypeName); }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string MatchTypeName
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

        [ConfigurationProperty("condition", IsRequired = true)]
        public string Condition
        {
            get
            {
                return (string)this["condition"];
            }
            set
            {
                this["condition"] = value;
            }
        }

        [ConfigurationCollection(typeof(MatchFilterCollection))]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public MatchFilterCollection Filters
        {
            get
            {
                return (MatchFilterCollection)base[""];
            }
        }      

        global::System.Collections.Generic.IEnumerable<IDashboardMatchFilter> IDashboardMatch.Filters
        {
            get { return Filters.OnlyLocalConfig<MatchFilterElement>(); }
        }
    }
}