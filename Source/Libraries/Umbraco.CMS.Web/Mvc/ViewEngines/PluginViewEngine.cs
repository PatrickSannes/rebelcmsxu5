using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Mvc.Metadata;
using System.Web.Hosting;
using System.Web;
using Umbraco.Cms.Web.Packaging;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{


    /// <summary>
    /// A ViewEngine to support non-embedded views for packaged plugins for both editors, property editors and dashboards
    /// </summary>
    /// <remarks>
    /// 
    /// Views will be found in the folders:
    /// 
    /// ~/App_Plugins/Packages/[PackageName]/Editors/[EditorId]/Views/{0}
    /// ~/App_Plugins/Packages/Core/Editors/[EditorId]/Views/{0}
    /// 
    /// ~/App_Plugins/Packages/[PackageName]/PropertyEditors/[EditorId]/Views/{0}
    /// ~/App_Plugins/Packages/Core/PropertyEditors/[EditorId]/Views/{0}
    /// 
    /// ~/App_Plugins/Packages/[PackageName]/Dashboards/{0}
    /// ~/App_Plugins/Packages/Core/Dashboards/{0}
    /// 
    /// Front-end partial views & child action views can be found in these folders:
    /// 
    /// ~/App_Plugins/Packages/[PackageName]/Views/Partial/{0}
    /// 
    /// </remarks>
    public class PluginViewEngine : RazorViewEngine
    {
        private readonly UmbracoSettings _settings;
        private readonly IPackageContext _packageContext;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="packageContext"></param>
        public PluginViewEngine(UmbracoSettings settings, IPackageContext packageContext)
        {
            _settings = settings;
            _packageContext = packageContext;

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
            
            
            SetViewLocations();
        }

        private void SetViewLocations()
        {
            //these are the originals:

            //base.AreaViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
            //base.AreaMasterLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
            //base.AreaPartialViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
            //base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
            //base.MasterLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
            //base.PartialViewLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
            //base.FileExtensions = new string[] { "cshtml", "vbhtml" };

            var pluginsPath = _settings.PluginConfig.PluginsPath;

            var viewLocationsArray = new[]
                {
                    string.Concat(pluginsPath, "/Packages/{2}/Views/Editors/{1}/{0}.cshtml"),
                    string.Concat(pluginsPath, "/Packages/{2}/Views/Editors/{1}/{0}.vbhtml"),
                    
                    //add the core folders as well (core only uses cshtml)
                    string.Concat(pluginsPath, "/Core/Views/Editors/{1}/{0}.cshtml"),
                };

            //set all of the area view locations to the plugin folder
            AreaViewLocationFormats = viewLocationsArray;
            AreaMasterLocationFormats = viewLocationsArray;

            var packageFolders = _packageContext.LocalPackageManager.FileSystem.GetDirectories(
                _packageContext.LocalPackageManager.LocalRepository.Source)
                .ToArray();

            
            AreaPartialViewLocationFormats = new[]
                {
                    //Used for ChildView macros - when a ChildView macro is rendered with a PartialViewResult, it is proxied to execute under it's packages Area.
                    //When a PartialView macro is rendering with an 'area'(package) prefix it is proxied to execute under it's packages area.
                    string.Concat(pluginsPath, "/Packages/{2}/Views/Partial/{0}.cshtml"),
                    string.Concat(pluginsPath, "/Packages/{2}/Views/Partial/{0}.vbhtml"),                    
                    string.Concat(pluginsPath, "/Packages/{2}/Views/MacroPartials/{0}.cshtml"),
                    string.Concat(pluginsPath, "/Packages/{2}/Views/MacroPartials/{0}.vbhtml"),                    
                    //for editor partials
                    string.Concat(pluginsPath, "/Packages/{2}/Views/Editors/Shared/{0}.cshtml"),
                    string.Concat(pluginsPath, "/Packages/{2}/Views/Editors/Shared/{0}.vbhtml"),
                    //for property editor views that are not embedded
                    string.Concat(pluginsPath, "/Packages/{2}/Views/PropertyEditors/{0}.cshtml"),
                    string.Concat(pluginsPath, "/Packages/{2}/Views/PropertyEditors/{0}.vbhtml"),
                    //add the core folders as well (core only uses cshtml)
                    string.Concat(pluginsPath, "/Core/Views/PropertyEditors/{0}.cshtml"),
                    string.Concat(pluginsPath, "/Core/Views/Dashboards/{0}.cshtml"),
                    //this is the elephant in the room... some plugins or devs may put their dashboards in the main dashboard folder
                    "~/Umbraco/Views/Dashboards/{0}.cshtml",
                    "~/Umbraco/Views/Dashboards/{0}.vbhtml"
                }
                //This will add paths for dashboard views for each package that is installed, this is required without using the 
                //area placeholder (i.e. {3}) because not all packages are registered as areas since not all packages container Controllers to route.
                .Concat(packageFolders
                            .SelectMany(x =>
                                        new[]
                                            {
                                                string.Concat(pluginsPath, "/Packages/", x, "/Views/Dashboards/{0}.cshtml"),
                                                string.Concat(pluginsPath, "/Packages/", x, "/Dashboards/{0}.vbhtml")
                                            })
                ).ToArray();


            AreaViewLocationFormats = viewLocationsArray
                .Concat(new[]
                    {
                        string.Concat(pluginsPath, "/Packages/{2}/Views/Editors/Shared/{0}.cshtml"),
                        string.Concat(pluginsPath, "/Packages/{2}/Views/Editors/Shared/{0}.vbhtml")                        
                    })
                .ToArray();

        }

        /// <summary>
        /// If we're not rendering in an umbraco context this will exit
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="partialViewName"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        /// <remarks>
        /// NOTE: If someone has a lot of packages installed, the number of folders being searched will be fairly insane
        ///  it would be good if VirtualPathProviderViewEngine exposed its members for path finding but they are all marked as 
        ///  private, so we could either copy all of its code in here or find another nice way to avoid having to lookup so many folders.
        ///  We could aid in this performance by injecting different data tokens for different types of views (i.e. Dashboards, etc...)
        ///  then we can actually have a differerent ViewEngines for those specific ones by checking for the correct token.
        /// </remarks>
        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (!ShouldFindView(controllerContext, true))
            {
                return new ViewEngineResult(new string[] { });
            }

            return base.FindPartialView(controllerContext, partialViewName, useCache);
        }

        /// <summary>
        /// If we're not rendering in an umbraco context, this will exit, otherwise will find the view based
        /// on the base class implementation.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="viewName"></param>
        /// <param name="masterName"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            if (!ShouldFindView(controllerContext, false))
            {
                return new ViewEngineResult(new string[] { });
            }

            return base.FindView(controllerContext, viewName, masterName, useCache);
        }      

        /// <summary>
        /// Determines if a view should be found, this is to improve performance of view lookups but also to 
        /// decrease the amount of overlap with other view engines.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="isPartial"></param>
        /// <returns></returns>
        private bool ShouldFindView(ControllerContext controllerContext, bool isPartial)
        {

            //if we're rendering a partial view in the umbraco front-end or back-end 
            // then allow so that partial macros/dashboards, etc.. stored in a plugin folder work.
            // NOTE: this is kind of ugly since if its the Umbraco area then we should just return false but because of the current way that dashboard views
            //  are found, we need to return true. If we can get dashboard views proxied to their area if the dashboard is found in an area this would be better.
            // (both the front end and back office will have an 'umbraco' key in the data tokens
            //  though they will have different values)
            if ((isPartial || controllerContext.IsChildAction)
                && controllerContext.RouteData.DataTokens.ContainsKey("umbraco"))
            {
                return true;
            }

            //only find views if we're rendering the umbraco back office
            if (controllerContext.RouteData.DataTokens.ContainsKey("umbraco")
                && controllerContext.RouteData.DataTokens["umbraco"].ToString().InvariantEquals("backoffice"))
            {
                //don't find views if we're simply rendering the 'Umbraco' area as that is not a plugin and nobody
                //should be naming their plugin folder 'Umbraco' !
                if (controllerContext.RouteData.DataTokens.ContainsKey("area")
                    && controllerContext.RouteData.DataTokens["area"].ToString().InvariantEquals(_settings.UmbracoPaths.BackOfficePath))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

    }
}
