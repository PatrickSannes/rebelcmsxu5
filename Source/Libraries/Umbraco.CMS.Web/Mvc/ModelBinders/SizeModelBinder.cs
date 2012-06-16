using System.Drawing;
using System.Web.Mvc;
using Umbraco.Cms.Web.DependencyManagement;

namespace Umbraco.Cms.Web.Mvc.ModelBinders
{

    /// <summary>
    /// Model binder for the Size struct
    /// </summary>
    [ModelBinderFor(typeof(Size))]
    public class SizeModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            //need to get the width/height values manually
            if (bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
            {
                int width;
                int height;
                if (int.TryParse(bindingContext.ValueProvider.GetValue(string.Concat(bindingContext.ModelName, ".Width")).AttemptedValue, out width)
                    && int.TryParse(bindingContext.ValueProvider.GetValue(string.Concat(bindingContext.ModelName, ".Height")).AttemptedValue, out height))
                {
                    var size = new Size(width, height);
                    //add the bound value to model state if it's not already there, generally only simple props will be there
                    if (!bindingContext.ModelState.ContainsKey(bindingContext.ModelName))
                    {
                        bindingContext.ModelState.Add(bindingContext.ModelName, new ModelState());
                        bindingContext.ModelState.SetModelValue(bindingContext.ModelName,
                            new ValueProviderResult(size, string.Concat(width, ",", height), null));
                    }
                    return size;
                }
            }
            
            return base.BindModel(controllerContext, bindingContext);
            
        }
    }
}
