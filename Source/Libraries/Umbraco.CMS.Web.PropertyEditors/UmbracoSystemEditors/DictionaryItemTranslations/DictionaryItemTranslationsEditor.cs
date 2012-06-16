using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.DictionaryItemTranslations
{
    [UmbracoPropertyEditor]
    [PropertyEditor(CorePluginConstants.DictionaryItemTranslationsPropertyEditorId, "DictionaryItemTranslationsEditor", "Dictionary Item Translations Editor")]
    public class DictionaryItemTranslationsEditor : PropertyEditor<DictionaryItemTranslationsEditorModel>
    {
        private IUmbracoApplicationContext _appContext;

        public DictionaryItemTranslationsEditor(IUmbracoApplicationContext appContext)
        {
            _appContext = appContext;
        }

        public override DictionaryItemTranslationsEditorModel CreateEditorModel()
        {
            return new DictionaryItemTranslationsEditorModel(_appContext.Settings.Languages.ToDictionary(k => k.IsoCode, v => v.Name));
        }
    }
}
