using System;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// The model representing a macro parameter
    /// </summary>
    public class MacroParameterDefinitionModel
    {
        /// <summary>
        /// Whether or not to show the parameter editable when inserting into Rich text editor
        /// </summary>
        public bool Show { get; set; }

        /// <summary>
        /// The alias of the parameter
        /// </summary>
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The editor Id to display when inserting parameter into Rich text editor
        /// </summary>
        [Required]
        public Guid ParameterEditorId { get; set; }
    }
}