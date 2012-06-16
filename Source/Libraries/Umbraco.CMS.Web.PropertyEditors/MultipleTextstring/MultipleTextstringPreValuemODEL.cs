using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.MultipleTextstring
{
    public class MultipleTextstringPreValueModel : PreValueModel
    {
        [AllowDocumentTypePropertyOverride]
        public bool IsRequired { get; set; }
    }
}
