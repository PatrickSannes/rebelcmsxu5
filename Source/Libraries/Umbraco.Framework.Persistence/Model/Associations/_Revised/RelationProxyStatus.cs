namespace Umbraco.Framework.Persistence.Model.Associations._Revised
{
    /// <summary>
    /// Indicates an origin of a <see cref="RelationProxy"/>.
    /// </summary>
    public enum RelationProxyStatus
    {
        /// <summary>
        /// The <see cref="RelationProxy"/> was automatically loaded from a lazy-loader delegate.
        /// </summary>
        AutoLoaded,
        /// <summary>
        /// The <see cref="RelationProxy"/> was created manually. Indicates that if the entities are saved to the datastore, the relation encompassed by this proxy should be saved.
        /// </summary>
        ManuallyAdded
    }
}