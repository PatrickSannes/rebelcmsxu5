using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Macros
{
    /// <summary>
    /// Utility class for parsing macro syntax in string content
    /// </summary>
    public class MacroSyntaxParser
    {

        public static readonly Regex MacroMatch = new Regex(@"(<div.*?class=""umb-macro-holder"".*?>\s*?<!-- start macro -->.*?<!-- end macro -->\s*?</div>)", RegexOptions.Compiled);
        public static readonly Regex AliasMatch = new Regex(@"data-macro-alias=""(.*?)""", RegexOptions.Compiled);
        public static readonly Regex ParamMatch = new Regex(@"data-macro-params=""(.*?)""", RegexOptions.Compiled);

        /// <summary>
        /// Finds a macro placeholder amongst the input content (generally html) and renders the macro content into the output.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="macroFoundCallback">Callback that is fired when a macro is found, this callback should return the macro contents based on the callback parameters</param>
        /// <param name="results"></param>
        /// <returns></returns>
        public string Parse(
            string input, 
            Func<string, IDictionary<string, string>, string> macroFoundCallback, 
            out IEnumerable<MacroParserResult> results)
        {
            var output = new List<MacroParserResult>();
            var s = MacroMatch.Replace(input, x =>
                {
                    var strMatch = x.ToString();
                    var r = new MacroParserResult
                        {
                            MacroAlias = AliasMatch.Match(strMatch).Groups[1].Value
                        };

                    if (ParamMatch.IsMatch(strMatch))
                    {
                        var paramsString = ParamMatch.Match(strMatch).Groups[1].Value;
                        var macroParamsDict = paramsString.DecodeMacroParameters();

                        foreach (var macroParam in macroParamsDict)
                        {
                            r.MacroParameters.Add(macroParam.Key, macroParam.Value);
                        }
                    }

                    //now that we have the alias and parameters, we can get the contents
                    r.MacroContents = macroFoundCallback(r.MacroAlias, r.MacroParameters);        
                    output.Add(r);
                    return r.MacroContents;
                });

            results =  output;
            return s;
        }

        

    }
}
