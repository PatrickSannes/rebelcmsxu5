using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Cms.Web.ParameterEditors.Textarea
{
    [ParameterEditor("BBE74F13-5A57-4371-A027-D8A07BDC3B82", "Textarea", "Textarea", "3F5ED845-7018-4BDE-AB4E-C7106EE0992D")]
    public class TextareaEditor : ParameterEditor
    {
        public TextareaEditor(IPropertyEditorFactory propertyEditorFactory)
            : base(propertyEditorFactory)
        { }

        // Not sure if this should be a string, or should be the property editors actual pre value model type
        public override string PropertyEditorPreValues
        {
            get
            {
                return @"
                    <preValues>
                        <preValue name='Mode'><![CDATA[MultiLine]]></preValue>
                    </preValues>";
            }
        }
    }
}
