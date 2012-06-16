using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// A ViewEngine that allows a controller's views to be shared with other 
    /// controllers without having to put these shared views in the 'Shared' folder.
    /// This is useful for when you have inherited controllers.
    /// This will allow of 'overriding' of views, for example if i have a controller called 'Media' that has an alternate view location of 'Content', the 'Content' folder could contain all of the
    /// partial views necessary, but we could have a different main view in the 'Media' folder which would be used and then all of the 'Content' partial views could be used.
    /// </summary>
    public class AlternateLocationViewEngine : RazorViewEngine
    {
        public AlternateLocationViewEngine()
        {
            // TODO: Resolve TwoLevelViewCache problems in release builds, as it seems to cache views without taking parent folder into account
            ViewLocationCache = DefaultViewLocationCache.Null;
            //if (HttpContext.Current == null || HttpContext.Current.IsDebuggingEnabled)
            //{
            //    ViewLocationCache = DefaultViewLocationCache.Null;
            //}
            //else
            //{
            //    //override the view location cache with our 2 level view location cache
            //    ViewLocationCache = new TwoLevelViewCache(ViewLocationCache);
            //}
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            //check if a view exist with the 'real' controller
            var view = base.FindPartialView(controllerContext, partialViewName, useCache);
            if (view.View != null)
                return view;

            //see if we can get the view with the alternate controller specified, if its found return the result
            var altContext = GetAlternateControllerContext(controllerContext);
            if (altContext != null)
            {                
                var result = base.FindPartialView(altContext, partialViewName,useCache);
                if (result.View != null)
                {
                    return result;
                }
            }

            //return the original searched view
            return view;
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            //check if a view exist with the 'real' controller
            var view = base.FindView(controllerContext, viewName, masterName, useCache);
            if (view.View != null)
                return view;

            //see if we can get the view with the alternate controller specified, if its found return the result
            var altContext = GetAlternateControllerContext(controllerContext);
            if (altContext!= null)
            {
                var result = base.FindView(altContext, viewName, masterName, useCache);
                if (result.View != null)
                {
                    return result;
                }
            }

            //return the original searched view
            return view;
        }

        /// <summary>
        /// Returns a new controller context with the alternate controller name in the route values
        /// if the current controller is found to contain an AlternateViewEnginePathAttribute.
        /// </summary>
        /// <param name="currentContext"></param>
        /// <returns></returns>
        private static ControllerContext GetAlternateControllerContext(ControllerContext currentContext)
        {
            IController controller;
            var altControllerAttribute = new List<AlternateViewEnginePathAttribute>();

            Func<IController, List<AlternateViewEnginePathAttribute>> getAttributes =
                c => c.GetType()
                         .GetCustomAttributes(typeof (AlternateViewEnginePathAttribute), false)
                         .OfType<AlternateViewEnginePathAttribute>()
                         .ToList();

            //check if this is an extender controller
            if (currentContext.RouteData.IsControllerExtender())
            {
                //try to get the alternate view path from the parent controller
                controller = currentContext.RouteData.GetControllerExtenderParentContext().Controller;
                altControllerAttribute = getAttributes(controller);
            }

            //if none are found check the current controller
            if (!altControllerAttribute.Any())
            {
                controller = currentContext.Controller;
                altControllerAttribute = getAttributes(controller);    
            }

            if (altControllerAttribute.Any())
            {
                var altController = altControllerAttribute.Single().AlternateControllerName;
                var altArea = altControllerAttribute.Single().AlternateAreaName;

                var newRouteData = new RouteData
                    {
                        Route = currentContext.RouteData.Route,
                        RouteHandler = currentContext.RouteData.RouteHandler
                    };
                currentContext.RouteData.DataTokens.ForEach(x => newRouteData.DataTokens.Add(x.Key, x.Value));
                currentContext.RouteData.Values.ForEach(x => newRouteData.Values.Add(x.Key, x.Value));
                //now, update the new route data with the new alternate controller name
                newRouteData.Values["controller"] = altController;

                // if we have an area specified, make sure the datatoken is updated
                if (!string.IsNullOrWhiteSpace(altArea)) {
                    if (newRouteData.DataTokens.ContainsKey("area")) {
                        newRouteData.DataTokens["area"] = altArea;
                    }
                    else {
                        newRouteData.DataTokens.Add("area", altArea);
                    }
                }

                //now create a new controller context to pass to the view engine
                var newContext = new ControllerContext(currentContext.HttpContext, newRouteData, currentContext.Controller);
                return newContext;
            }

            return null;
        }
    }
}