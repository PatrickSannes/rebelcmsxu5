using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Foundation.Localization.Maintenance;
using Umbraco.Foundation.Localization.Tests.TestPlugin;

namespace Umbraco.Foundation.Localization.Tests
{
    [TestClass]
    public class TextSourceTests
    {
        TextManager _manager;

        [TestInitialize]
        public void Setup()
        {
            _manager = new DefaultTextManager { CurrentLanguage = () => new LanguageInfo { Key = "en-US" }};                 
        }

        [TestMethod]
        public void LoadFromAssembly()
        {
            Assert.AreEqual("Hello", _manager.Get<TextSourceTests>("TestEntry"));
            Assert.IsNull(_manager.Get<TextSourceTests>("NotFound"));
        }

        [TestMethod]
        public void LoadFromOtherAssembly()
        {
            Assert.IsNotNull(_manager.Get<ATestPlugin>("Plugin.Key"));            
        }

        [TestMethod]
        public void TestFactoryGeneratedText()
        {
            Assert.AreEqual("I'm from a factory", _manager.Get<ATestPlugin>("FactoryTest"));            
        }

        [TestMethod]
        public void TestFactoryGeneratedMutatingText()
        {
            //The ultimate test of change events and assembly text factories.
            //When Mutate is called the factory will change one of the texts in the provided text source
            //This will then bubble up through a SimpleTextFactory, then the assembly's inner AggregatingTextSource and then the outer AggregatingTextSource
            Assert.IsNull(_manager.Get<ATestPlugin>("Mutating"));

            MutatingTextSourceFactory.Mutate("Test 1");
            Assert.AreEqual("Test 1", _manager.Get<ATestPlugin>("Mutating"));

            MutatingTextSourceFactory.Mutate("Test 2");
            Assert.AreEqual("Test 2", _manager.Get<ATestPlugin>("Mutating"));            
        }

        [TestMethod]
        public void GetDefaultText()
        {
            Assert.AreEqual("Expected 'Test'", _manager.Get<TextSourceTests>("DefaultExpressionParser.SyntaxError.ExpectedToken", new {
                Token = "Test"
            }));
        }
        
        [TestMethod]
        public void OverrideDefaultText()
        {
            Assert.AreEqual("Override", _manager.Get<TextSourceTests>("DefaultExpressionParser.SyntaxError.UnexpectedToken", new
            {
                Token = "Test"
            }));
        }

        [TestMethod]
        public void DefaultNamespace()
        {
            _manager.DefaultNamespace = _manager.GetNamespace<TextSourceTests>();

            Assert.AreEqual("Hello", _manager.Get("TestEntry"));
        }

        [TestMethod]
        public void MissingDefaultNamespace()
        {            

            Assert.AreNotEqual("Hello", _manager.Get("TestEntry"));
        }

        [TestMethod]
        public void Separation()
        {
            Assert.AreEqual("From plugin", _manager.Get<ATestPlugin>("Plugin.OtherKey"));
            Assert.AreEqual("I'm test 1", _manager.Get<ATestPlugin>("Plugin.SingleTest1"));
            Assert.AreEqual("I'm test 2", _manager.Get<ATestPlugin>("Plugin.SingleTest2"));
            
            Assert.AreEqual("From test", _manager.Get<TextSourceTests>("Plugin.OtherKey"));                                   
        }

        [TestMethod]
        public void MissingTextHandler()
        {
            _manager.MissingTextHandler = (ns, key, lang) => "Missing " + key;
            Assert.AreEqual("Missing foo", _manager.Get("foo"));
        }

        [TestMethod]
        public void MultiLanguageTest()
        {
            _manager.DefaultNamespace = _manager.GetNamespace<TextSourceTests>();
            Assert.AreEqual("Multi", _manager.Get("MultiLanguage"));
            Assert.AreEqual("Multi", _manager.Get("MultiLanguage", language: new LanguageInfo { Key = "da-DK" }));
            Assert.IsNull(_manager.Get("MultiLanguage", language: new LanguageInfo { Key = "pl-PL" }));
        }
       
        [TestMethod]
        public void MissingTextDefaultNull()
        {            
            Assert.IsNull(_manager.Get("foo"));
        }

    }

}
