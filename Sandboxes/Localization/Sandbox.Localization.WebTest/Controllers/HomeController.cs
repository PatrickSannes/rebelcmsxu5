using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Foundation.Web.Localization.JavaScript;
using System.IO;

namespace Sandbox.Localization.WebTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";                        

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
