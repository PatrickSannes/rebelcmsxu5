using System.Web.Configuration;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Dashboards.Filters
{
    public class DisplayDashboard : DashboardFilter
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {         
            switch (Action)
            {
                case AuthorizationRuleAction.Deny:
                    filterContext.Result = new ContentResult() { Content = "" };
                    return;
                default:
                    base.OnActionExecuting(filterContext);
                    break;
            }
        }
        
    }
}