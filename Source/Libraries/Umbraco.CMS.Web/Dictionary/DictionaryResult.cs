using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.Dictionary
{
    public class DictionaryResult
    {
        public DictionaryResult(bool found, string key, string value = "", string sourceLanguage = "")
        {
            Found = found;
            Key = key;
            Value = value;
            SourceLanguage = sourceLanguage;
        }

        public bool Found { get; set; }
        public string Key { get; protected set; }
        public string Value { get; protected set; }
        public string SourceLanguage { get; protected set; }
    }
}
