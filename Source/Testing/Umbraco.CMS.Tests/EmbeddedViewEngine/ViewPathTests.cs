using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using NUnit.Framework;
using Umbraco.Cms.Web.EmbeddedViewEngine;

namespace Umbraco.Tests.Cms.EmbeddedViewEngine
{
    [TestFixture]
    public class EmbeddedViewPathTests
    {
        [Test]
        public void Created_ViewPath_Passes_Microsoft_VirtualPath_Parser()
        {
            var path = EmbeddedViewPath.GetFullPath(EmbeddedViewPath.Create("Umbraco.Tests.Cms.EmbeddedViewEngine.TestView.cshtml", "Umbraco.Tests.Cms"));

            try
            {
                var result = BuildManager.CreateInstanceFromVirtualPath(path, typeof(WebViewPage));
            }
            catch (Exception ex)
            {
                //this is the exception we are trying to avoid
                if (ex is ArgumentException && ex.Message.StartsWith("The relative virtual path") && ex.Message.EndsWith("is not allowed here."))
                {
                    throw;
                }
            }

            //no exception was thrown
            Assert.Pass();
        }

    }
}
