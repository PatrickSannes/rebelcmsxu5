using System.Collections.Specialized;
using System.Linq;

namespace Umbraco.Cms.Web
{
    public static class NameValueCollectionExtensions
    {
        public static bool ContainsKey(this NameValueCollection collection, string key)
        {
            return collection.Keys.Cast<string>().Any(x => x == key);
        }
    }
}