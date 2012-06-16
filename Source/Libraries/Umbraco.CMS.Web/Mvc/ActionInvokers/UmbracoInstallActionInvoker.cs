using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mvc.ActionFilters;

namespace Umbraco.Cms.Web.Mvc.ActionInvokers
{
    /// <summary>
    /// Ensures that any filter of type UmbracoInstallAction get's an IApplicationContext set
    /// </summary>
    internal class UmbracoInstallActionInvoker : UmbracoBackOfficeActionInvoker
    {

        public UmbracoInstallActionInvoker(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        {
            
        }

        protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            base.GetFilters(controllerContext, actionDescriptor);

            var filters = base.GetFilters(controllerContext, actionDescriptor);
            foreach (var filter in
                filters.AuthorizationFilters.Where(filter => filter.GetType().Equals(typeof(UmbracoInstallAuthorizeAttribute))))
            {
                ((UmbracoInstallAuthorizeAttribute)filter).ApplicationContext = BackOfficeRequestContext.Application;
            }
            return filters;
        }

    }
}