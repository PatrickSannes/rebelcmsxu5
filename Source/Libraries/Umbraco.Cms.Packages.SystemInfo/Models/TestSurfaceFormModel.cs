using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Umbraco.Cms.Packages.SystemInfo.Models
{
    [Bind(Exclude="SomeString")]
    public class TestSurfaceFormModel : IValidatableObject
    {
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public string SomeString { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0,4)]
        public int Age { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Name != "shannon")
                yield return new ValidationResult("Name can only be 'shannon'", new[] {"Name"});
        }
    }
}