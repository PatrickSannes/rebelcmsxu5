using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.PropertyEditors.RichTextBox
{
    /// <summary>
    /// Model binder for the richt text box prevaluemodel which helps the binding of the selected
    /// features and stylesheets check box lists
    /// </summary>
    public class RichTextBoxPreValueModelBinder : DefaultModelBinder
    {

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var model = (RichTextBoxPreValueModel) bindingContext.Model;

            switch (propertyDescriptor.Name)
            {               
                case "Features":
                    var features = model.GetFeatures().ToArray();
                    foreach (var e in features)
                    {
                        //get the values from the value provider for each element
                        var valueName = string.Concat(bindingContext.ModelName, ".", propertyDescriptor.Name, ".", e.Value);
                        var value = bindingContext.ValueProvider.GetValue(valueName);
                        if(value != null)
                            e.Selected = bool.Parse(((string[])value.RawValue)[0]);
                    }
                    propertyDescriptor.SetValue(bindingContext.Model, features);
                    break;
                case "Stylesheets":
                    var stylesheets = model.GetStylesheets().ToArray();
                    foreach (var e in stylesheets)
                    {
                        //get the values from the value provider for each element
                        var valueName = string.Concat(bindingContext.ModelName, ".", propertyDescriptor.Name, ".", e.Value);
                        var value = bindingContext.ValueProvider.GetValue(valueName);
                        if (value != null)
                            e.Selected = bool.Parse(((string[])value.RawValue)[0]);
                    }
                    propertyDescriptor.SetValue(bindingContext.Model, stylesheets);
                    break;
                default:
                    base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                    break;
            }

        }
    }
}