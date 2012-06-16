using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// A custom view cache to improve performance of view lookups. 
    /// Reference: 
    /// http://blogs.msdn.com/b/marcinon/archive/2011/08/16/optimizing-mvc-view-lookup-performance.aspx
    /// </summary>
    public class TwoLevelViewCache : IViewLocationCache
    {
        private readonly static object Key = new object();
        private readonly IViewLocationCache _cache;

        public TwoLevelViewCache(IViewLocationCache cache)
        {
            _cache = cache;
        }

        private static IDictionary<string, string> GetRequestCache(HttpContextBase httpContext)
        {
            var d = httpContext.Items[Key] as IDictionary<string, string>;
            if (d == null)
            {
                d = new Dictionary<string, string>();
                httpContext.Items[Key] = d;
            }
            return d;
        }

        public string GetViewLocation(HttpContextBase httpContext, string key)
        {
            var d = GetRequestCache(httpContext);
            string location;
            if (!d.TryGetValue(key, out location))
            {
                location = _cache.GetViewLocation(httpContext, key);
                d[key] = location;
            }
            return location;
        }

        public void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath)
        {
            _cache.InsertViewLocation(httpContext, key, virtualPath);
        }
    }
}