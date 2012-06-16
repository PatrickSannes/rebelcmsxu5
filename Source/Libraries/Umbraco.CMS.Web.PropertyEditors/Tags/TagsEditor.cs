using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.Tags.Resources.jquery.tagsinput.js", "application/x-javascript")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.Tags.Resources.jquery.tagsinput.css", "text/css", PerformSubstitution = true)]

namespace Umbraco.Cms.Web.PropertyEditors.Tags
{
    [PropertyEditor(CorePluginConstants.TagsPropertyEditorId, "Tags", "Tags")]
    public class TagsEditor : PropertyEditor<TagsEditorModel, TagsPreValueModel>
    {
        public override TagsEditorModel CreateEditorModel(TagsPreValueModel preValues)
        {
            return new TagsEditorModel(preValues);
        }

        public override TagsPreValueModel CreatePreValueEditorModel()
        {
            return new TagsPreValueModel();
        }
    }
}
