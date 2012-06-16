using System.Collections.ObjectModel;
using System.Linq;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// A readonly collection of HostnameEntry's
    /// </summary>
    public class ReadonlyHostnameCollection : ReadOnlyCollection<HostnameEntry>
    {
        public ReadonlyHostnameCollection(HostnameCollection list)
            : base(list)
        {

        }

        /// <summary>
        /// Returns all host name entries for the specified content id
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public HostnameEntry[] this[HiveId contentId]
        {
            get { return this.Where(x => x.ContentId == contentId).ToArray(); }
        }

        public bool ContainsContentId(HiveId contentId)
        {
            return this.Any(x => x.ContentId == contentId);
        }

        public bool ContainsHostname(string key)
        {
            return this.Any(x => x.Hostname == key);
        }

        /// <summary>
        /// Returns the host name entry for the specified domain
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public HostnameEntry this[string hostname]
        {
            get { return this.Where(x => x.Hostname == hostname).SingleOrDefault(); }
        }
    }
}