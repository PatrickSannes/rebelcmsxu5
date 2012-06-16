using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;
using Umbraco.Hive;

using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.Persistence.Model.Constants;


namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.StylesheetTreeControllerId, "Stylesheets")]
    [UmbracoTree]
    public class StylesheetTreeController : AbstractFileSystemTreeController
    {

        public StylesheetTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.StylesheetEditorControllerId); }
        }
        
        protected override string HiveUriRouteMatch
        {
            get { return "storage://stylesheets"; }
        }


        /// <summary>
        /// Need to check if the node being requested is a file, if so, return the rules for it, otherwise process as per normal
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            using (var uow = Hive.CreateReadonly())
            {
                var stylesheet = uow.Repositories.Get<File>(parentId);
                if (!stylesheet.IsContainer)
                {
                    var rules = StylesheetHelper.ParseRules(stylesheet);
                    if (rules.Any())
                    {
                        foreach (var rule in rules)
                        {
                            var hiveId = new HiveId(new Uri(HiveUriRouteMatch), string.Empty, new HiveIdValue(parentId.Value + "/" + rule.Name.Replace(" ", "__s__")));
                            var node = CreateTreeNode(hiveId,
                                null,
                                rule.Name,
                                Url.GetEditorUrl("EditRule", hiveId, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings),
                                false,
                                "tree-css-item");
                            node.AddEditorMenuItem<Delete>(this, "deleteUrl", "DeleteRule");
                            NodeCollection.Add(node);
                        }
                    }
                    return UmbracoTree();
                }
            }
            
            return base.GetTreeData(parentId, queryStrings);
        }

        protected override void CustomizeFileNode(TreeNode n, FormCollection queryStrings)
        {
            n.AddEditorMenuItem<CreateItem>(this, "createUrl", "EditRule");
            base.CustomizeFileNode(n, queryStrings);            
            n.AddMenuItem<Reload>();

            n.Icon = "tree-css";

            using (var uow = Hive.CreateReadonly())
            {
                var stylesheet = uow.Repositories.Get<File>(n.HiveId);
                var rules = StylesheetHelper.ParseRules(stylesheet);                
                n.HasChildren = rules.Count() > 0;
                if (n.HasChildren)
                {
                    n.JsonUrl = Url.GetTreeUrl(GetType(), n.HiveId, queryStrings);
                }

            }
        }
    }
}
