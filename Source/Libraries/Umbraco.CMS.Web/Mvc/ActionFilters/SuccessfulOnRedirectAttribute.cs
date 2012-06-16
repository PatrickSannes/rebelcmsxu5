using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// This checks if the result is a RedirectToRouteResult and if so either:
    ///  - appends a 'success' query string with the parameter specified in the RouteData
    ///  - Or, if a cookie is chosen, a new cookie will be created with the value of the parameter 
    ///     specified in the RouteData and a 'success' query string will be appended with the cookie id that was created
    /// </summary>
    public class SuccessfulOnRedirectAttribute : AbstractTempDataCookieFilter
    {
        /// <summary>
        /// Utility for ensuring that the correct key/value is in the controller context's route data
        /// for the ActionFilter to perform its filtering properly. Useful for injecting the key/value
        /// when the route data doesn't already exist in the request
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void EnsureRouteData(Controller controller, string key, object value)
        {            
            if (controller != null && controller.ControllerContext != null)
            {
                if (controller.ControllerContext.RouteData == null)
                {
                    controller.ControllerContext.RouteData = new global::System.Web.Routing.RouteData();
                }

                controller.ControllerContext.RouteData.Values[key] = value;
            }            
        }

        
        private readonly string _routeDataKey;

        /// <summary>
        /// If the routeDataKey is required to be found in the route data, if true (default) then an exception will be thrown if the route data key is missing
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Will create a cookie with the value instead of appending the value in the query string
        /// </summary>
        public bool EnableCookieMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessfulOnRedirectAttribute"/> class.
        /// </summary>
        public SuccessfulOnRedirectAttribute()
        {
            _routeDataKey = "id";
            IsRequired = true;
            EnableCookieMode = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SuccessfulOnRedirectAttribute"/> class.
        /// </summary>
        /// <param name="routeDataKey">The route data key.</param>
        public SuccessfulOnRedirectAttribute(string routeDataKey)
        {
            _routeDataKey = routeDataKey;
            IsRequired = true;
            EnableCookieMode = false;
        }

        public override string CookieNamePrefix
        {
            get { return "success_"; }
        }

        public override string QueryStringName
        {
            get { return "success"; }
        }

        /// <summary>
        /// Returns the request id as the temp data key
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        protected override string GetTempDataKey(ControllerContext filterContext)
        {
            var backOfficeController = GetController(filterContext.Controller);
            return backOfficeController.BackOfficeRequestContext.RequestId.ToString("N");
        }    

        /// <summary>
        /// Checks if we are using a cookie to store data or just the query string, if using the cookie then passes logic 
        /// to the base class, otherwise puts the query string value in 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="filterContext"></param>
        protected override void AddQueryString(RedirectToRouteResult result, ActionExecutedContext filterContext)
        {
            if (EnableCookieMode)
            {
                base.AddQueryString(result, filterContext);
            }
            else
            {
                var value = GetValueFromRoute(filterContext);
                if (value != null)
                {
                    result.RouteValues.Add(QueryStringName, value);
                }
            }            
        }

        /// <summary>
        /// Only create the cookie if its specified
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void CreateCookie(ResultExecutedContext filterContext)
        {
            if (EnableCookieMode)
            {
                base.CreateCookie(filterContext);    
            }
        }

        /// <summary>
        /// Returns the value to be stored in the cookie or query string
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        protected override object GetTempDataValue(ControllerContext filterContext)
        {
            return GetValueFromRoute(filterContext);
        }

        /// <summary>
        /// Returns the value to be stored in the cookie or query string
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        private object GetValueFromRoute(ControllerContext controllerContext)
        {
            return IsRequired
                       ? controllerContext.RouteData.GetRequiredValue(_routeDataKey)
                       : controllerContext.RouteData.GetOptionalValue(_routeDataKey);
        }

        /// <summary>
        /// Gets the BackOfficeController type from the filter context's controller instance
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        private IRequiresBackOfficeRequestContext GetController(IController controller)
        {
            var backOfficeController = controller as IRequiresBackOfficeRequestContext;
            if (backOfficeController != null)
            {
                return backOfficeController;
            }
            else
            {
                throw new NotSupportedException("The " + GetType().Name + " can only be used on controllers of type " + typeof(IRequiresBackOfficeRequestContext).Name);
            }
        }
    }
}