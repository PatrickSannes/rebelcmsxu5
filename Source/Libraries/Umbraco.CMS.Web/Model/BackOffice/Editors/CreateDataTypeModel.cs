using System;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Model representing the create new wizard view for a data type
    /// </summary>
    public class CreateDataTypeModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [UIHint("PropertyEditorDropDown")]
        public Guid PropertyEditorId { get; set; }
    }
}