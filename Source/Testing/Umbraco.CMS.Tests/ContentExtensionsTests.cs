using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class ContentExtensionsTests
    {
        [Test]
        public void TrySwapTemplate_Swaps_Template_When_Valid_Alt_Template_Alias()
        {
            // Arrange
            var content = CreateDummyContentItem();

            // Act
            var result = content.TrySwapTemplate("Template2");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("Template2", content.CurrentTemplate.Alias);
        }
        
        [Test]
        public void TrySwapTemplate_Swaps_Template_When_Valid_Alt_Template_Alias_CaseInsensitive()
        {
            // Arrange
            var content = CreateDummyContentItem();

            // Act
            var result = content.TrySwapTemplate("TeMpLaTe2");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("Template2", content.CurrentTemplate.Alias);
        }

        [Test]
        public void TrySwapTemplate_Doesnt_Swap_Template_When_Invalid_Alt_Template_Alias()
        {
            // Arrange
            var content = CreateDummyContentItem();

            // Act
            var result = content.TrySwapTemplate("Template5");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("Template1", content.CurrentTemplate.Alias);
        }

        private static Content CreateDummyContentItem()
        {
            var template1 = new Template(new HiveId(Guid.NewGuid()))
            {
                Name = "Template1.html",
                Alias = "Template1"
            };
            var template2 = new Template(new HiveId(Guid.NewGuid()))
            {
                Name = "Template2.html",
                Alias = "Template2"
            };
            var template3 = new Template(new HiveId(Guid.NewGuid()))
            {
                Name = "Template3.html",
                Alias = "Template3"
            };
            var template4 = new Template(new HiveId(Guid.NewGuid()))
            {
                Name = "Template4.html",
                Alias = null
            };

            var content = new Content
            {
                CurrentTemplate = template1,
                AlternativeTemplates = new List<Template> { template1, template2, template3, template4 }
            };

            return content;
        }
    }
}
