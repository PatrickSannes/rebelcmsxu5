using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration.ApplicationSettings;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;
using System.Web.Routing;

using Umbraco.Framework;

namespace Umbraco.Cms.Web.Trees
{
    using global::System.Text;

    /// <summary>
    /// A controller to render the applictaion containing sub-trees via proxy actions
    /// </summary>
    [Tree("A700C7F9-2428-464B-9F1B-EF26ADC28730", "Application")]
    [UmbracoTree]
    public class ApplicationTreeController : SecuredBackOfficeController
    {

        public ApplicationTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            NodeCollection = new TreeNodeCollection();
        }


        /// <summary>
        /// The model collection for trees to add nodes to
        /// </summary>
        protected TreeNodeCollection NodeCollection { get; private set; }

        /// <summary>
        /// Render out the tree nodes for each tree in the application specified
        /// </summary>
        /// <param name="appAlias">Name of the app.</param>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult Index(string appAlias)
        {
            //get the request app name, by default it is content (based on default routes)
            if (string.IsNullOrEmpty(appAlias))
            {
                appAlias = "content";
            }
            
            //get the defined application
            var application = BackOfficeRequestContext.Application.Settings.Applications.Where(x => x.Alias.InvariantEquals(appAlias)).SingleOrDefault();
            if (application == null)
            {
                //nothing found so return an empty set
                return new UmbracoTreeResult(new TreeNodeCollection(), ControllerContext);
            }
            
            //set the application cookie
            this.HttpContext.Response.Cookies.Add(new HttpCookie("UMBAPP", appAlias));

            //get the trees for the application
            var trees = BackOfficeRequestContext.Application.Settings.Trees.Where(x => x.ApplicationAlias == application.Alias).ToArray();

            //if there's only one tree, then proxy the response to the one tree)
            if (trees.Count() == 1)
            {               
                //add the resulting nodes
                return GetRootResultFromTreeController(trees.First());
            }

            //if there's more than one tree, then add the proxied results to the tree collection for each of the
            //start nodes.

            //first we need to create the root node
            var nodeId = new HiveId(-1);
            var rootNode = new TreeNode(nodeId, BackOfficeRequestContext.RegisteredComponents.MenuItems, "")
            {
                HasChildren = true,
                EditorUrl = Url.GetDashboardUrl(appAlias),
                Title = application.Name
            };
            rootNode.Style.AddCustom("root-wrapper");
            NodeCollection.Add(rootNode);

            //create the child node collection
            rootNode.Children = new TreeNodeCollection();
            foreach (var t in trees)
            {
                //add the resulting nodes
                rootNode.Children.AddRange(GetRootResultFromTreeController(t).NodeCollection);
            }

            return new UmbracoTreeResult(NodeCollection, ControllerContext);

        }

        /// <summary>
        /// A proxy action for ISearchableTree action for the specified tree id
        /// </summary>
        /// <param name="searchModel"></param>
        /// <returns></returns>
        [HttpPost]
        public TreeSearchJsonResult Search(SearchModel searchModel)
        {
            Mandate.ParameterNotNull(searchModel, "model");

            //find the tree with the specified id
            var tree = BackOfficeRequestContext.RegisteredComponents
                .TreeControllers.Where(x => x.Metadata.Id.Equals(searchModel.TreeId)).SingleOrDefault();
            if (tree != null)
            {
                using (var controller = tree.Value)
                {
                    if (controller == null)
                    {
                        throw new TypeLoadException("Could not create controller: " + UmbracoController.GetControllerName(tree.Metadata.ComponentType));
                    }

                    //ensure the same controller context is set
                    controller.ControllerContext = ControllerContext;

                    //check if tree is actually searchable
                    var searchableTree = tree.Value as ISearchableTree;
                    if (searchableTree != null)
                    {
                        //proxy the request to the tree controller
                        return this.ProxyRequestToController(searchableTree, x => x.Search(searchModel.SearchText));
                    }
                    throw new InvalidOperationException("The treeId specified does not map to a ISearchableTree");
                }
            }

            throw new TypeLoadException("Could not find tree id in tree meta data list " + searchModel.TreeId);
        }


        /// <summary>
        /// This will return the root model content from the controller associated with a TreeElement
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected UmbracoTreeResult GetRootResultFromTreeController(ITree t)
        {
            var tree = BackOfficeRequestContext.RegisteredComponents
                .TreeControllers.Where(x => x.Metadata.ComponentType.Equals(t.ControllerType)).SingleOrDefault();

            if (tree != null)
            {
                using (var controller = tree.Value)
                {
                    if (controller == null)
                    {
                        throw new TypeLoadException("Could not create controller: " + UmbracoController.GetControllerName(t.ControllerType));
                    }

                    //proxy the request to the tree controller
                    var rootNodeId = controller.GetRootNodeId();
                    var result = this.ProxyRequestToController(controller, x => x.Index(rootNodeId, new FormCollection()));

                    return result;
                }
            }

            // Give a descriptive error to help the poor sod who has to debug this with a custom tree
            var errorBuilder = new StringBuilder();
            errorBuilder.AppendFormat("Tree {0} with alias {1} was requested, but its type wasn't registered at app startup.\n", t.ControllerType, t.ApplicationAlias);
            errorBuilder.AppendFormat("If the tree type is in a plugin assembly, check it is attributed with the {0}.\n", typeof(AssemblyContainsPluginsAttribute).Name);
            errorBuilder.Append("Here are the types that got registered:\n");
            foreach (var treeController in BackOfficeRequestContext.RegisteredComponents
                .TreeControllers)
            {
                errorBuilder.AppendFormat("• {0} from Assembly {1} found in folder {2}\n", UmbracoController.GetControllerName(treeController.Metadata.ComponentType), treeController.Metadata.ComponentType.Assembly, treeController.Metadata.PluginDefinition.PackageFolderPath);
            }

            throw new TypeLoadException(errorBuilder.ToString());
        }

    }
}
