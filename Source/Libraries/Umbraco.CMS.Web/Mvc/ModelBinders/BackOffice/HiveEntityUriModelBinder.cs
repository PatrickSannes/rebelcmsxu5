using System.Web.Mvc;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.ModelBinders.BackOffice
{
    /// <summary>
    /// Model binder used to bind the HiveEntityUri parameter
    /// </summary>
    [ModelBinderFor(typeof(HiveEntityUri))]
    public class HiveEntityUriModelBinder : DefaultModelBinder
    {
        /// <summary>
        /// Binds the model
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {

            var idName = string.IsNullOrEmpty(bindingContext.ModelName) ? "id" : bindingContext.ModelName;


            var valueProviderResult = bindingContext.GetValue(idName);
            if (valueProviderResult == null)
            {
                return null;    
            }

            var rawId = valueProviderResult.ConvertTo(typeof(string)) as string;

            HiveEntityUri nodeId = null;
            if (HiveEntityUri.TryParse(rawId, out nodeId))
            {
                //add the bound value to model state if it's not already there, generally only simple props will be there
                if (!bindingContext.ModelState.ContainsKey(idName))
                {
                    bindingContext.ModelState.Add(idName, new ModelState());
                    bindingContext.ModelState.SetModelValue(idName, new ValueProviderResult(nodeId, nodeId.ToString(), null));
                }
            }
            
            return nodeId;
        }    

    }
}
