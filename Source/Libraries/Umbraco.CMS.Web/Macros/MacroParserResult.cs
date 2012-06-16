using System.Collections.Generic;

namespace Umbraco.Cms.Web.Macros
{
    public class MacroParserResult
    {
        public MacroParserResult()
        {
            MacroParameters = new Dictionary<string, string>();
        }

        public string MacroAlias { get; internal set; }
        public string MacroContents { get; internal set; }
        public IDictionary<string, string> MacroParameters { get; private set; }
    }
}