namespace Umbraco.Framework.Caching
{
    public class CacheEntry<T>
    {
        public CacheEntry(CacheKey key, ICacheValueOf<T> value)
        {
            Key = key;
            Value = value;
        }

        public CacheKey Key { get; protected set; }

        public ICacheValueOf<T> Value { get; protected set; }
    }
}