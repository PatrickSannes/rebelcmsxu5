using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    [Bind(Exclude = "SchemaId")]
    public class Tab
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public HiveId Id { get; set; }

        /// <summary>
        /// Gets or sets the schema id.
        /// </summary>
        [ReadOnly(true), ScaffoldColumn(false)]
        public HiveId SchemaId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        public int SortOrder { get; set; }
    }
}
