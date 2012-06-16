using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.Slider
{
	public class SliderPreValueModel : PreValueModel
	{

		public override string GetSerializedValue()
		{
			return (new JavaScriptSerializer()).Serialize(this);
		}

        public override void SetModelValues(string serializedVal)
		{
			if (string.IsNullOrEmpty(serializedVal))
			{
				this.EnableRange = false;
				this.RangeValue = SliderOptionsRange.None;
				this.Value = 50;
				this.Value2 = 0;
				this.MinValue = 0;
				this.MaxValue = 100;
				this.EnableStep = true;
				this.StepValue = 5;
				this.Orientation = SliderOptionsOrientation.Horizontal;

				return;
			}

			var deserialized = (new JavaScriptSerializer()).Deserialize<SliderPreValueModel>(serializedVal);

			// set values
			this.EnableRange = deserialized.EnableRange;
			this.RangeValue = deserialized.RangeValue;
			this.Value = deserialized.Value;
			this.Value2 = deserialized.Value2;
			this.MinValue = deserialized.MinValue;
			this.MaxValue = deserialized.MaxValue;
			this.EnableStep = deserialized.EnableStep;
			this.StepValue = deserialized.StepValue;
			this.Orientation = deserialized.Orientation;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [enable range].
		/// </summary>
		/// <value><c>true</c> if [enable range]; otherwise, <c>false</c>.</value>
		[DisplayName("Enable Range")]
		public bool EnableRange { get; set; }

		/// <summary>
		/// Gets or sets the range value.
		/// </summary>
		/// <value>The range value.</value>
		[DisplayName("Range")]
		[UIHint("EnumDropDownList")]
		public SliderOptionsRange RangeValue { get; set; }

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[DisplayName("Initial Value")]
		public int Value { get; set; }

		/// <summary>
		/// Gets or sets the second value.
		/// </summary>
		/// <value>The second value.</value>
		[DisplayName("Initial Value 2")]
		public int Value2 { get; set; }

		/// <summary>
		/// Gets or sets the min value.
		/// </summary>
		/// <value>The min value.</value>
		[DisplayName("Minimum Value")]
		public int MinValue { get; set; }

		/// <summary>
		/// Gets or sets the max value.
		/// </summary>
		/// <value>The max value.</value>
		[DisplayName("Maximum Value")]
		public int MaxValue { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [enable step].
		/// </summary>
		/// <value><c>true</c> if [enable step]; otherwise, <c>false</c>.</value>
		[DisplayName("Enable Step Increments")]
		public bool EnableStep { get; set; }

		/// <summary>
		/// Gets or sets the step.
		/// </summary>
		/// <value>The step.</value>
		[DisplayName("Step Increments")]
		public int StepValue { get; set; }

		/// <summary>
		/// Gets or sets the orientation.
		/// </summary>
		/// <value>The orientation.</value>
		[DisplayName("Orientation")]
		[UIHint("EnumDropDownList")]
		public SliderOptionsOrientation Orientation { get; set; }
	}
}