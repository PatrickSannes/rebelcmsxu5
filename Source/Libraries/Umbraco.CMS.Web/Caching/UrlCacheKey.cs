using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.Caching
{
    using Umbraco.Framework;
    using Umbraco.Framework.Caching;

    public class UrlCacheKey //: CacheKey<UrlCacheKey>
    {
        public UrlCacheKey(HiveId entityId)
        {
            EntityId = entityId;
        }

        public HiveId EntityId { get; set; }
    }
}
