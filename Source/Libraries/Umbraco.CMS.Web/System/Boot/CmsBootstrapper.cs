using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using DataAnnotationsExtensions.ClientValidation;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Mapping;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Mvc.Areas;
using Umbraco.Cms.Web.Mvc.Validation;
using Umbraco.Framework.Context;
using Umbraco.Framework.Localization.Web;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Tasks;

namespace Umbraco.Cms.Web.System.Boot
{
    public class CmsBootstrapper : AbstractBootstrapper
    {
        private readonly UmbracoSettings _settings;
        private readonly UmbracoAreaRegistration _areaRegistration;
        private readonly InstallAreaRegistration _installRegistration;
        private readonly IEnumerable<PackageAreaRegistration> _componentAreas;
        private readonly IAttributeTypeRegistry _attributeTypeRegistry;

        public CmsBootstrapper(UmbracoSettings settings,
            UmbracoAreaRegistration areaRegistration,
            InstallAreaRegistration installRegistration,
            IEnumerable<PackageAreaRegistration> componentAreas)
        {
            _areaRegistration = areaRegistration;
            _installRegistration = installRegistration;
            _componentAreas = componentAreas;
            _settings = settings;
            _attributeTypeRegistry = new DependencyResolverAttributeTypeRegistry();
        }

        public CmsBootstrapper(UmbracoSettings settings,
            UmbracoAreaRegistration areaRegistration,
            InstallAreaRegistration installRegistration,
            IEnumerable<PackageAreaRegistration> componentAreas,
            IAttributeTypeRegistry attributeTypeRegistry)
        {
            _areaRegistration = areaRegistration;
            _installRegistration = installRegistration;
            _componentAreas = componentAreas;
            _attributeTypeRegistry = attributeTypeRegistry;
            _settings = settings;
        }

        public override void Boot(RouteCollection routes)
        {
            base.Boot(routes);

            routes.RegisterArea(_installRegistration);
            //register all component areas
            foreach (var c in _componentAreas)
            {
                routes.RegisterArea(c);
            }

            //IMPORTANT: We need to register the Umbraco area after the components because routes overlap.
            // For example, a surface controller might have a url of:
            //   /Umbraco/MyPackage/Surface/MySurface
            // and because the default action is 'Index' its not required to be there, however this same route
            // matches the default Umbraco back office route of Umbraco/{controller}/{action}/{id}
            // so we want to make sure that the plugin routes are matched first
            routes.RegisterArea(_areaRegistration);

            //ensure that the IAttributeTypeRegistry is set
            AttributeTypeRegistry.SetCurrent(_attributeTypeRegistry);

            //register validation extensions
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();

            LocalizationWebConfig.RegisterRoutes(routes, _settings.UmbracoPaths.LocalizationPath);
            
            //If this is outside of an ASP.Net application (i.e. Unit test) and RegisterVirtualPathProvider is called then an exception is thrown.
            if (HostingEnvironment.IsHosted)
            {
                HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedViewVirtualPathProvider());    
            }
            

            //register custom validation adapters
            DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(
                typeof (HiveIdRequiredAttribute),
                (metadata, controllerContext, attribute) =>
                new RequiredHiveIdAttributeAdapter(metadata, controllerContext, (HiveIdRequiredAttribute) attribute));

        }

    }
}
