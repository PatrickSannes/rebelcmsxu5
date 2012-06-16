using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.PropertyEditors.Tags;


[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.TextBox.Resources.TextBoxPropertyEditor.js", "application/x-javascript")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.TextBox.Resources.TextBoxPropertyEditor.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.TextBox.Resources.icons.gif", "image/gif")]

namespace Umbraco.Cms.Web.PropertyEditors.TextBox
{
    [PropertyEditor("3F5ED845-7018-4BDE-AB4E-C7106EE0992D", "TextBox", "Text Box", IsParameterEditor = true)]
    public class TextBoxEditor : PropertyEditor<TextBoxEditorModel, TextBoxPreValueModel>
    {
        public TextBoxEditor()
        {
        }

        public override TextBoxEditorModel CreateEditorModel(TextBoxPreValueModel preValues)
        {
            return new TextBoxEditorModel(preValues);
        }

        public override TextBoxPreValueModel CreatePreValueEditorModel()
        {
            return new TextBoxPreValueModel();
        }
    }
}
