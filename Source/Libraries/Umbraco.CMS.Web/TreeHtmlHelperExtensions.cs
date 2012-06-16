using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ClientDependency.Core;
using ClientDependency.Core.Mvc;
using Umbraco.Cms.Web.Context;
using ClientDependency.Core.Mvc;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Trees;

namespace Umbraco.Cms.Web
{
    public static class TreeHtmlHelperExtensions
    {
       
        /// <summary>
        /// Renders a tree
        /// </summary>
        /// <param name="html"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static IHtmlString RenderTree(this HtmlHelper html, TreeRenderModel model)
        {
            //Ensure dependencies
            html
                .RequiresJs("jQuery/jquery.js", "Scripts", 0)
                .RequiresJs("jQuery/jquery.hotkeys.js", "Scripts", 10)
                .RequiresJs("jQuery/jquery.jstree.js", "Scripts", 20)
                .RequiresJs("WebToolkit/webtoolkit.md5.js", "Scripts", 0)
                .RequiresJs("Modernizr/modernizr.js", "Scripts", 0)
                .RequiresJs("Tree/UmbracoTree.js", "Modules", 100)
                .RequiresJs("Tree/UmbracoContextMenu.js", "Modules", 101)
                .RequiresJs("Tree/NodeFormatter.js", "Modules", 101)
                .RequiresJs("Tree/ApplicationPersistence.js", "Modules", 101)
                .RequiresCss("Tree/themes/umbraco/MenuIcons.css", "Modules");

            var backOfficeRequestContext = DependencyResolver.Current.GetService<IBackOfficeRequestContext>();
            foreach (var path in backOfficeRequestContext.DocumentTypeIconResolver.Sprites.Select(sprites =>
                    backOfficeRequestContext.Application.Settings.UmbracoFolders.DocTypeIconFolder + "/" + sprites.Value.Name))
            {
                html.ViewContext.HttpContext.GetLoader().RegisterDependency(path, ClientDependencyType.Css);
            }

            var treeHolder = new TagBuilder("div");
            treeHolder.AddCssClass("tree");
            treeHolder.Attributes.Add("id", model.TreeHtmlElementId);

            var urlHelper = new UrlHelper(html.ViewContext.RequestContext);

            var script = @"(function ($) {
    $(document).ready(function () {
        var opts = {
            jsonUrl: '" + model.TreeUrl + @"',
            themeFolder: '" + urlHelper.GetModulesPath().Path + "/Tree/themes/" + @"',
            showContext: " + model.ShowContextMenu.ToString().ToLower() + @"
        };
        $('#" + model.TreeHtmlElementId + @"').umbracoTree(opts, " + model.ManuallyInitialize.ToString().ToLower() + @");
    });
})(jQuery);";

            var scriptBuilder = new TagBuilder("script");
            scriptBuilder.Attributes.Add("type", "text/javascript");
            scriptBuilder.InnerHtml = script;
            return new MvcHtmlString(treeHolder.ToString() + scriptBuilder.ToString());
        }

        /// <summary>
        /// Renders out the menu definition html structure and ensures that sub menu items are recursively created
        /// </summary>
        /// <param name="html"></param>
        /// <param name="menuItems"></param>
        /// <returns></returns>
        public static IHtmlString RenderMenuDefinition(this HtmlHelper html, IEnumerable<MenuItem> menuItems)
        {           
            var builder = new StringBuilder();
            html.RenderMenuDefinition(menuItems, builder);
            return new MvcHtmlString(builder.ToString());
        }

        private static void RenderMenuDefinition(this HtmlHelper html, IEnumerable<MenuItem> menuItems, StringBuilder sb)
        {
            const string s = @"<div data-id=""{0}"" data-title=""{1}"" data-sep-before=""{2}"" data-sep-after=""{3}"" data-client-click=""{4}"" data-icon=""{5}"" {6}>";
            foreach (var m in menuItems)
            {
                var customAttributes = new StringBuilder();
                foreach(var custom in m.AdditionalData)
                {
                    customAttributes.AppendFormat("data-custom-{0}=\"{1}\"", custom.Key, custom.Value);
                }

                sb.AppendLine(string.Format(s, m.MenuItemId, m.Title, m.SeparatorBefore.ToString().ToLower(), m.SeparatorAfter.ToString().ToLower(), m.OnClientClick, m.Icon, customAttributes.ToString()));
                //recurse if children
                if (m.SubMenuItems.Any())
                {
                    html.RenderMenuDefinition(m.SubMenuItems, sb);
                }
                sb.AppendLine("</div>");
            }
        }
    }
}
