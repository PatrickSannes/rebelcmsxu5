namespace Umbraco.Framework.Persistence.Model.Associations._Revised
{
    /// <summary>
    /// A functional proxy class for a <see cref="Relation"/> used by <see cref="RelationProxyCollection"/>.
    /// </summary>
    public class RelationProxy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationProxy"/> class.
        /// </summary>
        /// <param name="newRelation">The new relation.</param>
        /// <param name="status">The status.</param>
        public RelationProxy(IReadonlyRelation<IRelatableEntity, IRelatableEntity> newRelation, RelationProxyStatus status)
        {
            Item = newRelation;
            Status = status;
        }

        /// <summary>
        /// Gets or sets the status of the proxy.
        /// </summary>
        /// <value>The status.</value>
        public RelationProxyStatus Status { get; protected set; }

        /// <summary>
        /// Gets or sets the relation.
        /// </summary>
        /// <value>The item.</value>
        public IReadonlyRelation<IRelatableEntity, IRelatableEntity> Item { get; protected set; }
    }
}