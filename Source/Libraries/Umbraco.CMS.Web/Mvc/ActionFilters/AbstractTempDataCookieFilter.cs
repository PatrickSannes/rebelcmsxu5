using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// An abstract ActionFilter used to store temporary cookie data that is only persisted for one redirect
    /// </summary>
    public abstract class AbstractTempDataCookieFilter : ActionFilterAttribute
    {
        public abstract string CookieNamePrefix { get; }
        public abstract string QueryStringName { get; }

        /// <summary>
        /// Returns the key used to reference the cookie temp data 
        /// </summary>
        /// <returns></returns>
        protected abstract string GetTempDataKey(ControllerContext filterContext);

        /// <summary>
        /// Clears out all cookies starting with the prefix
        /// </summary>
        /// <param name="filterContext"></param>
        /// <remarks>
        /// Generally these cookies will be cleared out on the client side but just in case this will
        /// help ensure that we don't leave residule cookes laying around, even though we've already set 
        /// them to expire in a minute... better safe than sorry
        /// </remarks>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Mandate.ParameterNotNullOrEmpty(CookieNamePrefix, "CookieNamePrefix");
            Mandate.ParameterNotNullOrEmpty(QueryStringName, "QueryStringParameterName");
            
            //we don't want to delete this cookie if the notification query string exists, if that is the case
            //then that means that the cookie was written in a 302 redirect and that cookie needs to persist
            //after this request. If the query string doesn't exist, then it means the cookie should have been 
            //read by the client side. To be safe, if people are saving/publishing at nearly the same time in multiple
            //windows, we'll also not remove the cookies if it is a POST.);
            if (string.IsNullOrEmpty(filterContext.HttpContext.Request.QueryString[QueryStringName])
                && filterContext.HttpContext.Request.RequestType != "POST")
            {
            foreach (var c in filterContext.HttpContext.Request.Cookies.AllKeys.Where(x => x.StartsWith(CookieNamePrefix)))
                {
                    //set it to be expired
                    var cookieToDelete = new HttpCookie(c) { Expires = DateTime.Now.AddDays(-1) };
                    //add this to the response so the cookie is definitely removed when the response is returned
                    filterContext.HttpContext.Response.Cookies.Add(cookieToDelete);
                }
            }
        }

        /// <summary>
        /// Checks the result type, if it is a RedirectToRouteResult it calls the abstract OnRedirect method
        /// otherwise if its a view result calls the abstract OnView method.
        /// </summary>
        /// <param name="filterContext"></param>       
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Mandate.ParameterNotNullOrEmpty(CookieNamePrefix, "CookieNamePrefix");
            Mandate.ParameterNotNullOrEmpty(QueryStringName, "QueryStringParameterName");

            if (filterContext.Result is RedirectToRouteResult)
            {
                AddQueryString((RedirectToRouteResult)filterContext.Result, filterContext);
            }
            else if (filterContext.Result is ViewResult)
            {
                AddViewData((ViewResult)filterContext.Result, filterContext);
            }
        }

        /// <summary>
        /// Adds the query string name with the temp data key to QueryStrings
        /// </summary>
        /// <param name="result"></param>
        /// <param name="filterContext"></param>
        protected virtual void AddQueryString(RedirectToRouteResult result, ActionExecutedContext filterContext)
        {
            //add notification query string to the result
            var key = GetTempDataKey(filterContext);
            var val = GetTempDataValue(filterContext);
            //only add the query string if both the key and value are not null
            if (key != null && val != null)
            {
                result.RouteValues.Add(QueryStringName, key);    
            }
        }

        /// <summary>
        /// Adds the query string name with the temp data key to ViewData
        /// </summary>
        /// <param name="result"></param>
        /// <param name="filterContext"></param>
        protected virtual void AddViewData(ViewResult result, ActionExecutedContext filterContext)
        {
            //add the notificationId to the view data
            var key = GetTempDataKey(filterContext);
            var val = GetTempDataValue(filterContext);
            //only add the view data if both the key and value are not null
            if (key != null && val != null)
            {
                filterContext.Controller.ViewData.Add(QueryStringName, GetTempDataKey(filterContext));
            }
        }

        /// <summary>
        /// Writes the notifications cookie
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            Mandate.ParameterNotNullOrEmpty(CookieNamePrefix, "CookieNamePrefix");
            Mandate.ParameterNotNullOrEmpty(QueryStringName, "QueryStringParameterName");
            CreateCookie(filterContext);
        }

        /// <summary>
        /// Creates the cookie
        /// </summary>
        /// <param name="filterContext"></param>
        protected virtual void CreateCookie(ResultExecutedContext filterContext)
        {
            var cookieVal = GetTempDataValue(filterContext);
            if (cookieVal == null)
            {
                return;
            }

            //create a unique cookie with the id of the current request to keep them unique amongst requests
            var cookieName = string.Concat(CookieNamePrefix, GetTempDataKey(filterContext));

            //create the cookie, ensure it expires in 1 minutes... that should be more than enough time for this request to return 
            //and potentially redirect if the request is successful.
            //the cookie will also be deleted on the client side using JavaScript once it's read.
            //the cookie value is a serialized json version of the messages
            var cookie = new HttpCookie(cookieName)
                {
                    Expires = DateTime.Now.AddMinutes(1),
                    Value = GetTempDataValue(filterContext).ToJsonString()
                };

            //output the cookie
            filterContext.HttpContext.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Returns the value to be stored in the cookie
        /// </summary>
        /// <returns></returns>
        protected abstract object GetTempDataValue(ControllerContext filterContext);
    }
}