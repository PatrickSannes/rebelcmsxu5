using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Cms.Web.Mvc.Metadata;
using System.ComponentModel.DataAnnotations;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.IO;

using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.PropertyEditors.RichTextBox
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Views.RichTextBoxEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class RichTextBoxEditorModel : EditorModel<RichTextBoxPreValueModel>, IValidatableObject
    {
        private readonly IUmbracoApplicationContext _appContext;

        public RichTextBoxEditorModel(RichTextBoxPreValueModel preValues, IUmbracoApplicationContext appContext, HiveId currNodeId)
            : base(preValues)
        {
            _appContext = appContext;
            CurrentNodeId = currNodeId;
            CreateToolbarButtons();
        }

        /// <summary>
        /// Gets the app context.
        /// </summary>
        public IUmbracoApplicationContext AppContext
        {
            get
            {
                return _appContext;
            }
        }

        
        /// <summary>
        /// Based on the pre-value options, the umbraco label will be toggled
        /// </summary>
        public override bool ShowUmbracoLabel
        {
            get
            {
                return PreValueModel.ShowLabel;
            }
        }


        /// <summary>
        /// The current node id rendering the editor, this is required to render some TinyMCE Plugins but will be null if its new content
        /// </summary>
        public HiveId CurrentNodeId { get; private set; }

        /// <summary>
        /// Gets or sets the RichTextBox value
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [ShowLabel(false)]
        public string Value { get; set; }

        /// <summary>
        /// Gets a list of stylesheets for the RichTextBox
        /// </summary>
        public IEnumerable<File> Stylesheets
        {
            get
            {
                return GetStylesheetFiles();
            }
        }

        /// <summary>
        /// Creates the toolbar buttons.
        /// </summary>
        private void CreateToolbarButtons()
        {
            //create the toolbar buttons
            var uiElements = new List<UIElement>();
            foreach (var b in PreValueModel.Features.Where(x => x.Selected))
            {
                var button = new ButtonUIElement { Alias = b.Value, CssClass = b.Value, Title = b.Text };
                //TODO: add the TinyMCE ...
                uiElements.Add(button);
            }

            UIElements = uiElements;
        }

        /// <summary>
        /// Gets the stylesheet files.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<File> GetStylesheetFiles()
        {
            using (var uow = AppContext.Hive.OpenReader<IFileStore>(new Uri("storage://stylesheets")))
            {
                var stylesheets =
                    uow.Repositories.GetAll<File>().Where(
                        f => PreValueModel.Stylesheets.Any(x => x.Selected && x.Value == f.Id.ToString()));

                return stylesheets.ToArray();
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && string.IsNullOrEmpty(Value))
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
