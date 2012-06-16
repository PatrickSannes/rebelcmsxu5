using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.IO;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class StylesheetHelperTests
    {
        // Standard rule stle
        [TestCase("Test", "p", "font-size: 1em;", @"/*
    Name: Test
*/
p {
    font-size: 1em;
}")]
        // All on one line
        [TestCase("Test", "p", "font-size: 1em;", @"/* Name: Test */ p { font-size: 1em; }")]
        // All on one line with no spaces
        [TestCase("Test", "p", "font-size: 1em;", @"/*Name:Test*/p{font-size: 1em;}")]
        // Every part on a new line
        [TestCase("Test", "p", "font-size: 1em;", @"/* 
Name:
Test
*/
p
{
font-size: 1em;
}")]
        public void StylesheetHelperTests_ParseRules_Parses(string name, string selector, string styles, string css)
        {
            // Arrange
            var file = new File(new HiveId("styles.css"))
            {
                ContentBytes = Encoding.UTF8.GetBytes(css)
            };

            // Act
            var results = StylesheetHelper.ParseRules(file);

            // Assert
            Assert.IsTrue(results.Count() == 1);

            Assert.IsTrue(results.First().RuleId.Value.Value.ToString() == file.Id.Value.Value +"/"+ name);
            Assert.IsTrue(results.First().Name == name);
            Assert.IsTrue(results.First().Selector == selector);
            Assert.IsTrue(results.First().Styles == styles);
        }

        // No Name: keyword
        [TestCase(@"/* Test2 */
p
{
    font-size: 1em;
}")]
        // Has a Name: keyword, but applies to 2 rules, so shouldn't parse
        [TestCase(@"/* Name: Test2 */
p, h2
{
    font-size: 1em;
}")]
        // Has it's name wrapping over two lines
        [TestCase("/* Name: Test\r\n2 */ p { font-size: 1em; }")]
        [TestCase("/* Name: Test\n2 */ p { font-size: 1em; }")]
        public void StylesheetHelperTests_ParseRules_DoesntParse(string css)
        {
            // Arrange
            var file = new File(new HiveId("styles.css"))
            {
                ContentBytes = Encoding.UTF8.GetBytes(css)
            };

            // Act
            var results = StylesheetHelper.ParseRules(file);

            // Assert
            Assert.IsTrue(results.Count() == 0);
        }
    }
}
