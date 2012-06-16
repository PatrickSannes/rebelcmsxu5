using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Umbraco.Cms.Web.Configuration.UmbracoSystem;

namespace Umbraco.Cms.Web.Routing
{
    public class IncludeExcludeRouteConstraint : IRouteConstraint
    {

        public IEnumerable<RouteMatchElement> Matches { get; set; }

        public IncludeExcludeRouteConstraint(IEnumerable<RouteMatchElement> matches)
        {
            Matches = matches;
        }

        /// <summary>
        /// Determines whether the URL parameter contains a valid value for this constraint.
        /// </summary>
        /// <returns>
        /// true if the URL parameter contains a valid value; otherwise, false.
        /// </returns>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param><param name="route">The object that this constraint belongs to.</param><param name="parameterName">The name of the parameter that is being checked.</param><param name="values">An object that contains the parameters for the URL.</param><param name="routeDirection">An object that indicates whether the constraint check is being performed when an incoming request is being handled or when a URL is being generated.</param>
        public virtual bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var pathToCheck = string.Empty;
            if (httpContext.Request != null)
            {
                if (httpContext.Request.Url != null)
                    pathToCheck = httpContext.Request.Url.AbsolutePath;

                if (string.IsNullOrWhiteSpace(pathToCheck))
                    pathToCheck = httpContext.Request.AppRelativeCurrentExecutionFilePath;
            }

            var match = (!Matches.ContainsExclusion(pathToCheck) &&
                         Matches.ContainsInclusion(pathToCheck));

            return match;
        }
    }
}