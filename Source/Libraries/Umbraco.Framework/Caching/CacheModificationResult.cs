namespace Umbraco.Framework.Caching
{
    public class CacheModificationResult
    {
        public CacheModificationResult(bool wasUpdated, bool wasInserted)
        {
            WasUpdated = wasUpdated;
            WasInserted = wasInserted;
        }

        public bool WasUpdated { get; protected set; }
        public bool WasInserted { get; protected set; }
    }
}