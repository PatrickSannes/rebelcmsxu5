using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Security;
using Umbraco.Cms.Web.System.Boot;

using Umbraco.Framework;
using Umbraco.Framework.ProviderSupport;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// Ensures authorization occurs for the installer if it has already completed. If install has not yet occured
    /// then the authorization is successful
    /// </summary>
    public class UmbracoInstallAuthorizeAttribute : AuthorizeAttribute
    {

        public const string InstallRoleName = "umbraco-install-EF732A6E-AA55-4A93-9F42-6C989D519A4F";

        public IUmbracoApplicationContext ApplicationContext { get; set; }


        /// <summary>
        /// Ensures that the user must be in the Administrator or the Install role
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            try
            {
                if (httpContext.User.IsInRole("Administrator") || httpContext.User.IsInRole(InstallRoleName))
                {
                    return base.AuthorizeCore(httpContext);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {            
            Mandate.ParameterNotNull(filterContext, "filterContext");
            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
                throw new InvalidOperationException("Cannot use UmbracoInstallAuthorizeAttribute on a child action");
            if (AuthorizeCore(filterContext.HttpContext))
            {
                //with a little help from dotPeek... this is what it normally would do
                var cache = filterContext.HttpContext.Response.Cache;
                cache.SetProxyMaxAge(new TimeSpan(0L));
                cache.AddValidationCallback(CacheValidateHandler, null);
            }
            else
            {
                Mandate.That<NullReferenceException>(ApplicationContext != null);

                //check the application to see if its installed
                if (ApplicationContext.AllProvidersInstalled())
                {
                    //they aren't authorized but the app has installed, redirect to login
                    throw new HttpException((int)global::System.Net.HttpStatusCode.Unauthorized,
                        "You must login to view this resource.");
                }
                else
                {

                    //if we are not authorized, and we are not installed, we need to log the user in with a temporary/dummy Install account
                    //this is because on subsequent requests, we need to still show the installer even after the providers have been installed.
                    //Without doing this then the installer would just skip to the log in screen and bypass the user creation dialogs, etc...

                    //log the user in with a dummy account with the "Install" role    
                    var username = Guid.NewGuid().ToString("N");
                    filterContext.HttpContext.CreateUmbracoAuthTicket(new UserData
                        {
                            Id = HiveId.Empty.ToString(),
                            Username = username,
                            RealName = username,
                            AllowedApplications = new string[] {},
                            Roles = new[] { InstallRoleName },
                            SessionTimeout = 20,
                            StartContentNode = HiveId.Empty.ToString(),
                            StartMediaNode = HiveId.Empty.ToString()
                        });

                }
            }
        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
        }
    }
}