using System;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MemberTreeControllerId, "Members")]
    [UmbracoTree]
    public class MemberTreeController : SupportsEditorTreeController
    {
        public MemberTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// Customize root node
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var n = base.CreateRootNode(queryStrings);
            //add some menu items to the created root node
            n.AddEditorMenuItem<CreateItem>(this, "createUrl", "Create");
            n.AddMenuItem<Reload>();
            return n;
        }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            //if its the first level
            if (parentId == RootNodeId)
            {
                for (var i = 'A'; i < 'Z'; i++)
                {
                    var id = new HiveId(new Uri("security://members"), "", new HiveIdValue(Char.ConvertFromUtf32(i)));
                    var treeNode = CreateTreeNode(
                        id,
                        queryStrings,
                        Char.ConvertFromUtf32(i),
                        Url.GetEditorUrl(id, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings));
                    treeNode.HasChildren = true;
                    treeNode.AddMenuItem<Reload>();
                    NodeCollection.Add(treeNode);
                }
                //add the other
                var otherId = new HiveId(new Uri("security://members"), "", new HiveIdValue("OTHER"));
                var otherNode = CreateTreeNode(
                    otherId,
                    queryStrings,
                    "Other",
                    Url.GetEditorUrl(otherId, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings));
                NodeCollection.Add(otherNode);
            }
            else
            {
                if (parentId.Value.Type == HiveIdValueTypes.String)
                {
                    var stringId = parentId.Value.Value.ToString();
                    using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<ISecurityStore>(new Uri("security://members")))
                    {
                        IQueryable<Member> query;
                        if (stringId == "OTHER")
                        {
                            //TODO: Somehow we need to make this work but the query visitor doesn't support a !StartsWith(
                            throw new NotImplementedException("Currently the query visitors do not support a 'not starts with' query");
                        }
                        else
                        {
                            //get members by letter
                            query = uow.Repositories.QueryContext.Query<Member>()
                                .Where(x => x.Username.StartsWith(stringId));
                        }

                        foreach (var treeNode in query
                            .ToArray() //need to execute the query
                            .Select(x => CreateTreeNode(
                                x.Id,
                                queryStrings,
                                x.Name,
                                Url.GetEditorUrl(x.Id, EditorControllerId, BackOfficeRequestContext.RegisteredComponents, BackOfficeRequestContext.Application.Settings))))
                        {
                            treeNode.HasChildren = false;

                            //add the menu items
                            treeNode.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");

                            NodeCollection.Add(treeNode);
                        }
                    }

                }
                else
                {
                    throw new NotSupportedException("The Member tree does not have additional levels underneath a member object");
                }
            }

            return UmbracoTree();
        }

        /// <summary>
        /// Returns a hard coded root node id so that tree syncing works properly
        /// </summary>
        protected override HiveId RootNodeId
        {
            get { return new HiveId(new Uri("members://"), "", new HiveIdValue(new Guid("9E0C77B7-F59F-43B5-928B-9AC33A14B6C1"))); }
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.MemberEditorControllerId); }
        }

    }
}