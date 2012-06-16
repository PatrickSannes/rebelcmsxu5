using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web
{
    /// <summary>
    /// Extensions to create and renew and remove authentication tickets for the Umbraco back office
    /// </summary>
    public static class AuthenticationExtensions
    {

        internal const string UmbracoAuthCookieName = ".UMBAUTH";

        /// <summary>
        /// This clears the forms authentication cookie
        /// </summary>
        public static void UmbracoLogout(this HttpContextBase http)
        {
            Logout(http, UmbracoAuthCookieName);
        }

        /// <summary>
        /// This clears the forms authentication cookie
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        public static void Logout(this HttpContextBase http, string cookieName)
        {
            //remove from the request
            http.Request.Cookies.Remove(cookieName);

            //expire from the response
            var formsCookie = http.Response.Cookies[cookieName];
            if (formsCookie != null)
            {                
                //this will expire immediately and be removed from the browser
                formsCookie.Expires = DateTime.Now.AddYears(-1);
            }
            else
            {
                //ensure there's def an expired cookie
                http.Response.Cookies.Add(new HttpCookie(cookieName) { Expires = DateTime.Now.AddYears(-1) });
            }
        }

        /// <summary>
        /// Renews the Umbraco authentication ticket
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static bool RenewUmbracoAuthTicket(this HttpContextBase http)
        {
            return RenewAuthTicket(http, UmbracoAuthCookieName, 60);
        }

        /// <summary>
        /// Creates the Umbraco authentication ticket, this will look up the associated User
        /// for the supplied username in Hive
        /// </summary>
        /// <param name="http">The HTTP.</param>
        /// <param name="username">The username.</param>
        /// <param name="appContext">The app context.</param>
        public static void CreateUmbracoAuthTicket(this HttpContextBase http, string username, IUmbracoApplicationContext appContext)
        {
            using (var uow = appContext.Hive.OpenWriter<ISecurityStore>(new Uri("security://users")))
            {
                var user = BackOfficeMembershipProvider.GetUmbracoUser(appContext, uow, username, false);
                if (user == null)
                    throw new NullReferenceException("No User found with username " + username);

                http.CreateUmbracoAuthTicket(user);
            }         
        }

        /// <summary>
        /// Creates the Umbraco authentication ticket
        /// </summary>
        /// <param name="http"></param>
        /// <param name="user"></param>
        public static void CreateUmbracoAuthTicket(this HttpContextBase http, User user)
        {
            var roles = Roles.Providers.GetBackOfficeRoleProvider().GetRolesForUser(user.Username);
            var userData = new UserData
            {
                Id = user.Id.ToString(),
                Roles = roles,
                SessionTimeout = user.SessionTimeout,
                Username = user.Username,
                RealName = user.Name,
                StartContentNode = user.StartContentHiveId.IsNullValueOrEmpty() ? HiveId.Empty.ToString() : user.StartContentHiveId.ToString(),
                StartMediaNode = user.StartMediaHiveId.IsNullValueOrEmpty() ? HiveId.Empty.ToString() : user.StartMediaHiveId.ToString(),
                AllowedApplications = user.Applications.ToArray()
            };

            http.CreateUmbracoAuthTicket(userData);
        }

        /// <summary>
        /// Creates the umbraco authentication ticket
        /// </summary>
        /// <param name="http"></param>
        /// <param name="userdata"></param>
        internal static void CreateUmbracoAuthTicket(this HttpContextBase http, UserData userdata)
        {
            var userDataString = (new JavaScriptSerializer()).Serialize(userdata);

            CreateAuthTicket(http, userdata.Username, userDataString, userdata.SessionTimeout, userdata.SessionTimeout, "/", UmbracoAuthCookieName);
        }

        public static FormsAuthenticationTicket GetUmbracoAuthTicket(this HttpContextBase http)
        {
            return GetAuthTicket(http, UmbracoAuthCookieName);
        }

        public static FormsAuthenticationTicket GetAuthTicket(this HttpContextBase http, string cookieName)
        {
            var formsCookie = http.Request.Cookies[cookieName];
            if (formsCookie == null)
            {
                return null;
            }
            //get the ticket
            try
            {
                return FormsAuthentication.Decrypt(formsCookie.Value);
            }
            catch (Exception)
            {
                //occurs when decryption fails
                http.Logout(cookieName);
                return null;
            }
        }

        /// <summary>
        /// Renews the forms authentication ticket & cookie
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        /// <param name="minutesPersisted"></param>
        /// <returns></returns>
        public static bool RenewAuthTicket(this HttpContextBase http, string cookieName, int minutesPersisted)
        {
            //get the ticket
            var ticket = GetAuthTicket(http, cookieName);
            //renew the ticket
            var renewed = FormsAuthentication.RenewTicketIfOld(ticket);
            if (renewed == null)
            {
                return false;
            }
            //encrypt it
            var hash = FormsAuthentication.Encrypt(renewed);
            //write it to the response
            var cookie = new HttpCookie(cookieName, hash)
                             {
                                 Expires = DateTime.Now.AddMinutes(minutesPersisted)
                             };
            //rewrite the cooke
            http.Response.Cookies.Remove(cookieName);
            http.Response.Cookies.Add(cookie);
            return true;
        }

        /// <summary>
        /// Creates a custom FormsAuthentication ticket with the data specified
        /// </summary>
        /// <param name="http"></param>
        /// <param name="username"></param>
        /// <param name="userData"></param>
        /// <param name="loginTimeoutMins"></param>
        /// <param name="minutesPersisted"></param>
        /// <param name="cookiePath"></param>
        /// <param name="cookieName"></param>
        public static void CreateAuthTicket(this HttpContextBase http, 
                                  string username, 
                                  string userData, 
                                  int loginTimeoutMins, 
                                  int minutesPersisted, 
                                  string cookiePath,
                                  string cookieName)
        {
            // Create a new ticket used for authentication
            var ticket = new FormsAuthenticationTicket(
                4,
                username,
                DateTime.Now,
                DateTime.Now.AddMinutes(loginTimeoutMins),
                true,
                userData,
                cookiePath
                );

            // Encrypt the cookie using the machine key for secure transport
            var hash = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(
                cookieName,
                hash)
                             {
                                 Expires = DateTime.Now.AddMinutes(minutesPersisted)
                             };
            
            http.Response.Cookies.Add(cookie);            
        }
    }
}