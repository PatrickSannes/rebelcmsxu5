using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;

namespace Umbraco.Cms.Web
{
    public static class DashboardItemModelExtensions
    {
        public static Lazy<AbstractEditorController, EditorMetadata> GetEditorDashboardAction(
            this DashboardItemModel dashboard,
            ComponentRegistrations components,
            out MethodInfo action)
        {
            //we need to get the surface controller by name
            var parts = dashboard.ViewName.Split('.');
            if (parts.Length != 2)
                throw new FormatException("The string format for dashboard.ViewName for child actions must be: ControllerName.ActionName");
            var controllerName = parts[0];
            var controllerAction = parts[1];
            var editorController = components.EditorControllers
                .Where(x => x.Metadata.ControllerName == controllerName).SingleOrDefault();
            if (editorController == null)
                throw new ApplicationException("Could not find the Editor controller '" + controllerName);
            if (!editorController.Metadata.HasChildActionDashboards)
                throw new ApplicationException("The Editor controller '" + controllerName + "' is not advertising that it HasChildActionDashboards");
            //now we need to get the controller's referenced child action
            var childAction = editorController.Metadata.ComponentType.GetMethods()
                .Where(x => x.Name == controllerAction && x.GetCustomAttributes(typeof(ChildActionOnlyAttribute), false).Any())
                .SingleOrDefault();
            if (childAction == null)
                throw new ApplicationException("The Editor controller '" + controllerName + "' with Action: '" + controllerAction + "' could not be found or was not attributed with a ChildActionOnlyAttribute");

            action = childAction;
            return editorController;
        }
    }
}