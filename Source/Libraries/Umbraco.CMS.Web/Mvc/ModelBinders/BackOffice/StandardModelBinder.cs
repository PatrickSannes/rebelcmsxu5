using System;
using System.ComponentModel;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.ModelBinders.BackOffice
{
    /// <summary>
    /// Model binder to inherit from containing utility functions for aiding in model binding
    /// </summary>
    public abstract class StandardModelBinder : DefaultModelBinder
    {
        /// <summary>
        /// Method to get the attempted value for the property and to execute the callback method to retreive the 'real' value,
        /// then set the property whilst executing the correct events.
        /// The event order is based on how the DefaultModelBinder sets properties so figured I'd go with these 'best practices'.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyDescriptor"></param>
        /// <param name="getValue">callback method to get the 'real' value</param>        
        protected internal void SetPropertyValue(ControllerContext controllerContext,
                                        ModelBindingContext bindingContext,
                                        PropertyDescriptor propertyDescriptor,
                                        Func<string, object> getValue)
        {
            var valueObject = bindingContext.ValueProvider.GetValue(propertyDescriptor.Name);
            if (valueObject != null)
            {
                var val = valueObject.AttemptedValue;
                //get the real value from the callback
                var realVal = getValue.Invoke(val);

                //add the bound value to model state if it's not already there, generally only simple props will be there
                if (!bindingContext.ModelState.ContainsKey(propertyDescriptor.Name))
                {
                    bindingContext.ModelState.Add(propertyDescriptor.Name, new ModelState());
                    bindingContext.ModelState.SetModelValue(propertyDescriptor.Name, new ValueProviderResult(realVal, val, null));
                }

                //set the value
                if (OnPropertyValidating(controllerContext, bindingContext, propertyDescriptor, realVal))
                {
                    SetProperty(controllerContext, bindingContext, propertyDescriptor, realVal);
                    OnPropertyValidated(controllerContext, bindingContext, propertyDescriptor, realVal);
                    return;
                }
            }
        }
    }
}