using System;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;

namespace Umbraco.Cms.Web.Mvc.Areas
{
    public class InstallAreaRegistration : AreaRegistration
    {
        public const string RouteName = "umbraco-install";

        private readonly UmbracoSettings _umbracoSettings;

        public InstallAreaRegistration(UmbracoSettings umbracoSettings)
        {
            _umbracoSettings = umbracoSettings;            
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //map installer routes, only one controller
            context.Routes.MapRoute(
                RouteName,
                AreaName + "/{action}/{id}",
                new { controller = "Install", action = "Index", id = UrlParameter.Optional },
                null,
                new[] { typeof(InstallController).Namespace })//match controllers in these namespaces                
                .DataTokens.Add("area", AreaName);//only match this area
        }

        public override string AreaName
        {
            get { return "Install"; }
        }
    }
}