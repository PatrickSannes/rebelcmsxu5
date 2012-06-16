using System;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mvc.Areas;

using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// Requires that the application is installed
    /// </summary>
    public class InstalledFilterAttribute : FilterAttribute, IAuthorizationFilter, IRequiresRoutableRequestContext
    {

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            Mandate.That<NullReferenceException>(RoutableRequestContext != null);

            if (!RoutableRequestContext.Application.AllProvidersInstalled())
            {
                //if it's not installed, then we need to go to the installer!
                filterContext.Result = new RedirectToRouteResult(
                    InstallAreaRegistration.RouteName,
                    new RouteValueDictionary(new
                        {
                            Action = "Index",
                        }));

                return;
            }
            
        }

        public IRoutableRequestContext RoutableRequestContext { get; set; }
    }
}