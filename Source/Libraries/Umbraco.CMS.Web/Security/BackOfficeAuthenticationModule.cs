using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using Umbraco.Cms.Web.Configuration;

namespace Umbraco.Cms.Web.Security
{
    /// <summary>
    /// Authentication module to manage the back-office custom forms authentication
    /// </summary>
    /// <remarks>
    /// This also manages the temporary custom login for the Installer
    /// </remarks>
    public class BackOfficeAuthenticationModule: IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += AuthenticateRequest;
        }

        public void Dispose()
        {
            
        }

        /// <summary>
        /// Authenticates the request by reading the FormsAuthentication cookie and setting the 
        /// context and thread principle object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void AuthenticateRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var http = new HttpContextWrapper(app.Context);

            //we need to determine if the path being requested is an umbraco path, if not don't do anything
            var settings = UmbracoSettings.GetSettings();
            var currentPath = app.Request.Url.AbsolutePath;
            
            var fullBackOfficePath = string.Concat(app.Request.ApplicationPath.TrimEnd('/'), "/", settings.UmbracoPaths.BackOfficePath);
            var fullInstallerPath = string.Concat(app.Request.ApplicationPath.TrimEnd('/'), "/", "Install");

            if (currentPath.StartsWith(fullBackOfficePath, StringComparison.InvariantCultureIgnoreCase)
                || currentPath.StartsWith(fullInstallerPath, StringComparison.InvariantCultureIgnoreCase))
            {
                if (app.Context.User == null)
                {
                    if (app.User != null)
                    {
                        //set the principal object
                        app.Context.User = app.User;
                        Thread.CurrentPrincipal = app.User;
                    }
                    else
                    {

                        var ticket = http.GetUmbracoAuthTicket();
                        if (ticket != null && !ticket.Expired && http.RenewUmbracoAuthTicket())
                        {   
                            //create the Umbraco user identity 
                            var identity = new UmbracoBackOfficeIdentity(ticket);

                            //set the principal object
                            var principal = new GenericPrincipal(identity, identity.Roles);
                            app.Context.User = principal;
                            Thread.CurrentPrincipal = principal;
                        }
                    }
                }
            }

            
        }

        public static UmbracoBackOfficeIdentity GetUmbracoBackOfficeIdentity(HttpContextBase http)
        {
            var ticket = http.GetUmbracoAuthTicket();
            if (ticket != null && !ticket.Expired && http.RenewUmbracoAuthTicket())
            {
                return new UmbracoBackOfficeIdentity(ticket);
            }
            return null;
        }
    }
}