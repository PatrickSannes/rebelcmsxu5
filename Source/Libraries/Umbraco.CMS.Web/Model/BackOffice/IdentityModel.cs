using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    /// <summary>
    /// The most basic model representing a name and an Id
    /// </summary>
    public abstract class IdentityModel
    {
        /// <summary>
        /// Gets/sets the Id
        /// </summary>
        [HiddenInput]
        public HiveId Id { get; set; }

        /// <summary>
        /// The name of the model
        /// </summary>
        /// <remarks>
        /// This is the only editable property of CoreEntityModel
        /// </remarks>
        [Required]
        public virtual string Name { get; set; }

     
    }
}