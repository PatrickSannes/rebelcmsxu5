using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.PropertyEditors.ListPicker
{
    public enum ListPickerMode
    {
        [Display(Name = "Checkbox list")]
        CheckboxList,

        [Display(Name = "Drop down list")]
        DropDownList,

        [Display(Name = "List box")]
        ListBox,

        [Display(Name = "Radio button list")]
        RadioButtonList

    }
}
