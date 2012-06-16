namespace Umbraco.Hive.Configuration
{
    public class ProviderGroupMatchResult
    {
        public ProviderGroupMatchResult(WildcardUriMatch uriMatch, ProviderMappingGroup @group)
        {
            UriMatch = uriMatch;
            Group = @group;
        }

        public WildcardUriMatch UriMatch { get; protected set; }
        public ProviderMappingGroup Group { get; protected set; }
    }
}