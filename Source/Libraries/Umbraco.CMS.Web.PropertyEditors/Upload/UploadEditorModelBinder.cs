using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;

namespace Umbraco.Cms.Web.PropertyEditors.Upload
{
    public class UploadEditorModelBinder : DefaultModelBinder
    {
      

        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, global::System.ComponentModel.PropertyDescriptor propertyDescriptor)
        {
            switch (propertyDescriptor.Name)
            {
                case "NewFile":
                    var files = controllerContext.HttpContext.Request.Files;
                    HttpPostedFileBase file = null;
                    if (files.Count > 0 && files.AllKeys.Contains(bindingContext.ModelName + "_NewFile"))
                        file = files[bindingContext.ModelName + "_NewFile"];
                    propertyDescriptor.SetValue(bindingContext.Model, file);
                    break;
                default:
                    base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                    break;
            }
        }

        
    }
}
