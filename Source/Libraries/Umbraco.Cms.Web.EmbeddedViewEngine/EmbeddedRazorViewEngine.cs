using System;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.EmbeddedViewEngine
{
    

    /// <summary>
    /// View Engine for embedded razor views
    /// </summary>
    public class EmbeddedRazorViewEngine : RazorViewEngine
    {

        public EmbeddedRazorViewEngine(IViewPageActivator viewPageActivator)
            : base(viewPageActivator)
        {
            Init();
        }
        
        /// <summary>
        /// Sets default locations to look for views in for areas
        /// </summary>
        public EmbeddedRazorViewEngine()
        {
            Init();
        }

        private void Init()
        {
            //these are the originals:

            //base.AreaViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
            //base.AreaMasterLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
            //base.AreaPartialViewLocationFormats = new string[] { "~/Areas/{2}/Views/{1}/{0}.cshtml", "~/Areas/{2}/Views/{1}/{0}.vbhtml", "~/Areas/{2}/Views/Shared/{0}.cshtml", "~/Areas/{2}/Views/Shared/{0}.vbhtml" };
            //base.ViewLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
            //base.MasterLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
            //base.PartialViewLocationFormats = new string[] { "~/Views/{1}/{0}.cshtml", "~/Views/{1}/{0}.vbhtml", "~/Views/Shared/{0}.cshtml", "~/Views/Shared/{0}.vbhtml" };
            //base.FileExtensions = new string[] { "cshtml", "vbhtml" };

            //ViewLocationFormats = new string[] 
            //{ 
            //    "~/Views/Umbraco/{0}.cshtml",   //include the Umbraco path for 'master pages'
            //    "~/EV.axd/{0}.cshtml"  //include our virtual path for embedded views
            //};

            ViewLocationFormats = new[] 
            { 
                "~" + EmbeddedViewPath.PathPrefix + "{0}.cshtml"  //include our virtual path for embedded views
            };

            AreaViewLocationFormats = new[] 
            { 
                "~" + EmbeddedViewPath.PathPrefix + "{0}.cshtml"  //include our virtual path for embedded views
            };

            PartialViewLocationFormats = new[] 
            { 
                "~" + EmbeddedViewPath.PathPrefix + "{0}.cshtml"  //include our virtual path for embedded views
            };

            AreaPartialViewLocationFormats = new[] 
            { 
                "~" + EmbeddedViewPath.PathPrefix + "{0}.cshtml"  //include our virtual path for embedded views
            };

            //set the VirtualPathProvider
            VirtualPathProvider = new EmbeddedViewVirtualPathProvider();
        }

        /// <summary>
        /// This overrides this method specifically to just check against our virtual path provider instead
        /// of going back to the BuildManager to check.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            return this.VirtualPathProvider.FileExists(virtualPath);
        }        

    }

}
