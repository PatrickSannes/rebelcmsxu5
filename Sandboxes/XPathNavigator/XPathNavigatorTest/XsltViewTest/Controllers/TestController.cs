using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XsltViewEngine;

namespace XsltViewTest.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/        

        public ActionResult Index()
        {           
            return new XsltView(GetTestData());
        }

        public ActionResult IndexInline()
        {
            var testData = GetTestData();
            ViewBag.XsltBody = new XsltView(testData, "~/Views/Test/Index.xslt").Transform();

            return View("Index");
        }

        Vertex GetTestData()
        {
            var r = new Random(1337);

            var root = new Vertex();
            root.Name = "Root";
            for (int i = 0; i < 10; i++)
            {
                var child = new Vertex();
                child.Name = "Child #" + i;
                for (int j = 0, n = r.Next(0, 10); j < n; j++)
                {
                    var prop = new Property { Name = "Property #" + j, Value = "Value #" + j };
                    child.Properties[prop.Name] = prop;
                }

                root.Children.Add(child);
            }

            return root;
        }
    }
}
