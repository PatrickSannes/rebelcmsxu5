using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.PropertyEditors.ListPicker
{
    public class ListPickerPreValueModelBinder : DefaultModelBinder
    {

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = (ListPickerPreValueModel)bindingContext.Model;

            model.Items = new List<ListItem>();

            if (!string.IsNullOrEmpty(controllerContext.HttpContext.Request["items.Index"]))
            {
                foreach (var index in controllerContext.HttpContext.Request["items.Index"].Split(','))
                {
                    var val = controllerContext.HttpContext.Request[string.Format("items[{0}].Value", index)];
                    var id = controllerContext.HttpContext.Request[string.Format("items[{0}].Id", index)];

                    if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(id)) 
                        continue;

                    var itm = new ListItem
                    {
                        Value = val, 
                        Id = id
                    };

                    model.Items.Add(itm);
                }
            }
            return base.BindModel(controllerContext, bindingContext);
        }

    }
}
