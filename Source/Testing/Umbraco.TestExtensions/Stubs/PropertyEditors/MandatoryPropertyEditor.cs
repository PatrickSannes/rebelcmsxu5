using System;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Tests.Extensions.Stubs.PropertyEditors
{
    [PropertyEditor("13786610-171D-4A98-84BC-189357BC6F5B", "MandatoryPropertyEditor", "Mandatory Property")]
    public class MandatoryPropertyEditor : PropertyEditor<TestEditorModel, MandatoryPreValueModel>
    {
        public override TestEditorModel CreateEditorModel(MandatoryPreValueModel preValues)
        {
            return new TestEditorModel();
        }

        public override MandatoryPreValueModel CreatePreValueEditorModel()
        {
            return new MandatoryPreValueModel();
        }

        

    }
}