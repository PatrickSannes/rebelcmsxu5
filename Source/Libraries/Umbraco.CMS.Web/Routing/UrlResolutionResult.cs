namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// The result that is returned when a URL is generated for an entity
    /// </summary>
    public class UrlResolutionResult
    {
        public UrlResolutionResult(string url, UrlResolutionStatus status)
        {
            Status = status;
            Url = url;
        }

        public string Url { get; private set; }
        public UrlResolutionStatus Status { get; private set; }
    }
}