using System.Web.Configuration;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Dashboards.Filters
{
    public abstract class DashboardFilter : IActionFilter, IResultFilter
    {
        /// <summary>
        /// Initialize is called after the filter is created but before running the action
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="action">Whether the match was allowed or denied</param>
        public void Initialize(string parameters, AuthorizationRuleAction action)
        {
            Parameters = parameters;
            Action = action;
        }

        protected AuthorizationRuleAction Action { get; private set; }
        protected string Parameters { get; private set; }
        
        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public virtual void OnResultExecuting(ResultExecutingContext filterContext)
        {
        }

        public virtual void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}