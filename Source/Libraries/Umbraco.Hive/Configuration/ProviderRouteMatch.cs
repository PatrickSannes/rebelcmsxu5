using System;

namespace Umbraco.Hive.Configuration
{
    public class ProviderRouteMatch
    {
        private readonly string _wildcardMatch;

        public ProviderRouteMatch(string wildcardMatch)
        {
            _wildcardMatch = wildcardMatch;
        }

        public string WildcardMatch
        {
            get { return _wildcardMatch; }
        }

        public bool IsMatchForUri(Uri match)
        {
            return match.ToString().StartsWith(_wildcardMatch.TrimEnd('*'));
        }
    }
}