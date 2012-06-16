using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.PropertyEditors.TreeNodePicker
{
    /// <summary>
    /// Model binder for the tree node picker prevaluemodel which helps the binding of the selected tree type
    /// </summary>
    public class TreeNodePickerPreValueModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var model = (TreeNodePickerPreValueModel)bindingContext.Model;

            switch (propertyDescriptor.Name)
            {
                case "AvailableTrees":
                    var trees = model.GetAvailableTrees().ToArray();
                    var valueName = string.Concat(bindingContext.ModelName, ".", propertyDescriptor.Name);
                    var value = bindingContext.ValueProvider.GetValue(valueName);
                    if (value != null)
                    {
                        var item = trees.SingleOrDefault(x => x.Value == value.AttemptedValue);
                        if (item != null)
                            item.Selected = true;
                    }
                    propertyDescriptor.SetValue(bindingContext.Model, trees);
                    break;
                default:
                    base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                    break;
            }

        }
    }
}
