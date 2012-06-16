using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    using global::System.Web.Mvc;

    /// <summary>
    /// Represents the properties for any entity that supports authors and parent heirarchy
    /// </summary>
    public abstract class CoreEntityModel : TimestampedModel
    {
        
        [ReadOnly(true)]
        [Display(Name = "CoreEntityModel.CreatedBy")]
        public string CreatedBy { get; set; }

        [ReadOnly(true)]
        [Display(Name = "CoreEntityModel.UpdatedBy")]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the parent id.
        /// </summary>
        /// <value>The parent id.</value>
        /// <remarks></remarks>
        public virtual HiveId ParentId { get; set; }

        /// <summary>
        /// Gets or sets the ordinal relative to the <see cref="ParentId"/>.
        /// </summary>
        /// <value>The ordinal relative to parent.</value>
        public int OrdinalRelativeToParent { get; set; }
    }
}
