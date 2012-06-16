using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// A route created to ignore already routed plugin controllers from being routable in the fallback package routes.
    /// </summary>
    /// <example>
    /// 
    /// An example of this is an Editor controller in a package will get a URL such as:
    /// /Umbraco/SystemInfo/Editors/SystemInfoEditor/Backup
    /// 
    /// but because we are also auto routing non-plugin controllers found in packages to:
    /// /Umbraco/{PackageName}/{Controller/{Action}
    /// 
    /// then the above editor controller would also match this:
    /// /Umbraco/SystemInfo/SystemInfoEditor/Backup
    /// 
    /// which we don't want to allow, so this route will ensure requests to previously registered plugin controllers, based on the controller name
    /// are ignored for default routes.
    /// 
    /// </example>    
    /// <remarks>
    /// This does however mean that you cannot have a non-plugin controller in you package with the same name as a plugin controller, even if they are in a different namespace.
    /// </remarks>
    internal class IgnorePluginRoute : Route
    {
        private readonly string _areaName;
        private readonly IUmbracoApplicationContext _appContext;
        private readonly IEnumerable<ControllerPluginMetadata> _toIgnore;

        public IgnorePluginRoute(string url, string areaName, IUmbracoApplicationContext appContext, IEnumerable<ControllerPluginMetadata> toIgnore)
            : base(url, new StopRoutingHandler())
        {
            _areaName = areaName;
            _appContext = appContext;
            _toIgnore = toIgnore;
            Constraints = new RouteValueDictionary { { "ignore-existing", new IgnoreExistingControllersConstraint(appContext, _areaName, _toIgnore) } };
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues)
        {
            //NOTE: comments taken from MS source:
            // Never match during route generation. This avoids the scenario where an IgnoreRoute with 
            // fairly relaxed constraints ends up eagerly matching all generated URLs.
            return null;
        }

        /// <summary>
        /// The internal constraint class to match the previously registered controller names
        /// </summary>
        private class IgnoreExistingControllersConstraint : IRouteConstraint
        {
            private readonly IUmbracoApplicationContext _appContext;
            private readonly string _areaName;
            private readonly IEnumerable<string> _toIgnore;

            /// <summary>
            /// maintains a list of already ignored paths
            /// </summary>
            private readonly ConcurrentDictionary<string, bool> _alreadyProcessed = new ConcurrentDictionary<string, bool>();

            public IgnoreExistingControllersConstraint(IUmbracoApplicationContext appContext, string areaName, IEnumerable<ControllerPluginMetadata> toIgnore)
            {
                _appContext = appContext;
                _areaName = areaName;
                _toIgnore = toIgnore.Select(x => x.ControllerName).ToArray();
            }

            public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
            {
                if (routeDirection == RouteDirection.IncomingRequest)
                {
                    if (_alreadyProcessed.ContainsKey(httpContext.Request.Url.AbsolutePath))
                    {
                        return _alreadyProcessed[httpContext.Request.Url.AbsolutePath];
                    }
                    var pathParts = httpContext.Request.Url.AbsolutePath.Split('/').ToList();
                    var backOfficePart = pathParts.IndexOf(_appContext.Settings.UmbracoPaths.BackOfficePath);
                    //ensure there's enough parts to match
                    if (backOfficePart == -1 || ((pathParts.Count - 1) < backOfficePart + 2))
                    {
                        _alreadyProcessed.AddOrUpdate(httpContext.Request.Url.AbsolutePath, false, (k, v) => false);
                        return false;
                    }
                    //ensure were matching the correct area
                    if (pathParts[backOfficePart + 1] != _areaName)
                    {
                        _alreadyProcessed.AddOrUpdate(httpContext.Request.Url.AbsolutePath, false, (k, v) => false);
                        return false;
                    }
                    //+2 because the area name comes after the back office path
                    var controller = pathParts[backOfficePart + 2];
                    if (_toIgnore.InvariantContains(controller))
                    {
                        _alreadyProcessed.AddOrUpdate(httpContext.Request.Url.AbsolutePath, true, (k, v) => true);
                        return true;
                    }
                }

                return false;

            }
        }
    }
}