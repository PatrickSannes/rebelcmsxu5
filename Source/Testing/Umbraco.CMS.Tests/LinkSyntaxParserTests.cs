using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Framework;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class LinkSyntaxParserTests
    {
        [Test]
        public void LinkSyntaxParser_Parses_Links()
        {
            var parser = new LinkSyntaxParser();
            var output = parser.Parse(@"<p>Lorem <a href=""" + HiveId.Empty.ToString() + @""" title=""ipsum"" data-umbraco-link=""internal"">ipsum</a> dolar <a href=""" + HiveId.Empty.ToString() + @""" title=""sit"" data-umbraco-link=""internal"">sit</a> amet.</p>",
                          (hiveId) => "replacement_link");

            Assert.IsTrue(output == @"<p>Lorem <a href=""replacement_link"" title=""ipsum"" >ipsum</a> dolar <a href=""replacement_link"" title=""sit"" >sit</a> amet.</p>");
        }
    }
}
