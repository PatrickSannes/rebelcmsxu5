using System.Web.Hosting;
using System.Web.Routing;
using Umbraco.CMS.Web.EmbeddedViewEngine;
using Umbraco.CMS.Web.Mvc.Areas;
using Umbraco.CMS.Web.Mvc.RoutableRequest;
using Umbraco.CMS.Web.Mvc.RouteHandlers;
using Umbraco.Framework.DependencyManagement;

namespace Umbraco.CMS.Web.UmbracoSystem
{
    /// <summary>
    /// Used to setup all of the Umbraco specific MVC routes, AutoMapper and IoC
    /// </summary>
    internal class UmbracoInitializer
    {
        private static bool _isInitialized;
        private static readonly object Locker = new object();
        private readonly IDependencyResolver _container;

        /// <summary>
        /// Creates a new UmbracoInitializer
        /// </summary>
        /// <param name="container">The IoC container to use for manual resolution of objects</param>
        internal UmbracoInitializer(IDependencyResolver container)
        {
            _container = container;
        }

        /// <summary>
        /// Performs the initialization
        /// </summary>
        /// <param name="routes"></param>
        /// <remarks>
        /// Initialization will only happen once
        /// </remarks>
        internal void Initialize(RouteCollection routes)
        {
            if (!_isInitialized)
            {
                lock (Locker)
                {
                    if (!_isInitialized)
                    {
                        //register the Umbraco area, this requires manually interventino because we have cosntructor dependencies on the UmbracoArea
                        routes.RegisterArea<UmbracoAreaRegistration>(_container);

                        //setup automapper
                        var autoMapperInit = _container.Resolve<AutoMapperInitializer>();
                        autoMapperInit.EnsureInitialised();

                        var routableRequestProvider = _container.Resolve<IRoutableRequestProvider>();

                        //setup routes and front-end route handler
                        //TODO: Remove magic strings
                        var frontEndRouteHandler = _container.Resolve<IRouteHandler>("FrontEndRouteHandler");

                        ConfigureFrontEndRoutes(routes, frontEndRouteHandler, routableRequestProvider);

                        //adds custom virtual path provider
                        HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedViewVirtualPathProvider());

                        //set as init
                        _isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Creates the Umbraco routes for the front-end
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <param name="routeHandler">The route handler.</param>
        /// <param name="routableRequestProvider">The routable request provider.</param>
        internal static void ConfigureFrontEndRoutes(RouteCollection routes, IRouteHandler routeHandler,
                                                     IRoutableRequestProvider routableRequestProvider)
        {
            // Ignore standard stuff...
            System.Web.Mvc.RouteCollectionExtensions.IgnoreRoute(routes, "{resource}.axd/{*pathInfo}");
            System.Web.Mvc.RouteCollectionExtensions.IgnoreRoute(routes, "{*allaxd}",
                                                                 new {allaxd = @".*\.axd(/.*|\?.*)?"});
            System.Web.Mvc.RouteCollectionExtensions.IgnoreRoute(routes, "{*favicon}",
                                                                 new {favicon = @"(.*/)?favicon.ico(/.*)?"});

            System.Web.Mvc.RouteCollectionExtensions.MapRoute(
                //name
                routes, "Umbraco",
                //url to match (match all requests)
                "{*allpages}",
                //default options
                new {controller = "Umbraco", action = "Index"},
                //constraints
                new {umbPages = new UmbracoRouteConstraint(routableRequestProvider)})
                //set the route handler
                .RouteHandler = routeHandler;
        }
    }
}