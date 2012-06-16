using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class InsertFieldModel
    {
        public IEnumerable<string> AvailableCasingTypes { get; set; }
        public IEnumerable<string> AvailableEncodingTypes { get; set; }
        public IEnumerable<string> AvailableFields { get; set; }
    }
}
