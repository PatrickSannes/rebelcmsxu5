using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web.Configuration.UmbracoSystem;

namespace Umbraco.Cms.Web
{
    public static class RouteMatchExtensions
    {
        /// <summary>
        /// Determines whether the collection contains a matching <see cref="RouteMatchElement"/>, marked as 'include', for the specified absolute path.
        /// </summary>
        /// <param name="routeMatches"></param>
        /// <param name="absolutePath">The absolute path.</param>
        /// <returns>
        /// 	<c>true</c> if the collection contains a match; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsInclusion(this IEnumerable<RouteMatchElement> routeMatches, string absolutePath)
        {
            return routeMatches.Cast<RouteMatchElement>().Any(element => element.Type == RouteMatchTypes.Include && element.IsMatch(absolutePath));
        }

        /// <summary>
        /// Determines whether the collection contains a matching <see cref="RouteMatchElement"/>, marked as 'exclude', for the specified absolute path.
        /// </summary>
        /// <param name="routeMatches"></param>
        /// <param name="absolutePath">The absolute path.</param>
        /// <returns>
        /// 	<c>true</c> if the collection contains a match; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsExclusion(this IEnumerable<RouteMatchElement> routeMatches, string absolutePath)
        {
            return routeMatches.Cast<RouteMatchElement>().Any(element => element.Type == RouteMatchTypes.Exclude && element.IsMatch(absolutePath));
        }
    }
}
