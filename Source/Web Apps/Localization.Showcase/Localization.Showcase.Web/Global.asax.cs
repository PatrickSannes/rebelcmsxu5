using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Framework.Localization.Web;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Localization.Configuration;
using Umbraco.Framework.Localization.Web.Mvc.Areas;

namespace Localization.Showcase.Web
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

            
            LocalizationWebConfig.RegisterRoutes(routes);
            

            routes.MapRoute(
                "Default", // Route name
                "{language}", // URL with parameters
                new { language="en-US", controller = "Friends", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );


        }
        

        protected void Application_Start()
        {
            
            AreaRegistration.RegisterAllAreas();

            //Setup default manager
            LocalizationWebConfig.SetupDefaultManager(typeof(MvcApplication).Assembly);            

            //Setup localizing binding stuff
            LocalizationWebConfig.SetupMvcDefaults();
          

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            // Store this because we need it for the demo
            DefaultLanguageResolver = ((DefaultTextManager)LocalizationConfig.TextManager).CurrentLanguage;
        }

        public static Func<LanguageInfo> DefaultLanguageResolver;
    }
}