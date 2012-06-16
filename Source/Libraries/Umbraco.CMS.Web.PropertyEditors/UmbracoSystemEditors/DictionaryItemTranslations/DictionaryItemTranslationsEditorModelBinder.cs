using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.DictionaryItemTranslations
{
    public class DictionaryItemTranslationsEditorModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var model = (DictionaryItemTranslationsEditorModel)bindingContext.Model;

            var translations = new Dictionary<string, string>();
            foreach (var language in model.Languages)
            {
                var requestKey = "translation_" + language.Key;
                var value = controllerContext.HttpContext.Request[requestKey];

                if(translations.ContainsKey(language.Key))
                {
                    translations[language.Key] = value;
                }
                else
                {
                    translations.Add(language.Key, value);
                }
            }

            model.Translations = translations;

            return base.BindModel(controllerContext, bindingContext);
        }
    }
}
