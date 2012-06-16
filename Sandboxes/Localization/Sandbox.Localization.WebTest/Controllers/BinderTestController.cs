using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Umbraco.Foundation.Web.Localization.Mvc;

namespace Sandbox.Localization.WebTest.Controllers
{
    public class Foo
    {
        [Required]        
        [StringLength(10)]
        [RegularExpression(@"[^z]+")]
        public string String{ get; set; }


        
        [Required]
        [LocalizedValidation(typeof(RequiredAttribute), "AnotherString")]
        public string AnotherString { get; set; }


        public int Number { get; set; }
        
        [Range(10, 12), LocalizedValidation(typeof(RangeAttribute), "MessagesTest")]        
        [Required, LocalizedValidation(typeof(RequiredAttribute), "MessagesTest2")]                             
        public int Messages { get; set; }


        public decimal? AnotherNumber { get; set; }

        [Range(3, 5)]        
        [LocalizedValidation(typeof(RangeAttribute), "Stars")]
        public int Stars { get; set; }

        [Range(0, 500)]        
        public int RangedNumber { get; set; }
    }

    public class BinderTestController : Controller
    {
        
        public ActionResult Test(Foo foo)
        {            
            return View(new Foo());
        }        

    }
}
