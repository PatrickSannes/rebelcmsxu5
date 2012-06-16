namespace Umbraco.Cms.Web.Model.BackOffice
{
    public class SearchResultItem
    {
        ///// <summary>
        ///// The id of the search result item
        ///// </summary>
        //public HiveId Id { get; set; }

        /// <summary>
        /// The string representation of the ID, used for Web responses
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name/title of the search result item
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The rank of the search result
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Description/Synopsis of the item
        /// </summary>
        public string Description { get; set; }
    }
}