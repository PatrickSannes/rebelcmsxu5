using System.Collections.Generic;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public interface IDashboardGroup
    {
        IEnumerable<IDashboardApplication> Applications { get; }
        IEnumerable<IDashboard> Dashboards { get; }
        IEnumerable<IDashboardMatch> Matches { get; } 
    }
}