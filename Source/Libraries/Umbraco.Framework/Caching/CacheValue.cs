namespace Umbraco.Framework.Caching
{
    public class CacheValue : CacheValueOf<object>
    {
        public CacheValue(object value)
            : base(value)
        {
        }

        public CacheValue(object value, ICachePolicy policy)
            : base(value, policy)
        {
        }
    }
}