namespace Umbraco.Framework.Caching
{
    public class CacheCreationResult<T> : CacheModificationResult
    {
        public CacheCreationResult(bool wasUpdated, bool wasInserted, bool alreadyExisted, CacheValueOf<T> value, bool existsButWrongType = false)
            : base(wasUpdated, wasInserted)
        {
            AlreadyExisted = alreadyExisted;
            Value = value;
            ExistsButWrongType = existsButWrongType;
        }

        public bool ExistsButWrongType { get; protected set; }

        public bool AlreadyExisted { get; protected set; }

        public CacheValueOf<T> Value { get; protected set; }
    }
}