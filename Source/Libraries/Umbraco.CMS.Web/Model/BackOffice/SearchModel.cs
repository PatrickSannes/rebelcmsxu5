using System;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    public class SearchModel
    {
        /// <summary>
        /// The text to search for
        /// </summary>
        public string SearchText { get; set; }

        /// <summary>
        /// Which tree to search on
        /// </summary>
        public Guid TreeId { get; set; }
    }
}