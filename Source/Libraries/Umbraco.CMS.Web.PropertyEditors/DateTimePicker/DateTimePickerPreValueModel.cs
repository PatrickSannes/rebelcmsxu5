using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.DateTimePicker
{
    public class DateTimePickerPreValueModel : PreValueModel
    {
        
        public bool ShowTime { get; set; }

        [AllowDocumentTypePropertyOverride]
        public bool IsRequired { get; set; }
    }
}
