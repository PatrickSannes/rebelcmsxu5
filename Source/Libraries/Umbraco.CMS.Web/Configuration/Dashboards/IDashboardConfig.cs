using System.Collections.Generic;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public interface IDashboardConfig
    {
        IEnumerable<IDashboardGroup> Groups { get; }
    }
}