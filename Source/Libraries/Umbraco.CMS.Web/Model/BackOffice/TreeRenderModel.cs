namespace Umbraco.Cms.Web.Model.BackOffice
{
    /// <summary>
    /// A data model used to do the initial rendering of the tree
    /// </summary>
    public class TreeRenderModel
    {

        /// <summary>
        /// Constructor sets defaults
        /// </summary>
        public TreeRenderModel(string treeUrl)
        {
            TreeUrl = treeUrl;
            TreeHtmlElementId = "mainTree";
            ManuallyInitialize = false;
            ShowContextMenu = true;
        }

        public TreeRenderModel(string treeUrl, string treeHtmlElementId)
        {
            TreeUrl = treeUrl;
            TreeHtmlElementId = treeHtmlElementId;
            ManuallyInitialize = false;
        }

        /// <summary>
        /// The Url for the tree to retreive it's JSON data from
        /// </summary>
        public string TreeUrl { get; private set; }

        /// <summary>
        /// The DOM element ID to render the tree
        /// </summary>
        public string TreeHtmlElementId { get; set; }

        public bool ShowContextMenu { get; set; }

        /// <summary>
        /// A flag determining if the tree's createJsTree() method willl be called automatically or not
        /// </summary>
        public bool ManuallyInitialize { get; set; }

    }
}
