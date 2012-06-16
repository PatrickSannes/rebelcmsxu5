using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class CreateMacroModel
    {

        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public IEnumerable<SelectListItem> AvailableMacroTypes { get; set; }

        [Required]
        public string MacroType { get; set; }

        [Required]
        public string Name { get; set; }

    }
}