using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.DictionaryItemTranslations
{
    [Bind(Exclude = "LanguageCodes")]
    [ModelBinder(typeof(DictionaryItemTranslationsEditorModelBinder))]
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.DictionaryItemTranslations.Views.DictionaryItemTranslationsEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class DictionaryItemTranslationsEditorModel : EditorModel
    {
        public DictionaryItemTranslationsEditorModel(IDictionary<string, string> languages)
        {
            Languages = languages;
        }

        public override bool ShowUmbracoLabel { get { return false; } }

        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public IDictionary<string, string> Languages { get; private set; }

        public IDictionary<string, string> Translations { get; set; }

        public override IDictionary<string, object> GetSerializedValue()
        {
            return Translations.ToDictionary(k => k.Key, v => (object)v.Value);
        }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            Translations = serializedVal.ToDictionary(k => k.Key, v => v.Value.ToString());
        }
    }
}
