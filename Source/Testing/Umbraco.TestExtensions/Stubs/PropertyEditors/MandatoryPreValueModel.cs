using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Tests.Extensions.Stubs.PropertyEditors
{


    public class MandatoryPreValueModel : PreValueModel
    {
        [Required]
        public string Value { get; set; }

    }
}
