using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Macros
{
    /// <summary>
    /// A utility class for rendering macros either as ActionResult or as string
    /// </summary>
    public class MacroRenderer
    {
        private readonly ComponentRegistrations _componentRegistrations;
        private readonly IRoutableRequestContext _routableRequestContext;
        private readonly IUmbracoApplicationContext _applicationContext;

        public MacroRenderer(ComponentRegistrations componentRegistrations, IRoutableRequestContext routableRequestContext)
        {
            _componentRegistrations = componentRegistrations;
            _routableRequestContext = routableRequestContext;
            _applicationContext = routableRequestContext.Application;
        }

        /// <summary>
        /// Renders the macro output as a string
        /// </summary>
        /// <param name="macroAlias"></param>
        /// <param name="macroParams"></param>
        /// <param name="currentControllerContext"></param>
        /// <param name="isForRichTextEditor">If the request is to render the contents in the back office rich text editor</param>
        /// <param name="resolveContent"></param>
        /// <returns></returns>
        public string RenderMacroAsString(
            string macroAlias,
            IDictionary<string, string> macroParams,
            ControllerContext currentControllerContext,
            bool isForRichTextEditor,
            Func<Content> resolveContent)
        {
            //method to get the string output of the ActionResult
            Func<ActionResult, string> getStringOutput = ar =>
                {
                    if (ar is ViewResultBase)
                    {
                        var viewResult = (ViewResultBase)ar;

                        return currentControllerContext.RenderViewResultAsString(viewResult);
                    }
                    if (ar is ContentResult)
                    {
                        var contentResult = (ContentResult)ar;
                        return contentResult.Content;
                    }
                    throw new NotSupportedException("Its not possible to retreive the output of a macro that doesn't return a ViewResultBase");
                };

            if (isForRichTextEditor)
            {
                return getStringOutput(RenderMacro(macroAlias, macroParams, currentControllerContext, true, resolveContent));
            }

            //if we not rendering for the rich text editor then check if we've cached this in the ScopedCache first, 
            //even though we are caching some macros in application cache, we're only caching the ActionResult and not the  text
            //output which still requires a tiny amount of processing, so we'll store all equal macro string output in ScopedCache too, 
            //as this will speed things regardless of whehter macros are cached or not if there's a few of the same ones per page

            return _applicationContext.FrameworkContext
                .ScopedCache.GetOrCreate("umb-macro-" + macroAlias + macroParams.GetHashCode(),
                                         () => getStringOutput(RenderMacro(macroAlias, macroParams, currentControllerContext, false, resolveContent)))
                                         .ToString();
        }

        private Tuple<MacroEditorModel, File> GetMacroModel(string macroAlias)
        {
            using (var uow = _applicationContext.Hive.OpenReader<IFileStore>(new Uri("storage://macros")))
            {
                var filename = macroAlias + ".macro";
                var macroFile = uow.Repositories.Get<File>(new HiveId(filename));
                if (macroFile == null)
                    throw new ApplicationException("Could not find a macro with the specified alias: " + macroAlias);
                return new Tuple<MacroEditorModel, File>(MacroSerializer.FromXml(Encoding.UTF8.GetString(macroFile.ContentBytes)), macroFile);
            }
        }

        private ActionResult GetMacroResult(string macroAlias, Func<MacroEditorModel> getMacroMethod, Func<Content> getNodeMethod, IDictionary<string, string> macroParams, ControllerContext currentControllerContext)
        {
            var currentNode = getNodeMethod();
            if (currentNode == null)
            {
                //If we have no current node (i.e. its new content rendering in the TinyMCE editor), then
                //we can only return a friendly content message.
                return null;
            }
            var macro = getMacroMethod();

            var engine = _componentRegistrations.MacroEngines.SingleOrDefault(x => x.Metadata.EngineName.InvariantEquals(macro.MacroType));
            if (engine == null)
            {
                throw new InvalidOperationException("Could not find a MacroEngine with the name " + macro.MacroType);
            }

            try
            {
                //execute the macro engine
                return engine.Value.Execute(
                    currentNode,
                    macroParams,
                    new MacroDefinition { MacroEngineName = macro.MacroType, SelectedItem = macro.SelectedItem },
                    currentControllerContext,
                    _routableRequestContext);
            }
            catch (Exception ex)
            {
                //if there's an exception, display a friendly message and log the error
                var txt = "Macro.RenderingFailed.Message".Localize(this, new { Error = ex.Message, MacroName = macroAlias });
                var title = "Macro.RenderingFailed.Title".Localize();
                LogHelper.Error<MacroRenderer>(txt, ex);
                return MacroError(txt, title);
            }
        }

        /// <summary>
        /// Performs the action rendering
        /// </summary>
        /// <param name="macroAlias"></param>
        /// <param name="currentControllerContext"></param>
        /// <param name="isForRichTextEditor">If the request is to render the contents in the back office rich text editor</param>
        /// <param name="resolveContent">callback to get the 'Content' model</param>
        /// <param name="macroParams"></param>
        /// <returns></returns>
        /// <remarks>
        /// 
        /// We need to check if this is for the rich text editor, in which case, we need to bypass any caching
        /// so that it renders out the correct content,
        /// Then if its not for the RTE, we need to check our cache to see if its in there, if so then we'll just 
        /// return it, otherwise we will add the cached item with a dependency on our cache 'File' path.
        /// 
        /// </remarks>
        public ActionResult RenderMacro(
            string macroAlias,
            IDictionary<string, string> macroParams,
            ControllerContext currentControllerContext,
            bool isForRichTextEditor,
            Func<Content> resolveContent)
        {           

            ActionResult macroResult = null;

            if (!isForRichTextEditor)
            {
                //if its not for rich text....

                try
                {
                    var output = _applicationContext.FrameworkContext
                                .ApplicationCache.GetOrCreate("umb-macro-" + macroAlias + macroParams.GetHashCode(),
                                                              () =>
                                                              {
                                                                  var macroModel = GetMacroModel(macroAlias);

                                                                  //set the macroResult to the local variable
                                                                  macroResult = GetMacroResult(macroAlias, () => macroModel.Item1, resolveContent, macroParams, currentControllerContext);
                                                                  //if it is null, it means that there was no content item
                                                                  if (macroResult == null)
                                                                      return null;

                                                                  //check if the macro should be cached at all
                                                                  if (!macroModel.Item1.CacheByPage)
                                                                  {
                                                                      //By returning null, this will not be added to the cache and a null value returned
                                                                      return null;
                                                                  }
                                                                  //TODO: Check caching by member!

                                                                  //return our caching parameters
                                                                  //TODO: We might not want a 'normal' cache dependency at some point, need to determine a way to specify cache dependencies or create a custom one based on the 'File' object or something
                                                                  var cacheParams = new HttpRuntimeCacheParameters<ActionResult>(macroResult)
                                                                      {
                                                                          Dependencies = new CacheDependency(macroModel.Item2.RootedPath),
                                                                          SlidingExpiration = new TimeSpan(0, 0, 0, macroModel.Item1.CachePeriodSeconds)
                                                                      };
                                                                  return cacheParams;
                                                              });

                    //if this is null, it means:
                    // - there was no current node, or it didn't get cached because the macro definition said to not cache it.
                    //then we can check if our local 'macroResult' variable was set, if so it means:
                    // - there was a current node and it simply wasn't cached because of the macro definition.
                    if (output != null)
                        return (ActionResult)output;
                    if (macroResult != null)
                        return macroResult;
                    //return null because there must be no 'currentNode'
                    return null;

                }
                catch (ApplicationException ex)
                {
                    //if there's an exception, display a friendly message and log the error
                    var txt = "Macro.RenderingFailed.Message".Localize(this, new { Error = ex.Message, MacroName = macroAlias });
                    var title = "Macro.RenderingFailed.Title".Localize();
                    LogHelper.Error<MacroRenderer>(txt, ex);
                    return MacroError(txt, title);
                }

                
            }
            else
            {
                try
                {
                    //if it is for rich text...
                    var macroModel = GetMacroModel(macroAlias);

                    return !macroModel.Item1.RenderContentInEditor
                        ? NoRichTextRenderMode(macroAlias)
                        : GetMacroResult(macroAlias, () => macroModel.Item1, resolveContent, macroParams, currentControllerContext);
                }
                catch (ApplicationException ex)
                {
                    //if there's an exception, display a friendly message and log the error
                    var txt = "Macro.RenderingFailed.Message".Localize(this, new { Error = ex.Message, MacroName = macroAlias });
                    var title = "Macro.RenderingFailed.Title".Localize();
                    LogHelper.Error<MacroRenderer>(txt, ex);
                    return MacroError(txt, title);
                }
            }

        }

        /// <summary>
        /// Returns the html to display an error inline
        /// </summary>
        /// <param name="error"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public ContentResult MacroError(string error, string title)
        {
            return new ContentResult
            {
                Content = "<div style='color:#8a1f11;padding:4px;border:2px solid #FBC2C4;'><h4 style='color:#8a1f11;font-weight:bold;margin:0;font-size:14px;'>" + title + "</h4><p style='margin:0;font-size:12px;'>" + error + "</p></div>"
            };
        }

        /// <summary>
        /// Returns the content to display in the Rich Text Editor when the macro is flagged to not render contents
        /// </summary>
        /// <returns></returns>
        public ContentResult NoRichTextRenderMode(string macroAlias)
        {
            return new ContentResult
            {
                Content = "<div style='color:#514721;'>Macro: '<strong>" + macroAlias + "</strong>'</div>"
            };
        }
    }
}