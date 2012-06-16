using System;
using System.Web.Mvc;

namespace Umbraco.Cms.Web
{
    public static class ModelBindingContextExtensions
    {
        /// <summary>
        /// Returns a value from the value provider given the specified key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bindingContext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T? GetValue<T>(this ModelBindingContext bindingContext, string key) where T : struct
        {
            var valueResult = GetValue(bindingContext, key);

            if (valueResult == null)
            {
                return null;
            }

            try
            {
                return (T?)valueResult.ConvertTo(typeof(T));
            }
            catch (InvalidOperationException)
            {
                //if it can't be cast, return null
                return null;
            }
        }

        public static ValueProviderResult GetValue(this ModelBindingContext bindingContext, string key)
        {
            if (String.IsNullOrEmpty(key)) return null;
            //First, check if the key is the same name as the model name, this means were requesting the exact value
            var fullName = key == bindingContext.ModelName ? key : bindingContext.ModelName + "." + key;
            //Try it with the prefix...
            var valueResult = bindingContext.ValueProvider.GetValue(fullName);
            //Didn't work? Try without the prefix if needed...
            if (valueResult == null && bindingContext.FallbackToEmptyPrefix)
            {
                valueResult = bindingContext.ValueProvider.GetValue(key);
            }

            return valueResult;
        }
    }
}
