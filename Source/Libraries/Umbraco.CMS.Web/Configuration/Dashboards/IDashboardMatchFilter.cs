using System;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public interface IDashboardMatchFilter
    {
        Type MatchFilterType { get; }
        string DataValue { get; }
    }
}