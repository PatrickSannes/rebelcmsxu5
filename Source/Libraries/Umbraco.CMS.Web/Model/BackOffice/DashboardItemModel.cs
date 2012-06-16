using System.Collections.Generic;
using Umbraco.Cms.Web.Configuration.Dashboards;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    public class DashboardItemModel
    {
        public HiveId TabId { get; set; }

        public string ViewName { get; set; }

        public IEnumerable<IDashboardMatch> Matches { get; set; }

        public DashboardType DashboardType { get; set; }
    }
}