using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Dashboards.Rules
{
    public class DashboardRuleMetadata : PluginMetadataComposition
    {
        public DashboardRuleMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }
    }
}