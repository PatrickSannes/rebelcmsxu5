using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Framework.Persistence.Model.IO;

namespace Umbraco.Cms.Web.Templates
{
    public class TemplateParser
    {
        private Regex _layoutRegex = new Regex(@"@{.*?Layout\s*?=\s*?""([^""]*?)"";.*?}", RegexOptions.Compiled | RegexOptions.Singleline);
        private Regex _namedSectionRegex = new Regex(@"@RenderSection\(""([^""]*?)""", RegexOptions.Compiled | RegexOptions.Singleline);
        private Regex _bodySectionRegex = new Regex(@"@RenderBody\(\)", RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Parses the specified template for a Layout and any RenderSections.
        /// </summary>
        /// <param name="templateFile">The template file.</param>
        /// <returns></returns>
        public TemplateParserResult Parse(File templateFile)
        {
            //TODO: Would probably be better to use the Razor engine to parse but was unable to get it working, so resorted to Regex for now
            var fileContents = Encoding.UTF8.GetString(templateFile.ContentBytes);
            return Parse(fileContents);
        }

        /// <summary>
        /// Parses the specified template for a Layout and any RenderSections.
        /// </summary>
        /// <param name="templateFile">The template file.</param>
        /// <returns></returns>
        public TemplateParserResult Parse(string templateFileContents)
        {
            var layout = "";
            var sections = new List<string>();

            // Parse layout
            var layoutMatch = _layoutRegex.Match(templateFileContents);
            if (layoutMatch.Success)
                layout = layoutMatch.Groups[1].Value;

            // Parse named sections
            var namedSectionMatches = _namedSectionRegex.Matches(templateFileContents);
            foreach (Match namedSectionMatch in namedSectionMatches)
            {
                sections.Add(namedSectionMatch.Groups[1].Value);
            }

            // Parse body section
            if (_bodySectionRegex.IsMatch(templateFileContents))
                sections.Add("Body");

            return new TemplateParserResult
            {
                Layout = layout,
                Sections = sections
            };
        }
    }
}
