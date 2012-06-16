using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Tests.Extensions.Stubs.PropertyEditors
{
    [PropertyEditor("87C14B46-C743-430F-A64D-A269F0C4265A", "TestContentAwarePropertyEditor", "Test Content Aware Property Editor")]
    public class TestContentAwarePropertyEditor : ContentAwarePropertyEditor<TestEditorModel>
    {
        public override TestEditorModel CreateEditorModel()
        {
            return new TestEditorModel();
        }
    }
}