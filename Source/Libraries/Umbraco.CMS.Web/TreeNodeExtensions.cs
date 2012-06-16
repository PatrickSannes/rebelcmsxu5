using System;
using System.Collections.Generic;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees;

namespace Umbraco.Cms.Web
{
    public static class TreeNodeExtensions
    {
        /// <summary>
        /// A helper method to add a menu action that displays an editor
        /// </summary>
        /// <typeparam name="T">The type of menu action to add</typeparam>
        /// <param name="controller">The SupportsEditorTreeController</param>
        /// <param name="node">the tree node to add the menu action to</param>
        /// <param name="key">The key of the item that is added to the AdditionalData for the URL of the action</param>
        /// <param name="controllerAction">The action URL to lookup based on the EditorControllerId/EditorControllerName</param>
        /// <param name="additionalData"></param>
        public static void AddEditorMenuItem<T>(this TreeNode node, SupportsEditorTreeController controller, string key, string controllerAction, IDictionary<string, object> additionalData = null)
            where T : MenuItem
        {
            if (additionalData == null)
                additionalData = new Dictionary<string, object>();
            if (additionalData.ContainsKey(key))
            {
                throw new InvalidOperationException("The key supplied cannot also exist in the additionalData dictionary");
            }
            additionalData.Add(key, controller.Url.GetEditorUrl(controllerAction, node.HiveId, controller.EditorControllerId, controller.BackOfficeRequestContext.RegisteredComponents, controller.BackOfficeRequestContext.Application.Settings));
            node.AddMenuItem<T>(additionalData);
        }

    }
}