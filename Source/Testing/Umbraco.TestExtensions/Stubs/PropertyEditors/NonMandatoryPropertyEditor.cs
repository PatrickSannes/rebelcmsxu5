using System;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Tests.Extensions.Stubs.PropertyEditors
{
    [PropertyEditor("746A8F96-1BDF-4D79-A094-D0ED3794B396", "NonMandatoryPropertyEditor", "Non-Mandatory Property")]
    public class NonMandatoryPropertyEditor : PropertyEditor<NonMandatoryPropertyEditor.NonMandatoryEditorModel, NonMandatoryPropertyEditor.NonMandatoryEditorPreValueModel>
    {

        public NonMandatoryPropertyEditor()
        {

        }

        /// <summary>
        /// Constructor allowing overriding values specified in attributes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="alias"></param>
        public NonMandatoryPropertyEditor(Guid id, string name, string @alias)
        {
            Id = id;
            Name = name;
            Alias = alias;
        }

        public override NonMandatoryEditorModel CreateEditorModel(NonMandatoryEditorPreValueModel preValues)
        {
            return new NonMandatoryEditorModel();
        }

        public override NonMandatoryEditorPreValueModel CreatePreValueEditorModel()
        {
            return new NonMandatoryEditorPreValueModel();
        }

        public class NonMandatoryEditorModel : EditorModel
        {
            public string Value { get; set; }            
        }

        public class NonMandatoryEditorPreValueModel : PreValueModel
        {

        }
    }
}
