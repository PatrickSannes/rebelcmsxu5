namespace Umbraco.Cms.Web.Model.BackOffice
{
    /// <summary>
    /// Used to generate the applications icons/links
    /// </summary>
    public class ApplicationTrayModel
    {

        public string Name { get; set; }
        public string Alias { get; set; }
        public string Icon { get; set; }
        public int Ordinal { get; set; }
        public IconType IconType { get; set; }

        /// <summary>
        /// The dashboard URL for the application
        /// </summary>
        public string DashboardUrl { get; set; }

        /// <summary>
        /// A model representing the tree to load
        /// </summary>
        public TreeRenderModel TreeModel { get; set; }

    }
}
