using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.UI.Models
{
    public class MyFunModel : IValidatableObject
    {
        [Required]
        public string Name { get; set; }
        
        [Range(18, 24, ErrorMessage = "You are too old :(")]
        public int Age { get; set; }

        [Range(1000000, double.MaxValue, ErrorMessage = "You do not make enough money :(")]
        public double Income { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Name.InvariantEquals("frank"))
                yield return new ValidationResult("Sorry, women only", new[] {"Name"});
        }
    }
}