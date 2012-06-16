namespace Umbraco.Framework.Caching
{
    using System;
    using System.Runtime.Caching;

    public interface ICachePolicy
    {
        CacheItemPriority GetPriority();

        DateTimeOffset GetExpiryDate();

        //CacheEntryUpdateCallback EntryUpdated { get; set; }

        //CacheEntryRemovedCallback EntryRemoved { get; set; }

        CacheItemPolicy ToCacheItemPolicy();
    }
}