using System.Web.Mvc;
using Umbraco.Cms.Packages.SystemInfo.Models;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Surface;
using Umbraco.Framework;

namespace Umbraco.Cms.Packages.SystemInfo.Controllers
{

    [Surface("98625300-6DF0-41AF-A432-83BD0232815A")]
    public class TestSurfaceController : SurfaceController
    {
        public TestSurfaceController(IRoutableRequestContext routableRequestContext)
            : base(routableRequestContext)
        {
        }

        [ChildActionOnly]
        public ActionResult HelloWorld()
        {
            return Content("Hello world");
        }

        [ChildActionOnly]
        public PartialViewResult DisplayForm(string stringToDisplay)
        {
            return PartialView(new TestSurfaceFormModel() {SomeString = stringToDisplay});
        }

        [HttpPost] 
        public ActionResult HandleFormSubmit([Bind(Prefix = "MyTestForm")]TestSurfaceFormModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TestData = "hello";                

                return CurrentUmbracoPage();
            }
            
            //return RedirectToUmbracoPage(HiveId.Parse("content://p__nhibernate/v__guid/00000000000000000000000000001049"));
            TempData["TestData"] = "blah";
            return RedirectToCurrentUmbracoPage();
        }
    }
}