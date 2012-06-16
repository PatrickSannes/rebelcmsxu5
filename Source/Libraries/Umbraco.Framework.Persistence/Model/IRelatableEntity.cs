using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;

namespace Umbraco.Framework.Persistence.Model
{
    public interface IRelatableEntity : IEntity
    {
        /// <summary>
        /// A store of relation proxies for this entity, to support enlisting relations to this entity.
        /// The relations will not be persisted until the entity is passed to a repository for saving.
        /// If <see cref="RelationProxyCollection.IsConnected"/> is <code>true</code>, this sequence may have
        /// <see cref="RelationProxy"/> objects lazily loaded by enumerating the results of calling <see cref="RelationProxyCollection.LazyLoadDelegate"/>.
        /// </summary>
        /// <value>The relation proxies.</value>
        RelationProxyCollection RelationProxies { get; }
    }
}