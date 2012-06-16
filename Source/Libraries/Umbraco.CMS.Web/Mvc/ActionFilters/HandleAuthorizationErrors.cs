using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// A custom filter for handling 401 (no authenticated) and 403 (not authorized) so that
    /// the custom error pages specified in the web.config will be rendered with the correct 
    /// Http status codes.
    /// </summary>
    public class HandleAuthorizationErrors : ActionFilterAttribute, IExceptionFilter, IRequiresBackOfficeRequestContext
    {
        private readonly bool _showLoginOverlay;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="showLoginOverlay">true to display a login overlay if not authorized, or false to show the login page</param>
        public HandleAuthorizationErrors(bool showLoginOverlay)
        {
            _showLoginOverlay = showLoginOverlay;
        }

        public IBackOfficeRequestContext BackOfficeRequestContext { get; set; }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            if (filterContext.IsChildAction)
            {
                return;
            }

            //NOTE: I used to check for custom errors enabled but then realized that we 'never' really 
            // want to see a YSOD instead of being redirected to the installer or the not authorized page.
            //if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            //    return;
            if (filterContext.ExceptionHandled)
                return;

            var exception = filterContext.Exception;
            var httpException = new HttpException(null, exception);

            if (httpException.GetHttpCode() != (int)global::System.Net.HttpStatusCode.Forbidden
                && httpException.GetHttpCode() != (int)global::System.Net.HttpStatusCode.Unauthorized)
            {
                //ignore if not 403 or 401
                return;
            }
            
            switch (httpException.GetHttpCode())
            {
                case (int)global::System.Net.HttpStatusCode.Unauthorized:
                    //The user could not login                     

                    object routeVals;
                    if (_showLoginOverlay)
                    {
                        routeVals = new
                            {
                                area = "Umbraco",
                                action = "Login",
                                controller = "Default",
                                ReturnUrl = filterContext.HttpContext.Request.Url.PathAndQuery,
                                displayType = LoginDisplayType.ShowOverlay
                            };
                    }
                    else
                    {
                        //since we're not displaying the overlay, we wont include the other route val, makes urls nicer
                        routeVals = new
                            {
                                area = "Umbraco",
                                action = "Login",
                                controller = "Default",
                                ReturnUrl = filterContext.HttpContext.Request.Url.PathAndQuery
                            };
                    }

                    //redirect to login
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(routeVals));

                    break;
                case (int)global::System.Net.HttpStatusCode.Forbidden:
                    //The user does not have access to the resource, show the insufficient priviledges view

                    //need to update the route values so that Login returns the correct view
                    var defaultController = new DefaultController(BackOfficeRequestContext);
                    filterContext.RouteData.Values["Action"] = "InsufficientPermissions";
                    filterContext.RouteData.Values["Controller"] = "Default";
                    filterContext.HttpContext.Response.StatusCode = (int)global::System.Net.HttpStatusCode.Forbidden;
                    filterContext.Result = defaultController.InsufficientPermissions(filterContext.HttpContext.Request.Url.PathAndQuery);
                    break;
            }
            
            filterContext.ExceptionHandled = true;
            //NOTE: Due to the way the FormsAuthenticationModule in ASP.Net works, specifically in it's OnLeave method
            // it specifically checks for a 401 status code, changes it to a 301 and redirects to the FormsAuthentication.LoginUrl
            // which can only be set singly meaning that you can't have 2 different login URLs even when using the 'location' 
            // element in your web.config. Hopefully MS fixes this in future versions of .Net. In the meantime we have 2 options:
            // - Don't return a 401 Http status code
            // - Hijack the Application_EndRequest to detect that we wanted to render a 401 and change it to 401 which is kinda ugly.
            //   so, we're just not going to return a 401 status code.
            //filterContext.HttpContext.Response.StatusCode = httpException.GetHttpCode();
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            
        }

        
    }
}