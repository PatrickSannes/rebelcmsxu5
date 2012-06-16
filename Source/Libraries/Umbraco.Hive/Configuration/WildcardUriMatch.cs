using System;

namespace Umbraco.Hive.Configuration
{
    public class WildcardUriMatch
    {
        public WildcardUriMatch(Uri root)
        {
            Root = root;
        }

        public WildcardUriMatch(string uri)
        {
            Root = new Uri(uri.TrimEnd('*'), UriKind.Absolute);
        }

        public Uri Root { get; protected set; }
    }
}