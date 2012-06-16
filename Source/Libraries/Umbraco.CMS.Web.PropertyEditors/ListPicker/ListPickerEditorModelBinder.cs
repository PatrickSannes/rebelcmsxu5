using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.PropertyEditors.ListPicker
{
    public class ListPickerEditorModelBinder : DefaultModelBinder
    {

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = (ListPickerEditorModel)bindingContext.Model;

            // Force value list to clear out between bindings, as it was remembering previous selection
            // if no entries were selecting making it impossible to clear all selections
            model.Value = new List<string>(); 

            return base.BindModel(controllerContext, bindingContext);
        }

    }
}
