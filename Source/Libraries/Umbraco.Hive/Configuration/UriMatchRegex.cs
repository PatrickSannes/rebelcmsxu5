using System.Text.RegularExpressions;

namespace Umbraco.Hive.Configuration
{
    public class UriMatchRegex
    {
        public UriMatchRegex(WildcardUriMatch uriMatch, Regex regex)
        {
            UriMatch = uriMatch;
            Regex = regex;
        }

        public WildcardUriMatch UriMatch { get; protected set; }
        public Regex Regex { get; protected set; }
    }
}