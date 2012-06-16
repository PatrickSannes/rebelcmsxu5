using Umbraco.Framework;

namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// Represents a host name entry
    /// </summary>
    public class HostnameEntry
    {
        public HostnameEntry(string hostname, HiveId contentId, int ordinal)
        {
            Hostname = hostname;
            ContentId = contentId;
            Ordinal = ordinal;
        }

        public string Hostname { get; private set; }
        public HiveId ContentId { get; private set; }
        public int Ordinal { get; private set; }

    }
}