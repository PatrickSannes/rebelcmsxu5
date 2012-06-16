using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.ModelBinders.BackOffice
{
    public static class ModelBinderExtensions
    {

        /// <summary>
        /// Binds a IEnumerable{SelectList}
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="bindingContext">The binding context.</param>
        /// <param name="propertyDescriptor">The property descriptor.</param>
        public static void BindSelectList(this StandardModelBinder binder, ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            var selectList = (IEnumerable<SelectListItem>)propertyDescriptor.GetValue(bindingContext.Model);
            if (selectList != null)
            {
                //first, need to zero out all selections
                selectList.UnSelectItems();
                binder.SetPropertyValue(controllerContext, bindingContext, propertyDescriptor,
                                        val =>
                                            {
                                                selectList.SelectItems(val.Split(','));
                                                return selectList;
                                            });
            }
        }
    }
}