using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;

namespace Umbraco.Cms.Web.Dashboards
{
    public abstract class DashboardViewPage : WebViewPage<DashboardViewModel>, IRequiresBackOfficeRequestContext
    {

        public IBackOfficeRequestContext BackOfficeRequestContext { get; set; }

    }
}