using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.Slider
{
    [PropertyEditor(CorePluginConstants.SliderPropertyEditorId, "Slider", "Slider")]
	public class SliderEditor : PropertyEditor<SliderEditorModel, SliderPreValueModel>
	{
		public override SliderEditorModel CreateEditorModel(SliderPreValueModel preValues)
		{
			return new SliderEditorModel(preValues);
		}

		public override SliderPreValueModel CreatePreValueEditorModel()
		{
			return new SliderPreValueModel();
		}
	}
}