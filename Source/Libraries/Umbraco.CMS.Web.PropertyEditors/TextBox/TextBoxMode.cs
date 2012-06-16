using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Web.PropertyEditors.TextBox
{
    public enum TextBoxMode
    {
        [Display(Name = "Single line")]
        SingleLine,

        [Display(Name = "Multi line")]
        MultiLine,

        [Display(Name = "Multi line with controls")]
        MultiLineWithControls
    }
}