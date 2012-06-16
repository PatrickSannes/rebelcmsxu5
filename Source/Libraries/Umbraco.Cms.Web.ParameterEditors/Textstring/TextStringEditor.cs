using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;

namespace Umbraco.Cms.Web.ParameterEditors.Textstring
{
    [ParameterEditor("3BFB364A-ABDB-43CC-A75A-9AEA679512C2", "Textstring", "Textstring", "3F5ED845-7018-4BDE-AB4E-C7106EE0992D")]
    public class TextstringEditor : ParameterEditor
    {
        public TextstringEditor(IPropertyEditorFactory propertyEditorFactory) 
            : base(propertyEditorFactory)
        { }

        // Not sure if this should be a string, or should be the property editors actual pre value model type
        public override string PropertyEditorPreValues
        {
            get
            {
                return @"
                    <preValues>
                        <preValue name='Mode'><![CDATA[SingleLine]]></preValue>
                    </preValues>";
            }
        }
    }
}
