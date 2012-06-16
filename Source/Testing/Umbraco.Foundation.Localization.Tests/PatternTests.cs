using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using Umbraco.Foundation.Localization.Maintenance;
using System.Web;
using Umbraco.Foundation.Localization.Parsing;
using Umbraco.Foundation.Localization.Processing;
using Umbraco.Foundation.Localization.Processing.ParameterValues;
using Umbraco.Foundation.Localization.Configuration;

namespace Umbraco.Foundation.Localization.Tests
{
    [TestClass]
    public class PatternTests
    {

        TextManager _manager;
        SimpleTextSource _source;


        [TestInitialize]
        public void Setup()
        {
            _manager = new DefaultTextManager { CurrentLanguage = () => new LanguageInfo { Key = "en-US" } };
            _manager.Texts.Sources.Add(new PrioritizedTextSource(_source = new SimpleTextSource()));
            LocalizationConfig.TextManagerResolver = () => _manager;
        }

        void AddText(string key, string pattern, string language = "en-US", string dialect = "Default")
        {
            _source.Texts.Add(new LocalizedText
            {
                Key = key,
                Pattern = pattern,
                Language = language,
                PatternDialect = dialect
            });            
        }
        
        [TestMethod]
        public void SimpleTest()
        {
            AddText("Test", "Test output");


            Assert.AreEqual("Test output", _manager.Get("Test"));
        }
        

        [TestMethod]
        public void BasicSwitchTest()
        {
            AddText("Test", "It's #Count{1: one | not one}");

            Assert.AreEqual("It's one", _manager.Get("Test", new { Count = 1 }));
            Assert.AreEqual("It's not one", _manager.Get("Test", new { Count = 1.0001 }));
            Assert.AreEqual("It's not one", _manager.Get("Test", new { Count = 0 }));
            Assert.AreEqual("It's not one", _manager.Get("Test", new { Count = 2 }));
            Assert.AreEqual("It's not one", _manager.Get("Test", new { Count = -100 }));
            Assert.AreEqual("It's not one", _manager.Get("Test", new { Count = 100 }));
        }

        [TestMethod]
        public void SwitchTestTwo()
        {
            AddText("Test", "It's #Count{1: one | < 10: less than zero | [10, 15]: between 10 and 15 | something else}");

            Assert.AreEqual("It's one", _manager.Get("Test", new { Count = 1 }));
            Assert.AreEqual("It's less than zero", _manager.Get("Test", new { Count = 9 }));
            Assert.AreEqual("It's less than zero", _manager.Get("Test", new { Count = 9.9999999 }));
            Assert.AreNotEqual("It's less than zero", _manager.Get("Test", new { Count = 10 }));
            Assert.AreEqual("It's less than zero", _manager.Get("Test", new { Count = -10 }));
            Assert.AreEqual("It's between 10 and 15", _manager.Get("Test", new { Count = 10 }));
            Assert.AreEqual("It's between 10 and 15", _manager.Get("Test", new { Count = 14 }));
            Assert.AreEqual("It's between 10 and 15", _manager.Get("Test", new { Count = 15 }));
            Assert.AreEqual("It's something else", _manager.Get("Test", new { Count = 16 }));
            Assert.AreEqual("It's something else", _manager.Get("Test", new { Count = 99.72 }));
        }

        [TestMethod]
        public void NestedSwitchTest()
        {
            AddText("Test", "It's #Count{1: one and #InnerCount{1: one | not one} | not one and #InnerCount{1: one | not one}}");

            Assert.AreEqual("It's one and one", _manager.Get("Test", new { Count = 1, InnerCount = 1 }));
            Assert.AreEqual("It's one and not one", _manager.Get("Test", new { Count = 1, InnerCount = 10 }));
            Assert.AreEqual("It's not one and one", _manager.Get("Test", new { Count = 10, InnerCount = 1 }));
            Assert.AreEqual("It's not one and not one", _manager.Get("Test", new { Count = 10, InnerCount = 2 }));
        }

        /// <summary>
        /// Tests that string.Format is called correctly for different languages
        /// </summary>
        [TestMethod]
        public void StringFormatTest()
        {
            AddText("Test", "String.format says {Number:N2} and {Number:N0}");
            AddText("Test", "String.format says {Number:N2} and {Number:N0}", "da-DK");

            Assert.AreEqual("String.format says 10,000.00 and 10,000", _manager.Get("Test", new { Number = 10000 },
                                                                            language: new LanguageInfo { Key = "en-US" }));

            Assert.AreEqual("String.format says 10.000,00 and 10.000", _manager.Get("Test", new { Number = 10000 },
                                                                            language: new LanguageInfo { Key = "da-DK" }));
        }

        [TestMethod]
        public void MultiParameterTest()
        {
            AddText("Test", "{Number} {String} {Boolean} {AnotherNumber}");

            Assert.AreEqual("10 Test True 42", _manager.Get("Test", new
            {
                Number = 10,
                String = "Test",
                Boolean = true,
                AnotherNumber = 42
            }));
        }

        

        [TestMethod]
        public void EnumTest()
        {
            AddText("Test", @"Items are #Items{0: {#} |, {#}|-1:"" and {#}""}");

            Assert.AreEqual("Items are Item 1", _manager.Get("Test", new
            {
                Items = new[] { "Item 1" }
            }));

            Assert.AreEqual("Items are Item 1, Item 2, Item 3 and Item 4", _manager.Get("Test", new
            {
                Items = new[] { "Item 1", "Item 2", "Item 3", "Item 4" }
            }));

            Assert.AreEqual("Items are Item 1 and Item 2", _manager.Get("Test", new
            {
                Items = new[] { "Item 1", "Item 2" }
            }));

            Assert.AreEqual("Items are Item 1, Item 2 and Item 3", _manager.Get("Test", new
            {
                Items = new[] { "Item 1", "Item 2", "Item 3" }
            }));
        }

        [TestMethod]
        public void PluralTest()
        {
            AddText("Plural1", "1", dialect: "Text");            

            AddText("Test", "You have selected {Count} #Plural(Count){item | items}");

            Assert.AreEqual("You have selected 0 items", _manager.Get("Test", new { Count = 0 }));
            Assert.AreEqual("You have selected 1 item", _manager.Get("Test", new { Count = 1 }));
            Assert.AreEqual("You have selected 90 items", _manager.Get("Test", new { Count = 90 }));
        }

        [TestMethod]
        public void FormatTest()
        {
            AddText("Test", "You are <NameFormat: {Name}>");

            Assert.AreEqual("You are <b>Test</b>", _manager.Get("Test", new { Name = "Test", NameFormat = "<b>{#}</b>" }));                
        }

        [TestMethod]
        public void EncodingTest()
        {
            AddText("Test", @"You are \< <NameFormat: {Name}>");
            _manager.StringEncoder = (x) => HttpUtility.HtmlEncode(x);
            Assert.AreEqual("You are &lt; <b>Tes&lt;&gt;t</b>", _manager.Get("Test", new { Name = "Tes<>t", NameFormat = "<b>{#}</b>" }));
            Assert.AreEqual("You are < <b>Tes<>t</b>", _manager.Get("Test", new { Name = "Tes<>t", NameFormat = "<b>{#}</b>" }, encode:false));
        }

        [TestMethod]
        public void ParameterValueImplicitValueTest()
        {
            var pv = new ParameterValue<int>(8);

            Assert.IsTrue(pv > 5);
        }

        [TestMethod]
        public void PatternTransformTest()
        {
            _manager.Dialects["Default"].PatternTransformer = new HtmlPatternTransformer();
            AddText("Test", @"I <b {Param2}>contain {Value}</b> <!Link: and <i>a format group</i>>");
            
            Assert.AreEqual("I <b 80>contain html</b> <test>and <i>a format group</i></test>",
                _manager.Get("Test", new { Param2 = 80, Value = "html", Link = "<test>{#}</test>" }));
        }

        [TestMethod]
        public void NullTest()
        {
            AddText("Test", "#Value{Yes |? Null}");
            Assert.AreEqual("Yes", _manager.Get("Test", new { Value = "Not null" }));
            Assert.AreEqual("Null", _manager.Get("Test"));
        }

        [TestMethod]
        public void FormatWrapperTest()
        {
            AddText("Test", "Number {Number}");
            AddText("EnumNumberTest", "{@Enum(Values)}");
            AddText("EnumNumberTest2", "#Values:N2{0: {#}|, {#}|-1: \" and {#}\"}");
            AddText("SwitchTest", "#Number{>10: {#} | Less than or equal to ten}");
            _manager.StringEncoder = (x) => HttpUtility.HtmlEncode(x);

            Assert.AreEqual("Number <b>10</b>", _manager.Get("Test",  new { Number = new FormatWrapper(10, "<b>{#}</b>") }));

            Assert.AreEqual("<b>1</b>, <b>2</b> and <b>3</b>", 
                _manager.Get("EnumNumberTest", new { Values = new FormatWrapper(new [] {1, 2, 3}, "<b>{#}</b>") }));

            Assert.AreEqual("<b>1.00</b>, <b>2.00</b> and <b>3.00</b>",
                _manager.Get("EnumNumberTest2", new { Values = new FormatWrapper(new[] { 1, 2, 3 }, "<b>{#}</b>") }));
                        

            Assert.AreEqual("15",
                _manager.Get("SwitchTest", new { Number = 15 }));

            Assert.AreEqual("Less than or equal to ten",
                _manager.Get("SwitchTest", new { Number = 5 }));
        }

        [TestMethod]
        public void CasingTest()
        {
            AddText("Test", "{Value:lowercase} {Value:uppercase} {Value:capitalize-first} {Value:capitalize-all}");
            AddText("TestShortNames", "{Value:lc} {Value:uc} {Value:cf} {Value:ca}");

            AddText("TestShortValues", "{OneChar} {Missing:ca} {OneChar:ca}");            

            Assert.AreEqual("quick fox QUICK FOX Quick fox Quick Fox",
                _manager.Get("Test", new { Value="QuIcK FoX"}));

            Assert.AreEqual("quick fox QUICK FOX Quick fox Quick Fox",
                _manager.Get("TestShortNames", new { Value = "QuIcK FoX" }));

            Assert.AreEqual("a  A",
                _manager.Get("TestShortValues", new { Missing="", OneChar="a" }));
        }

        [TestMethod]
        public void LookupTest()
        {
            AddText("Text1", "Text 1");
            AddText("Text2", "Text 2");
            AddText("Text2Postfix", "Text 2 variant");
            
            AddText("Test1", "{@Text1}");
            AddText("Test2", "{@@Value}");
            AddText("Test3", "{@@Value+Postfix}");

            Assert.AreEqual("Text 1", _manager.Get("Test1"));
            Assert.AreEqual("Text 2", _manager.Get("Test2", new { Value = "Text2" }));
            Assert.AreEqual("Text 2 variant", _manager.Get("Test3", new { Value = "Text2" }));
        }

        [TestMethod]
        public void AllLanguageFallbackTest()
        {
            AddText("Test", "Foo");
            AddText("Test", "Fooz", language: "*");

            AddText("Test2", "Bar", language: "*");

            Assert.AreEqual("Foo", _manager.Get("Test"));
            Assert.AreEqual("Fooz", _manager.Get("Test", language: new LanguageInfo { Key = "da-DK" }));
            Assert.AreEqual("Bar", _manager.Get("Test2"));
            Assert.AreEqual("Bar", _manager.Get("Test2", language: new LanguageInfo { Key = "da-DK" }));            
            Assert.IsNull(_manager.Get("Test3", language: new LanguageInfo { Key = "da-DK" }));
        }

        [TestMethod]
        public void BuildInEnum()
        {
            AddText("Test", @"You have selected {@Enum(Values)}");
            AddText("TestOr", @"You have selected {@Enum(Values, ""or"")}");

            Assert.AreEqual("You have selected foo, bar and baz", _manager.Get("Test", new { Values = new[] { "foo", "bar", "baz" } }));
            Assert.AreEqual("You have selected foo, bar or baz", _manager.Get("TestOr", new { Values = new[] { "foo", "bar", "baz" } }));

            //With format
            AddText("TestFormat", @"You have selected {@Enum(Values:N2)}");            
            Assert.AreEqual("You have selected 2.00, 3.00, 4.00 and 9.00", _manager.Get("TestFormat", new { Values = new[] { 2, 3, 4, 9} }));
         }

        [TestMethod]
        public void BuildInPlural()
        {
            AddText("Test", "#Plural(Value){One|Many}");
            AddText("Test", "#Plural(Value){Case 1|Case 2|Case 3}", language: "pl-PL");

            Assert.AreEqual("One", _manager.Get("Test", new { Value = 1 }));
            Assert.AreEqual("Many", _manager.Get("Test", new { Value = 80 }));

            Assert.AreEqual("Case 1", _manager.Get("Test", new { Value = 1 }, language: new LanguageInfo { Key = "pl-PL" }));
            Assert.AreEqual("Case 2", _manager.Get("Test", new { Value = 3 }, language: new LanguageInfo { Key = "pl-PL" }));
            Assert.AreEqual("Case 2", _manager.Get("Test", new { Value = 23 }, language: new LanguageInfo { Key = "pl-PL" }));
            Assert.AreEqual("Case 3", _manager.Get("Test", new { Value = 27 }, language: new LanguageInfo { Key = "pl-PL" }));
            Assert.AreEqual("Case 3", _manager.Get("Test", new { Value = 13 }, language: new LanguageInfo { Key = "pl-PL" }));
            Assert.AreEqual("Case 2", _manager.Get("Test", new { Value = 123 }, language: new LanguageInfo { Key = "pl-PL" }));
            
        }

        [TestMethod]
        public void ParserException()
        {   
            //               1234567890123456789012345678
            AddText("Test", "This is an { invalid pattern");
            try
            {
                _manager.ParseAllPatterns();
            }
            catch (PatternException e)
            {
                Assert.AreEqual("Error while parsing Test for en-US in namespace \"\". Expected '}' while parsing ParameterSpec at pos 28", e.Message);
                return;
            }

            Assert.Fail();
        }
        
    }
}
