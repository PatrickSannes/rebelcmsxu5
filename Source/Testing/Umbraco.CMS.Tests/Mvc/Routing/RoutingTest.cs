using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using Umbraco.Cms.Web.Dashboards;
using Umbraco.Cms.Web.Dashboards.Filters;
using Umbraco.Cms.Web.Dashboards.Rules;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Routing;
using Umbraco.Cms.Web.System;
using Umbraco.Cms.Web.System.Boot;
using Umbraco.Cms.Web.UI;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Security;
using Umbraco.Framework.Testing;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Mvc.Areas;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Surface;
using Umbraco.Cms.Web.Trees;
using System.IO;
using System.Web.Routing;
using System.Web.Mvc;

using Umbraco.Framework;
using Umbraco.Framework.Tasks;
using Umbraco.Tests.Cms.Stubs.Surface;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;
using ApplicationTreeController = Umbraco.Tests.Cms.Stubs.Trees.ApplicationTreeController;

namespace Umbraco.Tests.Cms.Mvc.Routing
{
    using Umbraco.Cms.Web;

    [TestFixture]
    public abstract class RoutingTest : StandardWebTest
    {

        protected ComponentRegistrations Components { get; private set; }

        #region Initialize

        /// <summary>
        /// Initialize test
        /// </summary>
        [SetUp]
        public void InitTest()
        {

            Init();


            RenderModelFactory = FakeRenderModelFactory.CreateWithApp();
            var frontEndRouteHandler = new RenderRouteHandler(new TestControllerFactory(), UmbracoApplicationContext, RenderModelFactory);
            
            //register areas/routes...            

            RouteTable.Routes.Clear();

            var packageFolder = new DirectoryInfo(Path.Combine(Common.CurrentAssemblyDirectory, "App_Plugins", PluginManager.PackagesFolderName, "TestPackage"));

            Components = new ComponentRegistrations(new List<Lazy<MenuItem, MenuItemMetadata>>(),
                                                        GetEditorMetadata(packageFolder), 
                                                        GetTreeMetadata(packageFolder),
                                                        GetSurfaceMetadata(packageFolder),
                                                        new List<Lazy<AbstractTask, TaskMetadata>>(),
                                                        new List<Lazy<PropertyEditor, PropertyEditorMetadata>>(),
                                                        new List<Lazy<AbstractParameterEditor, ParameterEditorMetadata>>(),
                                                        new List<Lazy<DashboardMatchRule, DashboardRuleMetadata>>(),
                                                        new List<Lazy<DashboardFilter, DashboardRuleMetadata>>(),
                                                        new List<Lazy<Permission, PermissionMetadata>>(),
                                                        new List<Lazy<AbstractMacroEngine, MacroEngineMetadata>>());

            var componentRegistration = new PackageAreaRegistration(packageFolder, UmbracoApplicationContext, Components);
            var areaRegistration = new UmbracoAreaRegistration(UmbracoApplicationContext, Components);
            var installRegistration = new InstallAreaRegistration(UmbracoApplicationContext.Settings);
            
            var cmsBootstrapper = new CmsBootstrapper(UmbracoApplicationContext.Settings, areaRegistration, installRegistration, new[] {componentRegistration}, new DefaultAttributeTypeRegistry());
            var renderBootstrapper = new RenderBootstrapper(UmbracoApplicationContext, frontEndRouteHandler, RenderModelFactory);

            //bootstrappers setup the routes
            cmsBootstrapper.Boot(RouteTable.Routes);
            renderBootstrapper.Boot(RouteTable.Routes);

            new UmbracoWebApplication(null, null, null).RegisterRoutes(RouteTable.Routes);
        }

        #endregion

        protected FakeRenderModelFactory RenderModelFactory { get; private set; }

        private static IEnumerable<Lazy<SurfaceController, SurfaceMetadata>> GetSurfaceMetadata(DirectoryInfo packageFolder)
        {
            var surfaceTypes = new List<Type>
                                  {
                                      //standard editors
                                      typeof(CustomSurfaceController),
                                      typeof(BlahSurfaceController)                                     
                                  };
            //now we need to create the meta data for each
            return (from t in surfaceTypes
                    let a = t.GetCustomAttributes(typeof(SurfaceAttribute), false).Cast<SurfaceAttribute>().First()
                    select new Lazy<SurfaceController, SurfaceMetadata>(
                        new SurfaceMetadata(new Dictionary<string, object>())
                        {
                            Id = a.Id,
                            ComponentType = t,                                                       
                            ControllerName = UmbracoController.GetControllerName(t),
                            PluginDefinition = new PluginDefinition(
                                new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")), 
                                packageFolder.FullName,
                                null,false)
                        })).ToList();
        }

        private static IEnumerable<Lazy<AbstractEditorController, EditorMetadata>> GetEditorMetadata(DirectoryInfo packageFolder)
        {
            var editorTypes = new List<Type>
                                  {
                                      //standard editors
                                      typeof(ContentEditorController),
                                      typeof(DataTypeEditorController),
                                      typeof(DocumentTypeEditorController),
                                      //duplicate named editors
                                      typeof(Stubs.Editors.ContentEditorController),
                                      typeof(Stubs.Editors.MediaEditorController)
                                  };
            //now we need to create the meta data for each
            return (from t in editorTypes
                    let a = t.GetCustomAttributes(typeof (EditorAttribute), false).Cast<EditorAttribute>().First()
                    let defaultEditor = t.GetCustomAttributes(typeof (UmbracoEditorAttribute), false).Any()
                    select new Lazy<AbstractEditorController, EditorMetadata>(
                        new EditorMetadata(new Dictionary<string, object>())
                            {
                                Id = a.Id,
                                ComponentType = t,
                                IsInternalUmbracoEditor = defaultEditor,
                                ControllerName = UmbracoController.GetControllerName(t),
                                PluginDefinition = new PluginDefinition(
                                    new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")),
                                    packageFolder.FullName, 
                                    null, false)
                            })).ToList();
        }

        private static IEnumerable<Lazy<TreeController, TreeMetadata>> GetTreeMetadata(DirectoryInfo packageFolder)
        {
            //create the list of trees
            var treeControllerTypes = new List<Type>
                                          {
                                              //standard trees
                                              typeof (ContentTreeController),
                                              typeof (MediaTreeController),
                                              typeof (DataTypeTreeController),
                                              typeof (DocumentTypeTreeController),
                                              //duplicate named controllers
                                              typeof (ApplicationTreeController),
                                              typeof (Stubs.Trees.ContentTreeController),
                                              typeof (Stubs.Trees.MediaTreeController)
                                          };
            //now we need to create the meta data for each
            return (from t in treeControllerTypes
                    let a = t.GetCustomAttributes(typeof (TreeAttribute), false).Cast<TreeAttribute>().First()
                    let defaultTree = t.GetCustomAttributes(typeof (UmbracoTreeAttribute), false).Any()
                    select new Lazy<TreeController, TreeMetadata>(
                        new TreeMetadata(new Dictionary<string, object>())
                            {
                                Id = a.Id,
                                TreeTitle = a.TreeTitle,
                                ComponentType = t,
                                IsInternalUmbracoTree = defaultTree,
                                ControllerName = UmbracoController.GetControllerName(t),
                                PluginDefinition = new PluginDefinition(
                                    new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")),
                                    packageFolder.FullName, 
                                    null, false)
                            })).ToList();
        }



    }
}
