using Umbraco.Cms.Web.Model.BackOffice;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public interface IDashboard
    {
        string TabName { get; }
        DashboardType DashboardType { get; }
        string Name { get; }
    }
}