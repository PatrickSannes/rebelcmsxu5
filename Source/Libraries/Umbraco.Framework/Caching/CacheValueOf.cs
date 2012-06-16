namespace Umbraco.Framework.Caching
{
    public class CacheValueOf<T> : ICacheValueOf<T>
    {
        public CacheValueOf(T value)
            : this(value, StaticCachePolicy.CreateDefault())
        {
        }

        public CacheValueOf(T value, ICachePolicy policy)
        {
            Item = value;
            Policy = policy;
        }

        public T Item { get; protected set; }

        public ICachePolicy Policy { get; protected set; }
    }
}