namespace Umbraco.Framework.Caching
{
    using System;
    using System.Runtime.Caching;

    public class StaticCachePolicy : ICachePolicy
    {
        public static StaticCachePolicy CreateDefault()
        {
            return new StaticCachePolicy(TimeSpan.FromMinutes(5));
        }

        public StaticCachePolicy(TimeSpan slidingExpiration)
        {
            _slidingExpiry = slidingExpiration;

            SetFixedAbsoluteExpiry();
        }

        private void SetFixedAbsoluteExpiry()
        {
            _fixedAbsoluteExpiry = GenerateExpiryDate();
        }

        public StaticCachePolicy(CacheItemPolicy fromPolicy)
        {
            _priority = fromPolicy.Priority;
            _slidingExpiry = fromPolicy.SlidingExpiration;
            _absoluteExpiry = fromPolicy.AbsoluteExpiration;
            //EntryUpdated = fromPolicy.UpdateCallback;
            //EntryRemoved = fromPolicy.RemovedCallback;

            SetFixedAbsoluteExpiry();
        }

        private readonly CacheItemPriority _priority = CacheItemPriority.Default;
        public CacheItemPriority GetPriority()
        {
            return _priority;
        }

        private readonly TimeSpan _slidingExpiry = ObjectCache.NoSlidingExpiration;
        private readonly DateTimeOffset _absoluteExpiry = ObjectCache.InfiniteAbsoluteExpiration;
        private DateTimeOffset _fixedAbsoluteExpiry = ObjectCache.InfiniteAbsoluteExpiration;

        public DateTimeOffset GetExpiryDate()
        {
            return _fixedAbsoluteExpiry;
        }

        public bool IsExpired
        {
            get
            {
                return GetExpiryDate() <= DateTimeOffset.Now;
            }
        }

        private DateTimeOffset GenerateExpiryDate()
        {
            if (_absoluteExpiry == ObjectCache.InfiniteAbsoluteExpiration && _slidingExpiry != ObjectCache.NoSlidingExpiration)
            {
                return DateTimeOffset.Now.Add(_slidingExpiry);
            }
            return _absoluteExpiry;
        }

        //public CacheEntryUpdateCallback EntryUpdated { get; set; }

        //public CacheEntryRemovedCallback EntryRemoved { get; set; }

        public CacheItemPolicy ToCacheItemPolicy()
        {
            return new CacheItemPolicy()
                {
                    AbsoluteExpiration = GetExpiryDate(),
                    Priority = GetPriority()
                    //RemovedCallback = EntryRemoved,
                    //UpdateCallback = EntryUpdated
                };
        }
    }
}