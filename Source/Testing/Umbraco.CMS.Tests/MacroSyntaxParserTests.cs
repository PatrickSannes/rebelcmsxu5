using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Macros;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class MacroSyntaxParserTests
    {
        [Test]
        public void MacroSyntaxParser_Matches_Macro_Markers()
        {
            var parser = new MacroSyntaxParser();
            IEnumerable<MacroParserResult> results;
            var output = parser.Parse(@"<p>hello</p>
<div class=""umb-macro-holder"" data-macro-alias=""surface"" >
<!-- start macro --><!-- end macro -->
</div>
<p>asdf</p>
<p>&nbsp;</p>
<div class=""umb-macro-holder"" data-macro-alias=""partial""><!-- start macro -->asdfasdfasdf<!-- end macro --></div>
<!--this is my comment-->",
                          (alias, parameters) => "",
                          out results);

            Assert.AreEqual(2, results.Count());

        }

        [Test]
        public void MacroSyntaxParser_Matches_Macro_Params()
        {
            var parser = new MacroSyntaxParser();
            IEnumerable<MacroParserResult> results;
            var output = parser.Parse(@"<p>hello</p>
<div class=""umb-macro-holder"" data-macro-alias=""surface"" data-macro-params=""eyAiYmxhaCIgOiAiYXNkZmFzZGZhc2RmIiwgImVyd2Vyd2VyIiA6ICIyMzQyMzQyMzQiIH0="">
<!-- start macro --><!-- end macro -->
</div>
<p>asdf</p>
<p>&nbsp;</p>
<div class=""umb-macro-holder"" data-macro-alias=""partial"" data-macro-params=""eyAidGVzdCIgOiAiYXNkZmVlIiB9""><!-- start macro -->asdfasdfasdf<!-- end macro --></div>
<!--this is my comment-->",
                          (alias, parameters) =>
                          {
                              if (alias == "surface")
                              {
                                  Assert.AreEqual(2, parameters.Count);
                                  Assert.AreEqual("asdfasdfasdf", parameters["blah"]);
                                  Assert.AreEqual("234234234", parameters["erwerwer"]);
                              }
                              else
                              {
                                  Assert.AreEqual(1, parameters.Count);
                                  Assert.AreEqual("asdfee", parameters["test"]);
                              }
                              return "";
                          },
                          out results);

            Assert.AreEqual(2, results.Count());

        }

        [Test]
        public void MacroSyntaxParser_Returns_Callback_Data()
        {
            var parser = new MacroSyntaxParser();
            IEnumerable<MacroParserResult> results;
            var output = parser.Parse(@"<p>hello</p>
<div class=""umb-macro-holder"" data-macro-alias=""surface"" data-p-blah=""asdfasdfasdf"" data-p-erwerwer=""234234234"">
<!-- start macro --><!-- end macro -->
</div>
<p>asdf</p>
<p>&nbsp;</p>
<div class=""umb-macro-holder"" data-macro-alias=""partial"" data-p-test=""asdfee""><!-- start macro -->asdfasdfasdf<!-- end macro --></div>
<!--this is my comment-->",
                          (alias, parameters) =>
                          {
                              if (alias == "surface")
                              {
                                  return "macro 1 content";
                              }
                              return "macro 2 content";
                          },
                          out results);

            Assert.AreEqual(@"<p>hello</p>
macro 1 content
<p>asdf</p>
<p>&nbsp;</p>
macro 2 content
<!--this is my comment-->", output);

        }
    }
}
