using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel;

namespace Umbraco.Cms.Web.PropertyEditors.UrlPicker
{
    /// <summary>
    /// Required because of the "Allowed Modes" list box - if none are selected,
    /// nothing is bound
    /// </summary>
    public class UrlPickerPreValueModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            switch (propertyDescriptor.Name)
            {
                case "AllowedModes":
                    var valueName = string.Concat(bindingContext.ModelName, ".", propertyDescriptor.Name);
                    var valueObject = bindingContext.ValueProvider.GetValue(valueName);

                    if (valueObject != null)
                    {
                        // One or more selected - default behaviour is fine
                        base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                    }
                    else
                    {
                        // None selected, blank it out
                        SetProperty(controllerContext, bindingContext, propertyDescriptor, new List<UrlPickerMode>());
                    }

                    break;
                default:
                    base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                    break;
            }
        }
    }
}
