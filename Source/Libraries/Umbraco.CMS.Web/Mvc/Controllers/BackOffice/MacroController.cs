using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ModelBinders;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{

    /// <summary>
    /// Controller to render macro content
    /// </summary>
    public class MacroController : Controller, IRequiresRoutableRequestContext
    {
        private readonly IRenderModelFactory _renderModelFactory;
        private readonly MacroRenderer _macroRenderer;

        public MacroController(IRoutableRequestContext requestContext, IRenderModelFactory renderModelFactory)
        {
            RoutableRequestContext = requestContext;
            _renderModelFactory = renderModelFactory;
            _macroRenderer = new MacroRenderer(RoutableRequestContext.RegisteredComponents, RoutableRequestContext);
        }

        /// <summary>
        /// Child action to render a macro
        /// </summary>
        /// <param name="macroAlias">The macro alias to render</param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult Index(string macroAlias)
        {
            return RenderMacro(macroAlias, () =>
                {
                    var currentContext = _renderModelFactory.Create(HttpContext, HttpContext.Request.RawUrl);
                    return currentContext.CurrentNode;
                });
        }

        /// <summary>
        /// Performs the action rendering
        /// </summary>
        /// <param name="macroAlias"></param>
        /// <param name="resolveContent">callback to get the 'Content' model</param>
        /// <returns></returns>
        private ActionResult RenderMacro(string macroAlias, Func<Content> resolveContent)
        {
            //need to get the macro params object out of the route values
            IDictionary<string, string> macroParams = new Dictionary<string, string>();
            if (RouteData.Values.Any(x => x.Key == "macroParams" && x.Value != null))
                macroParams = RouteData.Values.Single(x => x.Key == "macroParams").Value as IDictionary<string, string>;

            var result = _macroRenderer.RenderMacro(macroAlias, macroParams, ControllerContext, false, resolveContent);
            return result ?? NoMacroData();
        }

        /// <summary>
        /// Returns the contents of a macro for use in the back office
        /// </summary>
        /// <param name="currentNodeId">The current node id.</param>
        /// <param name="macroAlias">The macro alias.</param>
        /// <param name="macroParams">The macro params.</param>
        /// <returns></returns>
        [UmbracoAuthorize()]
        [HttpPost]
        public JsonResult MacroContents(HiveId currentNodeId, string macroAlias,
            //custom model binder for this json dictionary
            [ModelBinder(typeof(JsonDictionaryModelBinder))]
            IDictionary<string, object> macroParams)
        {
            if (macroParams == null)
                macroParams = new Dictionary<string, object>();

            var stringOutput = _macroRenderer.RenderMacroAsString(
                macroAlias, macroParams.ToDictionary(x => x.Key, x => x.Value.ToString()), ControllerContext, true,
                () =>
                {

                    if (currentNodeId.IsNullValueOrEmpty())
                        return null;

                    using (var uow = RoutableRequestContext.Application.Hive.OpenReader<IContentStore>(currentNodeId.ToUri()))
                    {
                        var entity = uow.Repositories.Get<TypedEntity>(currentNodeId);
                        if (entity == null)
                            throw new NullReferenceException("Could not find entity with id " + currentNodeId.ToString());

                        return RoutableRequestContext.Application.FrameworkContext.TypeMappers.Map<Content>(entity);
                    }
                });

            return Json(new
            {
                macroContent = stringOutput
            });
        }

        

        /// <summary>
        /// REturns the html to display when the node isn't saved yet
        /// </summary>
        /// <returns></returns>
        public ContentResult NoMacroData()
        {
            return new ContentResult
            {
                Content = "<div style='color:#514721;'>Cannot render macro contents without existing node data</div>"
            };
        }

        public IRoutableRequestContext RoutableRequestContext { get; set; }
    }
}
