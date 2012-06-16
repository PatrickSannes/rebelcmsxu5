using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class InsertPartialModel
    {
        public IEnumerable<SelectListItem> AvailablePartials { get; set; }
    }
}
