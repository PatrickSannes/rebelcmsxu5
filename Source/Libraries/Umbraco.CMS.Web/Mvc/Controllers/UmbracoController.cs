using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ActionInvokers;
using Umbraco.Cms.Web.Mvc.Metadata;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using File = Umbraco.Framework.Persistence.Model.IO.File;
using MSMvc = System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.Controllers
{

    /// <summary>
    /// The default controller for Umbraco front-end request
    /// </summary>
    [InstalledFilter]
    [OutputCache(CacheProfile = "umbraco-default")]
    public class UmbracoController : Controller, IRequiresRoutableRequestContext
    {
        public IRoutableRequestContext RoutableRequestContext { get; set; }

        #region Static methods

        /// <summary>
        /// Return the ID of an controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="editorControllerType"></param>
        /// <returns></returns>
        public static Guid GetControllerId<T>(Type editorControllerType)
            where T : PluginAttribute
        {
            //Locate the editor attribute
            var editorAttribute = editorControllerType
                .GetCustomAttributes(typeof(T), false)
                .OfType<T>();
            if (!editorAttribute.Any()) throw new InvalidOperationException("The controller plugin type is missing the " + typeof(T).FullName + " attribute");
            var attr = editorAttribute.First();
            return attr.Id;
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public static string GetControllerName(Type controllerType)
        {
            return controllerType.Name.Substring(0, controllerType.Name.LastIndexOf("Controller"));
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetControllerName<T>()
        {
            return GetControllerName(typeof (T));
        }
        #endregion

        /// <summary>
        /// Constructor initializes custom action invoker
        /// </summary>
        public UmbracoController()
        {
            //this could be done by IoC but we really don't want people to have to create
            //the custom constructor each time they want to create a controller that extends this one.
            ActionInvoker = new UmbracoActionInvoker(RoutableRequestContext);
            RoutableRequestContext = DependencyResolver.Current.GetService<IRoutableRequestContext>();
        }

        /// <summary>
        /// Constructor initializes custom action invoker
        /// </summary>
        public UmbracoController(IRoutableRequestContext routableRequestContext)
        {
            ActionInvoker = new UmbracoActionInvoker(RoutableRequestContext);
            RoutableRequestContext = routableRequestContext;
        }

        /// <summary>
        /// The default action to render the template/view
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This will check if the view for the template for the IRoutableItem exists, if it doesn't will render the default 'Index' view.
        /// </remarks>
        [Internationalize]
        public virtual ActionResult Index(IUmbracoRenderModel model)
        {
            if (model.CurrentNode == null) return new HttpNotFoundResult();

            using (var uow = RoutableRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
            {
                var templateFile = model.CurrentNode.CurrentTemplate != null
                                   ? uow.Repositories.Get<File>(model.CurrentNode.CurrentTemplate.Id)
                                   : null;

                if (templateFile != null)
                {
                    //try to find the view based on all engines registered.
                    var view = MSMvc.ViewEngines.Engines.FindView(ControllerContext, 
                        Path.GetFileNameWithoutExtension(templateFile.RootedPath), "");

                    if (view.View != null)
                    {
                        return View(view.View, model.CurrentNode);
                    }
                }

                //return the compiled default view!
                //TODO: Remove magic strings
                return View(
                    EmbeddedViewPath.Create("Umbraco.Cms.Web.EmbeddedViews.Views.TemplateNotFound.cshtml,Umbraco.Cms.Web"),
                    model.CurrentNode);
            }

            
        }

        protected override void Dispose(bool disposing)
        {
            LogHelper.TraceIfEnabled<UmbracoController>("Controller being disposed, calling scope cleanup: {0}", () => RoutableRequestContext.Application != null);
            if (RoutableRequestContext.Application != null)
                RoutableRequestContext.Application.FrameworkContext.ScopedFinalizer.FinalizeScope();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Returns the currently rendered Umbraco page
        /// </summary>
        /// <returns></returns>
        protected UmbracoPageResult CurrentUmbracoPage()
        {
            return new UmbracoPageResult();
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(HiveId pageId)
        {
            return new RedirectToUmbracoPageResult(pageId, RoutableRequestContext);
        }

        /// <summary>
        /// Redirects to the Umbraco page with the given id
        /// </summary>
        /// <param name="pageEntity"></param>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToUmbracoPage(TypedEntity pageEntity)
        {
            return new RedirectToUmbracoPageResult(pageEntity, RoutableRequestContext);
        }

        /// <summary>
        /// Redirects to the currently rendered Umbraco page
        /// </summary>
        /// <returns></returns>
        protected RedirectToUmbracoPageResult RedirectToCurrentUmbracoPage()
        {
            //validate that the current page execution is not being handled by the normal umbraco routing system
            if (!ControllerContext.RouteData.DataTokens.ContainsKey("umbraco-route-def"))
            {
                throw new InvalidOperationException("Can only use " + typeof(UmbracoPageResult).Name + " in the context of an Http POST when using the BeginUmbracoForm helper");
            }

            var routeDef = (RouteDefinition)ControllerContext.RouteData.DataTokens["umbraco-route-def"];
            return new RedirectToUmbracoPageResult(routeDef.RenderModel.CurrentNode, RoutableRequestContext);
        }

        
    }
}
