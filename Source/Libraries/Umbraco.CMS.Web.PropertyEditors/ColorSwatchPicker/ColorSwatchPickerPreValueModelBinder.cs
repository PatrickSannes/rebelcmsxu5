using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.PropertyEditors.ColorSwatchPicker;
namespace Umbraco.Cms.Web.PropertyEditors.ColorSwatchPicker
{
    public class ColorSwatchPickerPreValueModelBinder: DefaultModelBinder
    {

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ((ColorSwatchPickerPreValueModel)bindingContext.Model).Colors = new List<ColorItem>();

            if (!string.IsNullOrEmpty(controllerContext.HttpContext.Request["colors.Index"]))
            {
                foreach (string index in controllerContext.HttpContext.Request["colors.Index"].Split(','))
                {
                    string val = controllerContext.HttpContext.Request[string.Format("colors[{0}].HexValue", index)];
                    if (!string.IsNullOrEmpty(val))
                    {
                        ColorItem col = new ColorItem();
                        col.HexValue = val;
                        ((ColorSwatchPickerPreValueModel)bindingContext.Model).Colors.Add(col);
                    }
                }
            }
            return base.BindModel(controllerContext, bindingContext);
        }
   
    }
}
