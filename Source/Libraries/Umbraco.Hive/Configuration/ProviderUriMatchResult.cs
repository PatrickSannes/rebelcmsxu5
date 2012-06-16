namespace Umbraco.Hive.Configuration
{
    public class ProviderUriMatchResult
    {
        public ProviderUriMatchResult(bool success, WildcardUriMatch matchingUri)
        {
            Success = success;
            MatchingUri = matchingUri;
        }

        public bool Success { get; protected set; }
        public WildcardUriMatch MatchingUri { get; protected set; }
    }
}