using System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// This checks if the result is a RedirectToRouteResult and if so appends a 'tabindex' query string 
    /// with the 'ActiveTabIndex' parameter in the ValueProvider (the key can be customized)
    /// </summary>
    public class PersistTabIndexOnRedirectAttribute : ActionFilterAttribute
    {
        private readonly string _postValueKey;

        /// <summary>
        /// Constructor specifying "ActiveTabIndex" as the default key
        /// </summary>
        public PersistTabIndexOnRedirectAttribute()
        {
            _postValueKey = "ActiveTabIndex";
        }

        /// <summary>
        /// Constructor allowing you to specify the post data key to use to send as a query string to the redirect
        /// </summary>
        /// <param name="postValueKey"></param>
        public PersistTabIndexOnRedirectAttribute(string postValueKey)
        {
            _postValueKey = postValueKey;
        }

        public const string QueryStringParameterName = "tabindex";

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {         
            if (filterContext.Result is RedirectToRouteResult)
            {
                var value = filterContext.Controller.ValueProvider.GetValue(_postValueKey);
                if (value != null)
                {
                    int index;
                    if (int.TryParse(value.AttemptedValue, out index))
                    {
                        var redirectResult = (RedirectToRouteResult)filterContext.Result;
                        redirectResult.RouteValues.Add(QueryStringParameterName, index);
                    }    
                }
                
            }           
        }
    }
}