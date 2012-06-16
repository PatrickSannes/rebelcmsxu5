using System;
using System.Web.UI;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Resources.RTEPreValueEditor.js", "application/x-javascript")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Resources.RichTextBox.js", "application/x-javascript")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Resources.SizeInputField.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Resources.FeaturesCheckboxList.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Resources.StylesheetsCheckboxList.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Resources.FeaturesIcons.gif", "image/gif")]

namespace Umbraco.Cms.Web.PropertyEditors.RichTextBox
{
    [PropertyEditor(CorePluginConstants.RichTextBoxPropertyEditorId, "RichTextBox", "WYSIWYG Editor (RTE)")]
    public class RichTextBoxEditor : ContentAwarePropertyEditor<RichTextBoxEditorModel, RichTextBoxPreValueModel>
    {
        private IUmbracoApplicationContext _appContext;

        public RichTextBoxEditor(IUmbracoApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public override RichTextBoxEditorModel CreateEditorModel(RichTextBoxPreValueModel preValues)
        {
            return new RichTextBoxEditorModel(preValues, _appContext, GetContentModelValue(x => x.Id, HiveId.Empty));
        }

        public override RichTextBoxPreValueModel CreatePreValueEditorModel()
        {
            return new RichTextBoxPreValueModel(_appContext);
        }

    }
}
