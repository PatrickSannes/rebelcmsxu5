using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.SelectedTemplate
{
    /// <summary>
    /// Represents the editor for the Selected Template in the content editor
    /// </summary>
    [Bind(Exclude = "AvailableTemplates")]
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.SelectedTemplate.Views.SelectedTemplateEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class SelectedTemplateEditorModel : EditorModel
    {
        
        /// <summary>
        /// Gets or sets the template Id selected
        /// </summary>
        /// <value>
        /// The name.
        /// </value>    
        /// <remarks>
        /// (APN) Note that the model's TemplateId property has been changed to HiveId? because currently the modelmetadata
        /// is considering a non-nullable value type to be implicitly required, making it impossible to save content without a template assigned (Sep 11)
        /// </remarks>
        public HiveId? TemplateId { get; set; }
        
        /// <summary>
        /// Gets or sets the available templates.
        /// </summary>
        /// <value>
        /// The available templates.
        /// </value>
        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public IEnumerable<SelectListItem> AvailableTemplates { get; set; }

        public override IDictionary<string, object> GetSerializedValue()
        {
            var val = new Dictionary<string, object>();
            // (APN) Note that the model's TemplateId property has been changed to HiveId? because currently the modelmetadata
            // is considering a non-nullable value type to be implicitly required, making it impossible to save content without a template assigned (Sep 11)
            // (MB) Removed the HiveId.Empty check to allow for "No Template" selections. Please note that there is now a difference between null (no value selected)
            // and HiveId.Empty ("No Template" value selected) (2012/01/18)
            if (TemplateId != null)
            {
                val.Add("TemplateId", TemplateId.ToString());
            }
            return val;
        }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            if (serializedVal.ContainsKey("TemplateId"))
            {
                TemplateId = HiveId.Parse((string)serializedVal["TemplateId"]);
            }
        }

     
    }
}
