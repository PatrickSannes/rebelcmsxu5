using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework.Persistence.Model.IO;

namespace Umbraco.Cms.Web.Macros
{
    /// <summary>
    /// A class used to serialize/deserialize a macro
    /// </summary>
    public class MacroSerializer
    {

        public static MacroEditorModel FromFile(File file)
        {
            var model = FromXml(Encoding.UTF8.GetString(file.ContentBytes));
            model.Id = file.Id;
            return model;
        }

        public static MacroEditorModel FromXml(string xmlString)
        {
            var xml = XDocument.Parse(xmlString);
            return new MacroEditorModel
                {
                    Alias = xml.Root.Attribute("alias").Value,
                    Name = xml.Root.Attribute("name").Value,
                    CacheByPage = (bool) xml.Root.Attribute("cacheByPage"),
                    CachePeriodSeconds = (int) xml.Root.Attribute("cachePeriodSeconds"),
                    CachePersonalized = (bool) xml.Root.Attribute("cachePersonalized"),
                    MacroType = xml.Root.Attribute("macroType").Value,
                    RenderContentInEditor = (bool) xml.Root.Attribute("renderContentInEditor"),
                    SelectedItem = xml.Root.Attribute("selectedItem").Value,
                    UseInEditor = (bool) xml.Root.Attribute("useInEditor"),
                    MacroParameters = xml.Root.Elements("parameter")
                        .Select(x => new MacroParameterDefinitionModel
                            {
                                Alias = (string) x.Attribute("alias"),
                                Name = (string)x.Attribute("name"),
                                ParameterEditorId = Guid.Parse((string)x.Attribute("parameterEditorId")),
                                Show = (bool)x.Attribute("show")
                            }).ToList()
                };
        }

        public static XDocument ToXml(MacroEditorModel model)
        {
            return new XDocument(
                new XElement("macro",                             
                             new XAttribute("alias", model.Alias),
                             new XAttribute("name", model.Name),
                             new XAttribute("cacheByPage", model.CacheByPage),
                             new XAttribute("cachePeriodSeconds", model.CachePeriodSeconds),
                             new XAttribute("cachePersonalized", model.CachePersonalized),
                             new XAttribute("macroType", model.MacroType),
                             new XAttribute("renderContentInEditor", model.RenderContentInEditor),
                             new XAttribute("selectedItem", model.SelectedItem),
                             new XAttribute("useInEditor", model.UseInEditor),
                             model.MacroParameters.Select(x => new XElement("parameter",
                                                                            new XAttribute("alias", x.Alias),
                                                                            new XAttribute("name", x.Name),
                                                                            new XAttribute("show", x.Show),
                                                                            new XAttribute("parameterEditorId", x.ParameterEditorId)))));
        }

    }
}
