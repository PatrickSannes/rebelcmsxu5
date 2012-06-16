using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.TrueFalse
{
    public class TrueFalseEditorModel : EditorModel
    {
        [ShowLabel(false)]
        public bool Value { get; set; }
    }
}
