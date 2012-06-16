using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// Custom route handler to assign the correct controller based on the Umbraco request
    /// </summary>
    public class RenderRouteHandler : IRouteHandler
    {
        internal const string SingletonServiceName = "RenderRouteHandler";

        public RenderRouteHandler(IControllerFactory controllerFactory, 
            IUmbracoApplicationContext applicationContext, 
            IRenderModelFactory modelFactory)
        {
            _modelFactory = modelFactory;
            _applicationContext = applicationContext;
            _controllerFactory = controllerFactory;
        }

        private readonly IControllerFactory _controllerFactory;
        private readonly IUmbracoApplicationContext _applicationContext;
        private readonly IRenderModelFactory _modelFactory;

        #region IRouteHandler Members

        /// <summary>
        /// Assigns the correct controller based on the Umbraco request and returns a standard MvcHandler to prcess the response,
        /// this also stores the render model into the data tokens for the current RouteData.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            // Generate a render model
            var renderModel = _modelFactory.Create(requestContext.HttpContext, requestContext.HttpContext.Request.RawUrl);
            
            //put the render model into the current RouteData
            requestContext.RouteData.DataTokens.Add("umbraco", renderModel);

            return GetHandlerForRoute(requestContext, renderModel);            
        }

        #endregion

        /// <summary>
        /// Checks the request and query strings to see if it matches the definition of having a Surface controller
        /// posted value, if so, then we return a PostedDataProxyInfo object with the correct information.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        private static PostedDataProxyInfo GetPostedFormInfo(RequestContext requestContext)
        {
            if (requestContext.HttpContext.Request.RequestType != "POST")
                return null;

            //this field will contain a base64 encoded version of the surface route vals 
            if (requestContext.HttpContext.Request["uformpostroutevals"].IsNullOrWhiteSpace())
                return null;

            var encodedVal = requestContext.HttpContext.Request["uformpostroutevals"];
            var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(encodedVal));
            //the value is formatted as query strings
            var decodedParts = decodedString.Split('&').Select(x => new {Key = x.Split('=')[0], Value = x.Split('=')[1]}).ToArray();
            
            //validate all required keys exist
            
            //the controller
            if (!decodedParts.Any(x => x.Key == "c"))
                return null;
            //the action
            if (!decodedParts.Any(x => x.Key == "a"))
                return null;
            //the area
            if (!decodedParts.Any(x => x.Key == "ar"))
                return null;

            //the surface id
            if (decodedParts.Any(x => x.Key == "i"))
            {
                Guid id;
                if (Guid.TryParse(decodedParts.Single(x => x.Key == "i").Value, out id))
                {
                    return new PostedDataProxyInfo
                    {
                        ControllerName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "c").Value),
                        ActionName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "a").Value),
                        Area = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "ar").Value),
                        SurfaceId = id,
                    };
                }                    
            }

            //return the proxy info without the surface id... could be a local controller.
            return new PostedDataProxyInfo
            {
                ControllerName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "c").Value),
                ActionName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "a").Value),
                Area = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "ar").Value),
            };
            
        }
        
        /// <summary>
        /// Handles a posted form to an Umbraco Url and ensures the correct controller is routed to and that
        /// the right DataTokens are set.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="postedInfo"></param>
        /// <param name="renderModel"></param>
        /// <param name="routeDefinition">The original route definition that would normally be used to route if it were not a POST</param>
        protected virtual IHttpHandler HandlePostedValues(RequestContext requestContext, PostedDataProxyInfo postedInfo, IUmbracoRenderModel renderModel, RouteDefinition routeDefinition)
        {
         
            //set the standard route values/tokens
            requestContext.RouteData.Values["controller"] = postedInfo.ControllerName;
            requestContext.RouteData.Values["action"] = postedInfo.ActionName;            
            requestContext.RouteData.DataTokens["area"] = postedInfo.Area;

            IHttpHandler handler = new MvcHandler(requestContext);

            //ensure the surface id is set if found, meaning it is a plugin, not locally declared
            if (postedInfo.SurfaceId != default(Guid))
            {
                requestContext.RouteData.Values["surfaceId"] = postedInfo.SurfaceId.ToString("N");
                //find the other data tokens for this route and merge... things like Namespace will be included here
                using (RouteTable.Routes.GetReadLock())
                {
                    var surfaceRoute = RouteTable.Routes.OfType<Route>()
                        .Where(x => x.Defaults != null && x.Defaults.ContainsKey("surfaceId") &&
                            x.Defaults["surfaceId"].ToString() == postedInfo.SurfaceId.ToString("N"))
                        .SingleOrDefault();
                    if (surfaceRoute == null)
                        throw new InvalidOperationException("Could not find a Surface controller route in the RouteTable for id " + postedInfo.SurfaceId);
                    //set the 'Namespaces' token so the controller factory knows where to look to construct it
                    if (surfaceRoute.DataTokens.ContainsKey("Namespaces"))
                    {
                        requestContext.RouteData.DataTokens["Namespaces"] = surfaceRoute.DataTokens["Namespaces"];    
                    }
                    handler = surfaceRoute.RouteHandler.GetHttpHandler(requestContext);
                }
                
            }

            //store the original URL this came in on
            requestContext.RouteData.DataTokens["umbraco-item-url"] = requestContext.HttpContext.Request.Url.AbsolutePath;
            //store the original route definition
            requestContext.RouteData.DataTokens["umbraco-route-def"] = routeDefinition;

            return handler;
        }

        /// <summary>
        /// Returns a RouteDefinition object based on the current renderModel
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="renderModel"></param>
        /// <returns></returns>
        protected virtual RouteDefinition GetUmbracoRouteDefinition(RequestContext requestContext, IUmbracoRenderModel renderModel)
        {
            var def = new RouteDefinition
                {
                    ControllerName = UmbracoController.GetControllerName<UmbracoController>(), 
                    Controller = new UmbracoController(),
                    RenderModel = renderModel,
                    ActionName = ((Route)requestContext.RouteData.Route).Defaults["action"].ToString()
                };

            //check that a template is defined)
            if (renderModel.CurrentNode != null && renderModel.CurrentNode.CurrentTemplate != null)
            {
                using (var uow = _applicationContext.Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
                {
                    var template = uow.Repositories.Get<File>(renderModel.CurrentNode.CurrentTemplate.Id);
                                        
                    //TODO: We need to add internal caching to the below, once we know that the controller can be 
                    // created, we shouldn't need to try to create it again,
                    // also, once we determine that the Action to route to, that should be cached too.

                    //check if there's a custom controller assigned
                    var controller = _controllerFactory.CreateController(requestContext, def.ControllerName);

                    //check if that controller exists
                    if (controller != null)
                    {                        

                        //ensure the controller is of type 'UmbracoController'
                        if (!(controller is UmbracoController))
                        {
                            LogHelper.Warn<RenderRouteHandler>("The current Document Type {0} matches a locally declared controller of type {1}. Custom Controllers for Umbraco routing must inherit from '{2}'.", renderModel.CurrentNode.ContentType.Alias, controller.GetType().FullName, typeof(UmbracoController).FullName);
                            //exit as we cannnot route to the custom controller
                            return def;
                        }
                        
                        def.ControllerName = renderModel.CurrentNode.ContentType.Alias;
                        def.Controller = (ControllerBase)controller;
                        def.ControllerName = UmbracoController.GetControllerName(controller.GetType());

                        // Template might be null if none is assigned to the node
                        if (template != null)
                        {
                            //check if the custom controller has an action with the same name as the template name (we convert ToUmbracoAlias since the template name might have invalid chars).
                            //NOTE: This also means that all custom actions MUST be PascalCase.. but that should be standard.
                            var templateName = template.Name.Split('.')[0].ToUmbracoAlias(StringAliasCaseType.PascalCase);
                            var action = controller.GetType().GetMethod(templateName);
                            if (action != null)
                            {
                                def.ActionName = templateName;
                            }
                            //otherwise it will continue to route to Index since thats teh default route
                        }
                        
                    }
                }
            }

            return def;
        }

        /// <summary>
        /// this will determine the controller and set the values in the route data
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="renderModel"></param>
        protected internal IHttpHandler GetHandlerForRoute(RequestContext requestContext, IUmbracoRenderModel renderModel)
        {
            var routeDef = GetUmbracoRouteDefinition(requestContext, renderModel);

            //Need to check for a special case if there is form data being posted back to an Umbraco URL
            var postedInfo = GetPostedFormInfo(requestContext);
            if (postedInfo != null)
            {
                return HandlePostedValues(requestContext, postedInfo, renderModel, routeDef);    
            }

            //no post values, just route to the controller/action requried (local)

            requestContext.RouteData.Values["controller"] = routeDef.ControllerName;
            if (!routeDef.ActionName.IsNullOrWhiteSpace())
            {
                requestContext.RouteData.Values["action"] = routeDef.ActionName;
            }
            return new MvcHandler(requestContext);
        }
    }
}