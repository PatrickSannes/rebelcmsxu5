using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Mvc.Areas;
using Umbraco.Cms.Web.Mvc.ControllerFactories;
using Umbraco.Cms.Web.Mvc.Metadata;
using Umbraco.Cms.Web.Mvc.ViewEngines;
using Umbraco.Cms.Web.Routing;
using Umbraco.Cms.Web.Surface;
using Umbraco.Cms.Web.System;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;

namespace Umbraco.Cms.Web.DependencyManagement.DemandBuilders
{
    /// <summary>
    /// registers the MVC types into the container
    /// </summary>
    public class MvcTypesDemandBuilder : IDependencyDemandBuilder
    {
        private readonly TypeFinder _typeFinder;

        public MvcTypesDemandBuilder(TypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            //register model binder provider
            containerBuilder.RegisterModelBinderProvider();

            // Register model binders & controllers
            var allReferencedPluginAssemblies = PluginManager.ReferencedPlugins.Select(x => x.ReferencedAssembly).ToArray();
            containerBuilder
                .RegisterModelBinders(allReferencedPluginAssemblies, _typeFinder)
                .RegisterModelBinders(TypeFinder.GetFilteredBinFolderAssemblies(allReferencedPluginAssemblies), _typeFinder)
                .RegisterControllers(allReferencedPluginAssemblies, _typeFinder)
                .RegisterControllers(TypeFinder.GetFilteredBinFolderAssemblies(allReferencedPluginAssemblies), _typeFinder);

            //register view engines
            containerBuilder.For<EmbeddedRazorViewEngine>().KnownAs<IViewEngine>();
            containerBuilder.For<PluginViewEngine>().KnownAs<IViewEngine>();
            containerBuilder.For<RenderViewEngine>().KnownAs<IViewEngine>();
            containerBuilder.For<AlternateLocationViewEngine>().KnownAs<IViewEngine>();

            //register the route handlers
            containerBuilder.For<RenderRouteHandler>()
                .Named<IRouteHandler>(RenderRouteHandler.SingletonServiceName)
                .ScopedAs.NewInstanceEachTime();

            // register our master controller factory
            containerBuilder.For<MasterControllerFactory>().KnownAs<IControllerFactory>().ScopedAs.Singleton();
            containerBuilder.For<RenderControllerFactory>().KnownAs<IFilteredControllerFactory>().ScopedAs.Singleton();

            //register our umbraco area, ensure that the TreeRouteHandler named service is injected for 
            //the constructor argument expecting a IRouteHandler
            containerBuilder.For<UmbracoAreaRegistration>().KnownAsSelf();
            containerBuilder.For<InstallAreaRegistration>().KnownAsSelf();

            //register master view page activator
            containerBuilder.For<MasterViewPageActivator>().KnownAs<IViewPageActivator>().ScopedAs.Singleton();
            //register our IPostViewPageActivators
            containerBuilder.For<UmbracoContextViewPageActivator>().KnownAs<IPostViewPageActivator>().ScopedAs.Singleton();
            containerBuilder.For<UmbracoHelperViewPageActivator>().KnownAs<IPostViewPageActivator>().ScopedAs.Singleton();
            containerBuilder.For<RenderViewPageActivator>().KnownAs<IPostViewPageActivator>().ScopedAs.Singleton();

            //register model meta data provider
            containerBuilder.For<UmbracoModelMetadataProvider>().KnownAs<ModelMetadataProvider>();

            ////register the default controller factory
            ////(We can't put this in the container based on the DependencyResolver because that will cause
            ////an infinite loop. Also, trying to use the Singly registered 'old school' way to get the
            ////ControllerFactory will cause AutoFac to call the DependencyResolver anyways)
            //containerBuilder.For<DefaultControllerFactory>().KnownAs<IControllerFactory>();
        }
    }
}
