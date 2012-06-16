using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.TinyMCE.InsertMacro
{
    [Bind(Exclude = "AvailableMacroItems")]
    public class SelectMacroModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is new.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is new; otherwise, <c>false</c>.
        /// </value>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the content id.
        /// </summary>
        /// <value>
        /// The content id.
        /// </value>
        public HiveId ContentId { get; set; }

        /// <summary>
        /// Gets or sets the macro alias.
        /// </summary>
        /// <value>
        /// The macro alias.
        /// </value>
        [DisplayName("Macro")]
        [Required]
        public string MacroAlias { get; set; }

        /// <summary>
        /// Gets or sets the available macro items.
        /// </summary>
        /// <value>
        /// The available macro items.
        /// </value>
        [ReadOnly(true)]
        public IEnumerable<SelectListItem> AvailableMacroItems { get; set; }
    }
}
