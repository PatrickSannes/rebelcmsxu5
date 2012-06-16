using System.Web;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;

namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// This constraint must pass to route anything to any back office controllers
    /// </summary>
    public class BackOfficeRouteConstraint : IRouteConstraint
    {
        private readonly IUmbracoApplicationContext _applicationContext;

        public BackOfficeRouteConstraint(IUmbracoApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
          
            return true;
        }
    }
}