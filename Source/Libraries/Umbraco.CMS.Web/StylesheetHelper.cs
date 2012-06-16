using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model.IO;

namespace Umbraco.Cms.Web
{
    public class StylesheetHelper
    {
        private static string _ruleRegexFormat = @"/\*\s*name:\s*(?<Name>{0}?)\s*\*/\s*(?<Selector>[^\s,{{]*?)\s*{{\s*(?<Styles>.*?)\s*}}";

        public static IEnumerable<StylesheetRule> ParseRules(File input)
        {
            var rules = new List<StylesheetRule>();
            var ruleRegex = new Regex(string.Format(_ruleRegexFormat, @"[^\*\r\n]*"), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            var contents = Encoding.UTF8.GetString(input.ContentBytes);
            var ruleMatches = ruleRegex.Matches(contents);

            foreach (Match match in ruleMatches)
            {
                rules.Add(new StylesheetRule
                {
                    RuleId = new HiveId(new Uri("storage://stylesheets"), string.Empty, new HiveIdValue(input.Id.Value + "/" + match.Groups["Name"].Value)),
                    StylesheetId = input.Id,
                    Name = match.Groups["Name"].Value,
                    Selector = match.Groups["Selector"].Value,
                    // Only match first selector when chained together
                    Styles = string.Join(Environment.NewLine, match.Groups["Styles"].Value.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray())
                });
            }

            return rules;
        }

        public static void ReplaceRule(File input, string oldRuleName, StylesheetRule rule)
        {
            var contents = Encoding.UTF8.GetString(input.ContentBytes);
            var ruleRegex = new Regex(string.Format(_ruleRegexFormat, oldRuleName), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            contents = ruleRegex.Replace(contents, rule != null ? rule.ToString() : "");
            input.ContentBytes = Encoding.UTF8.GetBytes(contents);
        }

        public static void AppendRule(File input, StylesheetRule rule)
        {
            var contents = Encoding.UTF8.GetString(input.ContentBytes);
            contents += Environment.NewLine + Environment.NewLine + rule.ToString();
            input.ContentBytes = Encoding.UTF8.GetBytes(contents);
        }
    }
}
