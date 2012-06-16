using System;
using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Dashboards.Filters
{
    public class DashboardFilterMetadata : PluginMetadataComposition
    {
        public DashboardFilterMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }
    }
}