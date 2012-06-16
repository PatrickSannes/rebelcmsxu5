using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;
using Umbraco.Cms.Web.Routing;
using Umbraco.Cms.Web.Surface;
using Umbraco.Cms.Web.System;
using Umbraco.Cms.Web.Trees;
using System.Web.Routing;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.Areas
{

    /// <summary>
    /// An AreaRegistration used to register packaged plugins requiring their own custom routes such as Trees and Editors
    /// </summary>
    public class PackageAreaRegistration : AreaRegistration
    {
        private readonly string _packageName;
        private readonly DirectoryInfo _packageFolder;
        private readonly IUmbracoApplicationContext _applicationContext;
        private readonly ComponentRegistrations _componentRegistrar;
        private readonly IEnumerable<Lazy<TreeController, TreeMetadata>> _treeControllers;
        private readonly IEnumerable<Lazy<AbstractEditorController, EditorMetadata>> _editorControllers;
        private readonly IEnumerable<Lazy<SurfaceController, SurfaceMetadata>> _surfaceControllers;

        public PackageAreaRegistration(DirectoryInfo packageFolder,
            IUmbracoApplicationContext applicationContext,
            ComponentRegistrations componentRegistrar)
        {
            //TODO: do we need to validate the package name??

            _packageName = packageFolder.Name;

            _packageFolder = packageFolder;
            _applicationContext = applicationContext;
            _componentRegistrar = componentRegistrar;
            _treeControllers = _componentRegistrar.TreeControllers;
            _editorControllers = _componentRegistrar.EditorControllers;
            _surfaceControllers = componentRegistrar.SurfaceControllers;
        }

        /// <summary>
        /// The area name will be a sub folder of the back office path such as:
        /// Umbraco/MyPackage
        /// </summary>
        public override string AreaName
        {
            get { return _packageName; }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            var editorControllersForPackage = _editorControllers
                .Select(x => x.Metadata)
                .Where(x => !x.IsInternalUmbracoEditor)
                .Where(x => x.PluginDefinition != null)
                .Where(x => x.PluginDefinition.PackageFolderPath == _packageFolder.FullName)
                .ToArray();
                             
            MapRouteEditors(context.Routes, editorControllersForPackage);

            var treeControllersForPackage = _treeControllers
                .Select(x => x.Metadata)
                .Where(x => !x.IsInternalUmbracoTree)
                .Where(x => x.PluginDefinition != null)
                .Where(x => x.PluginDefinition.PackageFolderPath == _packageFolder.FullName)
                .ToArray();
                
            MapRouteTrees(context.Routes, treeControllersForPackage);

            var surfaceControllersForPackage = _surfaceControllers
                .Select(x => x.Metadata)
                .Where(x => x.PluginDefinition != null)
                .Where(x => x.PluginDefinition.PackageFolderPath == _packageFolder.FullName)
                .ToArray();

            MapRouteSurface(context.Routes, surfaceControllersForPackage);

            //now we can map the rest of the controllers found in this area

            //first we need to add custom ignore routes to make sure the below routes don't also match the above controllers
            context.Routes.Add(new IgnorePluginRoute("{*all-" + AreaName + "}", AreaName, _applicationContext,
                                               editorControllersForPackage.Cast<ControllerPluginMetadata>()
                                                   .Concat(treeControllersForPackage)
                                                   .Concat(surfaceControllersForPackage)));

            //url to match normal controller routes for the back office
            var standardRoute = context.Routes.MapRoute(
                AreaName + "-default",
                _applicationContext.Settings.UmbracoPaths.BackOfficePath + "/" + AreaName + "/{controller}/{action}/{id}",
                //make the default controller name the name of the Area since we have no idea what it could be.
                new {controller = AreaName, action = "Index", id = UrlParameter.Optional});
            standardRoute.DataTokens.Add("area", AreaName);//only match this area
            standardRoute.DataTokens.Add("umbraco", "custom"); //ensure the umbraco token is set (used for view engine perf)
        }

        /// <summary>
        /// Register all editor controllers that have been found as plugins for the current package folder
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="editorControllers"></param>
        private void MapRouteEditors(RouteCollection routes, IEnumerable<EditorMetadata> editorControllers)
        {
            foreach (var t in editorControllers)
            {                
                this.RouteControllerPlugin(t.Id, t.ControllerName, t.ComponentType, routes, "editorId", "Editor", "", "Dashboard", UrlParameter.Optional, false, _applicationContext);    
            }
        }

        /// <summary>
        /// Register all tree controllers that have been found as plugins in the current package folder
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="treeControllers"></param>
        private void MapRouteTrees(RouteCollection routes, IEnumerable<TreeMetadata> treeControllers)
        {
            foreach (var t in treeControllers)
            {
                this.RouteControllerPlugin(t.Id, t.ControllerName, t.ComponentType, routes, "treeId", "Tree", "", "Index", HiveId.Empty, false, _applicationContext);
            }
        }

        /// <summary>
        /// Registers all surface controllers that have been found as plugins in the current package folder
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="surfaceControllers"></param>
        private void MapRouteSurface(RouteCollection routes, IEnumerable<SurfaceMetadata> surfaceControllers)
        {
            foreach (var t in surfaceControllers)
            {
                this.RouteControllerPlugin(t.Id, t.ControllerName, t.ComponentType, routes, "surfaceId", "Surface", "", "Index", UrlParameter.Optional, false, _applicationContext, "surface");
            }
        }
    }
}