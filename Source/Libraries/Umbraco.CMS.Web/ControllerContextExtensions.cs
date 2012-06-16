using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Umbraco.Framework;

namespace Umbraco.Cms.Web
{
    /// <summary>
    /// Extension methods for MVC ControllerContext objects
    /// </summary>
    public static class ControllerContextExtensions
    {

        /// <summary>
        /// This is generally used for proxying to a ChildAction which requires a ViewContext to be setup
        /// but since the View isn't actually rendered the IView object is null, however the rest of the 
        /// properties are filled in.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        public static ViewContext CreateEmptyViewContext(this ControllerContext controllerContext)
        {
            return new ViewContext
                {
                    Controller = controllerContext.Controller,
                    HttpContext = controllerContext.HttpContext,
                    RequestContext = controllerContext.RequestContext,
                    RouteData = controllerContext.RouteData,
                    TempData = controllerContext.Controller.TempData,
                    ViewData = controllerContext.Controller.ViewData
                };
        }

        /// <summary>
        /// Returns the string output from a ViewResultBase object
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="viewResult"></param>
        /// <returns></returns>
        public static string RenderViewResultAsString(this ControllerContext controllerContext, ViewResultBase viewResult)
        {
            using (var sw = new StringWriter())
            {
                controllerContext.EnsureViewObjectDataOnResult(viewResult);

                var viewContext = new ViewContext(controllerContext, viewResult.View, viewResult.ViewData, viewResult.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                foreach(var v in viewResult.ViewEngineCollection)
                {
                    v.ReleaseView(controllerContext, viewResult.View);
                }                
                return sw.ToString().Trim();
            }
        }

        /// <summary>
        /// Renders the partial view to string.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static string RenderPartialViewToString(this ControllerContext controllerContext, string viewName, object model)
        {

            controllerContext.Controller.ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                ViewContext viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Normally in MVC the way that the View object gets assigned to the result is to Execute the ViewResult, this however
        /// will write to the Response output stream which isn't what we want. Instead, this method will use the same logic inside
        /// of MVC to assign the View object to the result but without executing it. This also ensures that the ViewData and the TempData
        /// is assigned from the controller.
        /// This is only relavent for view results of PartialViewResult or ViewResult.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="controllerContext"></param>
        internal static void EnsureViewObjectDataOnResult(this ControllerContext controllerContext, ViewResultBase result)
        {
            result.ViewData.ModelState.Merge(controllerContext.Controller.ViewData.ModelState);

            // Temporarily copy the dictionary to avoid enumerator-modification errors
            var newViewDataDict = new ViewDataDictionary(controllerContext.Controller.ViewData);
            foreach (var d in newViewDataDict)
                result.ViewData[d.Key] = d.Value;    

            result.TempData = controllerContext.Controller.TempData;

            if (result.View != null) return;

            if (string.IsNullOrEmpty(result.ViewName))
                result.ViewName = controllerContext.RouteData.GetRequiredString("action");

            if (result.View != null) return;

            if (result is PartialViewResult)
            {
                var viewEngineResult = ViewEngines.Engines.FindPartialView(controllerContext, result.ViewName);

                if (viewEngineResult.View == null)
                {
                    throw new InvalidOperationException("Could not find the view " + result.ViewName + ", the following locations were searched: " + Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
                }
                
                result.View = viewEngineResult.View;
            }
            else if (result is ViewResult)
            {
                var vr = (ViewResult) result;
                var viewEngineResult = ViewEngines.Engines.FindView(controllerContext, vr.ViewName, vr.MasterName);

                if (viewEngineResult.View == null)
                {
                    throw new InvalidOperationException("Could not find the view " + vr.ViewName + ", the following locations were searched: " + Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
                }

                result.View = viewEngineResult.View;
            }
        }
    }
}