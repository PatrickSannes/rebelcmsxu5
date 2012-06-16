namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Provides assistance to mapping operations should values keyed by <see cref="HiveId"/> need resolving during mapping operations
    /// such as hydrating a deeper object graph than available in the source of the mapping operation.
    /// </summary>
    /// <remarks></remarks>
    public abstract class AbstractLookupHelper
    {
        public abstract T Lookup<T>(HiveId id) where T : class, IReferenceByGuid;
        public abstract void CacheCreation<T>(T item) where T : class, IReferenceByGuid;
    }
}