using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Localization;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Macros
{
    /// <summary>
    /// The ChildAction MacroEngine
    /// </summary>
    [UmbracoMacroEngine]
    [MacroEngine("5644CD86-EB78-4988-A9D7-735671B007D1", "ChildAction")]
    public class ChildActionMacroEngine : AbstractMacroEngine
    { 

        public override ActionResult Execute(
            Content currentNode,
            IDictionary<string, string> macroParams, 
            MacroDefinition macro, 
            ControllerContext currentControllerContext,
            IRoutableRequestContext routableRequestContext)
        {
            MethodInfo childAction;

            var surfaceController = macro.GetSurfaceMacroChildAction(routableRequestContext.RegisteredComponents, out childAction);

            if (!TypeFinder.IsTypeAssignableFrom<PartialViewResult>(childAction.ReturnType)
                && !TypeFinder.IsTypeAssignableFrom<ContentResult>(childAction.ReturnType))
            {
                throw new InvalidOperationException("ChildAction macros should have a return type of " + typeof (PartialViewResult).Name + " or " + typeof(ContentResult).Name);
            }

            using (var controller = surfaceController.Value)
            {
                if (controller == null)
                    throw new TypeLoadException("Could not create controller: " + UmbracoController.GetControllerName(surfaceController.Metadata.ComponentType));

                //need to get the macroParams to put into an array
                var actionParams = macroParams.Select(i => i.Value).ToArray();

                var area = routableRequestContext.Application.Settings.UmbracoPaths.BackOfficePath;
                if (surfaceController.Metadata.PluginDefinition.HasRoutablePackageArea())
                {
                    area = surfaceController.Metadata.PluginDefinition.PackageName;    
                }

                //proxy the request to the controller        
                var result = currentControllerContext.Controller.ProxyRequestToController(controller, childAction, area, actionParams);

                return result;
            }
        }

        public override IEnumerable<MacroParameter> GetMacroParameters(
            IBackOfficeRequestContext backOfficeRequestContext, 
            MacroDefinition macroDefinition)
        {
            MethodInfo childAction;
            var surfaceController = macroDefinition.GetSurfaceMacroChildAction(backOfficeRequestContext.RegisteredComponents, out childAction);
            //now we need to get all of the parameters from the method info object
            var parameters = childAction.GetParameters();
            if (parameters.Any(x => x.IsOut || x.IsRetval || x.IsLcid))
            {
                //localize this exception since it wil be displayed in the UI
                throw new InvalidOperationException("Macro.ParamsFailedSurfaceAction.Message".Localize());
            }

            return childAction.GetParameters()
                                            .OrderBy(x => x.Position)
                                            .Select(x => new MacroParameter()
                                            {
                                                ParameterName = x.Name
                                                //TODO: Figure out how to automatically select which macro parameter editor to use
                                            });
        }

        /// <summary>
        /// Returns the names of each ChildAction that can be used as a Macro
        /// </summary>
        /// <param name="backOfficeRequestContext"></param>
        /// <returns></returns>
        public override IEnumerable<SelectListItem> GetMacroItems(IBackOfficeRequestContext backOfficeRequestContext)
        {
            return (from s in backOfficeRequestContext.RegisteredComponents.SurfaceControllers
                        .Where(x => x.Metadata.HasChildActionMacros)
                    let childActions = s.Metadata.ComponentType.GetMethods()
                        .Where(a => a.GetCustomAttributes(typeof (ChildActionOnlyAttribute), false).Any())
                    from action in childActions
                    select (!s.Metadata.PluginDefinition.HasRoutablePackageArea() ? "" : s.Metadata.PluginDefinition.PackageName + "-") 
                        + s.Metadata.ControllerName + "." + action.Name)
                .ToArray()
                .Select(x => new SelectListItem {Text = x, Value = x});
        }
    }
}