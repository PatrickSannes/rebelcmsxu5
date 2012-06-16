using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Foundation.Web.Localization.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Globalization;

namespace Sandbox.Localization.WebTest
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {            

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_BeginRequest()
        {            
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        }
        
        protected void Application_Start()
        {
            //foreach (var p in ModelValidatorProviders.Providers)
            //{
            //    HttpContext.Current.Response.Write(p.GetType().Name + "<br />\n");
            //}

            //Bootstrap localization            
            LocalizationBootstrapper.Setup();
            var textManager = LocalizationBootstrapper.CurrentManager;
            ModelBinders.Binders.DefaultBinder = new LocalizingDefaultModelBinder(textManager);

            ModelValidatorProviders.Providers.Remove(ModelValidatorProviders.Providers.FirstOrDefault(x => x is DataAnnotationsModelValidatorProvider));
            ModelValidatorProviders.Providers.Add(new LocalizingDataAnnotationsModelValidatorProvider(textManager));
            

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }                
    }
}