using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Framework.Dynamics;

namespace Umbraco.Cms.Web.Mvc.ModelBinders
{

    /// <summary>
    /// Used to bind a json dictionary to a dictionary parameter of an Action
    /// </summary>
    /// <remarks>
    /// We need this because of a bug with MVC currently which you can see here:
    ///  http://connect.microsoft.com/VisualStudio/feedback/details/636647/make-jsonvalueproviderfactory-work-with-dictionary-types-in-asp-net-mvc
    /// </remarks>
    public class JsonDictionaryModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
            {
                if (controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    //read in the full stream
                    controllerContext.HttpContext.Request.InputStream.Position = 0;
                    using (var sr = new StreamReader(controllerContext.HttpContext.Request.InputStream))
                    {
                        var input = sr.ReadToEnd();
                        var json = JObject.Parse(input);
                        if (json[bindingContext.ModelName] != null)
                        {
                            //convert to dictionary
                            var d = json[bindingContext.ModelName]
                                .Where(i => i.Type == JTokenType.Property)
                                .Cast<JProperty>()
                                .ToDictionary<JProperty, string, object>(p => p.Name, p => (string)p.Value);

                            //add the bound value to model state if it's not already there, generally only simple props will be there
                            if (!bindingContext.ModelState.ContainsKey(bindingContext.ModelName))
                            {
                                bindingContext.ModelState.Add(bindingContext.ModelName, new ModelState());
                                bindingContext.ModelState.SetModelValue(bindingContext.ModelName,
                                    new ValueProviderResult(d, json[bindingContext.ModelName].ToString(), null));
                            }

                            return d;
                        }

                    }
                }    
            }

            

            return base.BindModel(controllerContext, bindingContext);
        }

    }
}
