using System.ComponentModel;
using System.Collections.Generic;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System;
using System.Linq;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.UrlPicker
{
    /// <summary>
    /// Prevalues for the URL Picker - these are settings which won't change for the lifetime of the
    /// URL picker as a JavaScript object, and are required for it's working
    /// </summary>
    [Bind(Exclude = "UniquePropertyId")]
    [ModelBinder(typeof(UrlPickerPreValueModelBinder))]
    public class UrlPickerPreValueModel : PreValueModel, IValidatableObject
    {

        public UrlPickerPreValueModel()
        {
            // Defaults: 
            AllowedModes = new List<UrlPickerMode> { 
                    UrlPickerMode.URL,
                    UrlPickerMode.Content,
                    UrlPickerMode.Media,
                    UrlPickerMode.Upload
                };
        }

        #region Serialization

        public override string GetSerializedValue()
        {
            return (new JavaScriptSerializer()).Serialize(this);
        }

        public override void SetModelValues(string serializedVal)
        {
            if (string.IsNullOrEmpty(serializedVal))
            {
                return;
            }

            var deserialized = (new JavaScriptSerializer()).Deserialize<UrlPickerPreValueModel>(serializedVal);

            AllowedModes = deserialized.AllowedModes;
            DefaultMode = deserialized.DefaultMode;
            DataFormat = deserialized.DataFormat;
            EnableTitle = deserialized.EnableTitle;
            EnableNewWindow = deserialized.EnableNewWindow;
        }

        #endregion

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AllowedModes.Count == 0)
            {
                yield return new ValidationResult("You must select at least one allowed mode");
            }

            if (!AllowedModes.Contains(DefaultMode))
            {
                yield return new ValidationResult("The default mode must be chosen from the allowed modes");
            }
        }

        #region User editable properties

        /// <summary>
        /// Which modes have been allowed for this picker
        /// </summary>
        [DisplayName("Allowed modes")]
        [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.UrlPicker.Views.MultipleUrlPickerModeEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
        public List<UrlPickerMode> AllowedModes { get; set; }

        /// <summary>
        /// The mode which is initally selected
        /// </summary>
        [DisplayName("Default mode")]
        [UIHint("EnumDropDownList")]
        public UrlPickerMode DefaultMode { get; set; }

        /// <summary>
        /// Store as comma seperated or XML
        /// </summary>
        [DisplayName("Data format")]
        [UIHint("EnumDropDownList")]
        public UrlPickerDataFormat DataFormat { get; set; }

        /// <summary>
        /// Whether the user can specify a title
        /// </summary>
        [DisplayName("Enable title field")]
        public bool EnableTitle { get; set; }

        /// <summary>
        /// Whether the user can specify the link to open in a new window
        /// </summary>
        [DisplayName("Enable new window")]
        public bool EnableNewWindow { get; set; }

        #endregion

        #region Hidden properties

        /// <summary>
        /// An integer unique to this instance of the editor
        /// 
        /// Used (at the time of writing) to save files in the correct subfolder under the media folder
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public int? UniquePropertyId { get; set; }

        /// <summary>
        /// Whether the editor is being used entirely client-side
        /// (affects how the data editor is rendered).  Leave this as 'true' if you
        /// are using the data editor as a child of another data editor, or accessing
        /// it solely through the javascript API.
        /// </summary>
        [ScaffoldColumn(false)]
        public bool Standalone { get; set; }

        /// <summary>
        /// URL for an Ajax method which obtains a Content node's URL
        /// </summary>
        [ScaffoldColumn(false)]
        public string ContentNodeUrlMethod { get; set; }

        /// <summary>
        /// URL for an Ajax method which obtains a Media node's URL
        /// </summary>
        [ScaffoldColumn(false)]
        public string MediaNodeUrlMethod { get; set; }

        /// <summary>
        /// URL to the ajax upload handler
        /// </summary>
        [ScaffoldColumn(false)]
        public string AjaxUploadHandlerUrl { get; set; }

        /// <summary>
        /// GUID of the AjaxUploadHander
        /// </summary>
        [ScaffoldColumn(false)]
        public string AjaxUploadHandlerGuid { get; set; }

        #endregion
    }
}
