using System.Collections.Generic;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using System.Web.Script.Serialization;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.Slider
{
	[EmbeddedView("Umbraco.Cms.Web.PropertyEditors.Slider.Views.SliderEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
	public class SliderEditorModel : EditorModel<SliderPreValueModel>
	{
		public SliderEditorModel(SliderPreValueModel preValueModel)
			: base(preValueModel)
		{
		}		

		public string Value { get; set; }

		public string SliderControl { get; set; }

       
	}
}