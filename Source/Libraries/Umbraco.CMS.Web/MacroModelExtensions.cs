using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Surface;

namespace Umbraco.Cms.Web
{
    public static class MacroModelExtensions
    {

        /// <summary>
        /// Gets the surface controller meta data and a reference to the MethodInfo object for the ChildAction to render based on the 
        /// macro definition
        /// </summary>
        /// <param name="macro"></param>
        /// <param name="components"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Lazy<SurfaceController, SurfaceMetadata> GetSurfaceMacroChildAction(
            this MacroDefinition macro, 
            ComponentRegistrations components,
            out MethodInfo action)
        {
            //need to see if the surface controller is part of an area
            var areaParts = macro.SelectedItem.Split('-');
            var areaName = areaParts.Length > 1
                               ? areaParts[0]
                               : "";

            //we need to get the surface controller by name
            var controllerParts = macro.SelectedItem.Split('.');
            if (controllerParts.Length != 2)
                throw new FormatException("The string format for macro.SelectedItem for child actions must be: [AreaName-]ControllerName.ActionName");
            var controllerName = controllerParts[0].Replace(areaName + "-", ""); //strip off area name and hyphen if present (will be for plug-in based surface controllers, won't be for local ones)
            var controllerAction = controllerParts[1];
            
            //get the surface controller for the area/controller name
            var surfaceController = components.SurfaceControllers
                .Where(x => x.Metadata.ControllerName == controllerName
                    && (areaName == "" || x.Metadata.PluginDefinition.PackageName == areaName))
                .SingleOrDefault();
            
            if (surfaceController == null)
                throw new ApplicationException("Could not find the Surface controller '" + controllerName);
            if (!surfaceController.Metadata.HasChildActionMacros)
                throw new ApplicationException("The Surface controller '" + controllerName + "' is not advertising that it HasChildActionMacros");
            //now we need to get the controller's referenced child action
            var childAction = surfaceController.Metadata.ComponentType.GetMethods()
                .Where(x => x.Name == controllerAction && x.GetCustomAttributes(typeof(ChildActionOnlyAttribute), false).Any())
                .SingleOrDefault();
            if (childAction == null)
                throw new ApplicationException("The Surface controller '" + controllerName + "' with Action: '" + controllerAction + "' could not be found or was not attributed with a ChildActionOnlyAttribute");

            action = childAction;
            return surfaceController;
        }

    }
}
