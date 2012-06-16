using System;
using System.IO;
using System.Reflection;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Mvc.Areas;
using Umbraco.Framework.DependencyManagement.Autofac;

namespace Umbraco.Tests.Cms.Mvc.Routing
{
    [TestFixture]
    public class RegistrationViaAbstractedIoC
    {
        [Test]
        public void UmbracoAreaRegistrationReceivesMetadataFromIoC()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var binFolder = Path.GetDirectoryName(path);
            var settingsFile = new FileInfo(Path.Combine(binFolder, "web.config"));

            var autofacBuilder = new AutofacContainerBuilder();

            autofacBuilder.ForFactory(
                context => new UmbracoSettings(settingsFile))
                .KnownAsSelf();

            autofacBuilder.For<UmbracoAreaRegistration>()
                .KnownAsSelf()
                .WithResolvedParam(context => context.Resolve<IRouteHandler>("TreeRouteHandler"));

            //why is this here? 

            var typeFinder = new TypeFinder();
            var componentRegistrar = new UmbracoComponentRegistrar();
            componentRegistrar.RegisterEditorControllers(autofacBuilder, typeFinder);
            componentRegistrar.RegisterMenuItems(autofacBuilder, typeFinder);
            //componentRegistrar.RegisterPackageActions(autofacBuilder, typeFinder);
            componentRegistrar.RegisterPropertyEditors(autofacBuilder, typeFinder);
            componentRegistrar.RegisterSurfaceControllers(autofacBuilder, typeFinder);
            componentRegistrar.RegisterTreeControllers(autofacBuilder, typeFinder);

            //build the container
            var container = autofacBuilder.Build();

            var result = container.Resolve<UmbracoAreaRegistration>();

            Assert.IsNotNull(result);
        }
    }
}
