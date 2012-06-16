using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;
using Umbraco.Framework.Context;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// A view engine to look into the template location specified in the config for the front-end/Rendering part of the cms,
    /// this includes paths to render partial macros and media item templates.
    /// </summary>
    public class RenderViewEngine : RazorViewEngine
    {
        
        private readonly UmbracoSettings _settings;
        
        private readonly IEnumerable<string> _supplementedViewLocations = new[] { "/{0}.cshtml", "/MediaTemplates/{0}.cshtml" };
        private readonly IEnumerable<string> _supplementedPartialViewLocations = new[] { "/{0}.cshtml", "/Partial/{0}.cshtml", "/MacroPartials/{0}.cshtml" };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="renderModelFactory"></param>
        public RenderViewEngine(UmbracoSettings settings, IRenderModelFactory renderModelFactory)
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

            _settings = settings;

            var replaceWithUmbracoFolder = _supplementedViewLocations.ForEach(location => _settings.UmbracoFolders.TemplateFolder + location);
            var replacePartialWithUmbracoFolder = _supplementedPartialViewLocations.ForEach(location => _settings.UmbracoFolders.TemplateFolder + location);

            //The Render view engine doesn't support Area's so make those blank
            ViewLocationFormats = replaceWithUmbracoFolder.ToArray();
            PartialViewLocationFormats = replacePartialWithUmbracoFolder.ToArray();

            AreaPartialViewLocationFormats = new string[] {};
            AreaViewLocationFormats = new string[] {};
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            if (!ShouldFindView(controllerContext, false))
            {
                return new ViewEngineResult(new string[] { });
            }

            return base.FindView(controllerContext, viewName, masterName, useCache);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (!ShouldFindView(controllerContext, true))
            {
                return new ViewEngineResult(new string[] { });
            }

            return base.FindPartialView(controllerContext, partialViewName, useCache);
        }

        /// <summary>
        /// Determines if the view should be found, this is used for view lookup performance and also to ensure 
        /// less overlap with other user's view engines. This will return true if the Umbraco back office is rendering
        /// and its a partial view or if the umbraco front-end is rendering but nothing else.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="isPartial"></param>
        /// <returns></returns>
        private bool ShouldFindView(ControllerContext controllerContext, bool isPartial)
        {
            //first check if we're rendering a partial view for the back office
            if (isPartial
                && controllerContext.RouteData.DataTokens.ContainsKey("umbraco")
                && controllerContext.RouteData.DataTokens["umbraco"].ToString().InvariantEquals("backoffice"))
            {
                return true;
            }

            //only find views if we're rendering the umbraco front end
            if (controllerContext.RouteData.DataTokens.ContainsKey("umbraco")
                && controllerContext.RouteData.DataTokens["umbraco"] != null
                && controllerContext.RouteData.DataTokens["umbraco"] is IUmbracoRenderModel)
            {
                return true;
            }

            return false;
        }

    }
}
