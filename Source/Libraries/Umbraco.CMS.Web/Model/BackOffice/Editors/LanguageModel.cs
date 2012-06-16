using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// A view model representing hostnames assigned to an entity
    /// </summary>
    [Bind(Include = "Id,IsoCode")]
    public class LanguageModel : DialogModel
    {
        public LanguageModel()
        {
            InstalledLanguages = new List<SelectListItem>();
        }

        /// <summary>
        /// The ID of the content node to assign language
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public HiveId Id { get; set; }

        /// <summary>
        /// The ISO code of the selected language.
        /// </summary>
        /// <value>
        /// The iso code.
        /// </value>
        public string IsoCode { get; set; }

        public IEnumerable<SelectListItem> InstalledLanguages { get; set; }
    }
}