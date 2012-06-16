using System.Collections.Generic;
using Umbraco.Cms.Web.Model.BackOffice.Editors;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    public class DashboardApplicationModel
    {
        public IEnumerable<Tab> Tabs { get; set; }

        public IEnumerable<DashboardItemModel> Dashboards { get; set; }
    }
}