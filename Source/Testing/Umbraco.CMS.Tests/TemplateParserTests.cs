using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Web.Templates;
using Umbraco.Framework.Persistence.Model.IO;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class TemplateParserTests
    {
        [Test]
        public void TemplateParserTests_Pares_Success()
        {
            // Arrange
            var file = new File
            {
                ContentBytes = Encoding.UTF8.GetBytes(@"
                @{
                    Layout = ""_Layout.cshtml"";
                }
                <html>
                    <head>
                        @RenderSection(""Head"")
                    </head>
                    <body>
                        @RenderSection(""Test"", true)
                        @RenderBody()
                    </body>
                </html>
                ")
            };

            // Act
            var result = new TemplateParser().Parse(file);

            // Assert
            Assert.IsTrue(result.Layout == "_Layout.cshtml");
            Assert.IsTrue(result.Sections.Count() == 3);
            Assert.IsTrue(result.Sections.Contains("Head"));
            Assert.IsTrue(result.Sections.Contains("Test"));
            Assert.IsTrue(result.Sections.Contains("Body"));
        }
    }
}
