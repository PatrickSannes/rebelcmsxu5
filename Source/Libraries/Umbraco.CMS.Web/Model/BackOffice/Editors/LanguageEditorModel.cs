using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class LanguageEditorModel : EditorModel, IModelBindAware
    {
        [Required]
        [DisplayName("Name")]
        public string IsoCode { get; set; }

        public IEnumerable<string> Fallbacks { get; set; }

        public IEnumerable<SelectListItem> InstalledLanguages { get; set; }
        public IEnumerable<SelectListItem> AvailableLanguages { get; set; }

        public LanguageEditorModel()
        {
            Fallbacks = new List<string>();
            InstalledLanguages = new List<SelectListItem>();
            AvailableLanguages = new List<SelectListItem>();

            PopulateUIElements();
        }

        public void BindModel(IModelUpdator modelUpdator)
        {
            modelUpdator.BindModel(this, string.Empty);
        }

        protected void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
        }
    }
}
