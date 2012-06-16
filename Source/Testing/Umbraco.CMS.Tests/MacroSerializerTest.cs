using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class MacroSerializerTest
    {
        [Test]
        public void MacroSerializer_ToXml()
        {
            var macro = new MacroEditorModel
                {
                    Alias = "test",
                    Name = "Test",
                    Id = new HiveId("my-macro.macro"),
                    CacheByPage = true,
                    CachePeriodSeconds = 1234,
                    CachePersonalized = false,
                    MacroType = "ChildAction",
                    RenderContentInEditor = true,
                    SelectedItem = "RenderTwitterFeed",
                    UseInEditor = false
                };
            macro.MacroParameters.Add(new MacroParameterDefinitionModel { Alias = "test1", Name = "Test1", ParameterEditorId = Guid.NewGuid(), Show = true });
            macro.MacroParameters.Add(new MacroParameterDefinitionModel { Alias = "test2", Name = "Test2", ParameterEditorId = Guid.NewGuid(), Show = false });

            var xml = MacroSerializer.ToXml(macro);

            Assert.AreEqual(macro.Alias, xml.Root.Attribute("alias").Value);
            Assert.AreEqual(macro.Name, xml.Root.Attribute("name").Value);
            Assert.AreEqual(macro.CacheByPage, (bool)xml.Root.Attribute("cacheByPage"));
            Assert.AreEqual(macro.CachePeriodSeconds, (int)xml.Root.Attribute("cachePeriodSeconds"));
            Assert.AreEqual(macro.CachePersonalized, (bool)xml.Root.Attribute("cachePersonalized"));
            Assert.AreEqual(macro.MacroType.ToString(), xml.Root.Attribute("macroType").Value);
            Assert.AreEqual(macro.RenderContentInEditor, (bool)xml.Root.Attribute("renderContentInEditor"));
            Assert.AreEqual(macro.SelectedItem, xml.Root.Attribute("selectedItem").Value);
            Assert.AreEqual(macro.UseInEditor, (bool)xml.Root.Attribute("useInEditor"));
            
            //TODO: test parameter values
            Assert.AreEqual(2, xml.Root.Elements("parameter").Count());
        }

        [Test]
        public void MacroSerializer_FromXml()
        {
            var xml = @"<macro id=""my-macro.macro"" alias=""test"" name=""Test"" cacheByPage=""true"" cachePeriodSeconds=""1234"" cachePersonalized=""false"" macroType=""ChildAction"" renderContentInEditor=""true"" selectedItem=""RenderTwitterFeed"" useInEditor=""false"" />";

            var macro = MacroSerializer.FromXml(xml);

            Assert.AreEqual("test", macro.Alias);
            Assert.AreEqual("Test", macro.Name);
            Assert.AreEqual(true, macro.CacheByPage);
            Assert.AreEqual(1234, macro.CachePeriodSeconds);
            Assert.AreEqual(false, macro.CachePersonalized);
            Assert.AreEqual("ChildAction", macro.MacroType);
            Assert.AreEqual(true, macro.RenderContentInEditor);
            Assert.AreEqual("RenderTwitterFeed", macro.SelectedItem);
            Assert.AreEqual(false, macro.UseInEditor);

            //TODO: Test parameters

        }
    }
}
