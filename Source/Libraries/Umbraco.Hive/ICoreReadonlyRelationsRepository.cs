using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive
{
    public interface IReadonlyProviderRelationsRepository : ICoreReadonlyRelationsRepository
    {
        /// <summary>
        /// Gets or sets the unit-scoped cache.
        /// </summary>
        /// <value>The unit-scoped cache.</value>
        AbstractScopedCache RepositoryScopedCache { get; set; }

        /// <summary>
        /// Gets the provider metadata.
        /// </summary>
        /// <value>The provider metadata.</value>
        ProviderMetadata ProviderMetadata { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can read relations.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can read relations; otherwise, <c>false</c>.
        /// </value>
        bool CanReadRelations { get; }
    }

    public interface ICoreReadonlyRelationsRepository
    {
        IEnumerable<IRelationById> GetParentRelations(HiveId childId, RelationType relationType = null);
        IEnumerable<IRelationById> GetAncestorRelations(HiveId descendentId, RelationType relationType = null);
        IEnumerable<IRelationById> GetDescendentRelations(HiveId ancestorId, RelationType relationType = null);
        IEnumerable<IRelationById> GetChildRelations(HiveId parentId, RelationType relationType = null);

        /// <summary>
        /// Gets relations on the same branch. A branch is considered to be where elements have the same parent, with the same relation type.
        /// Implementors should also return the relation describing the requested sibling.
        /// </summary>
        /// <param name="siblingId">The sibling id.</param>
        /// <param name="relationType">Type of the relation.</param>
        /// <returns></returns>
        IEnumerable<IRelationById> GetBranchRelations(HiveId siblingId, RelationType relationType = null);

        IRelationById FindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType);
    }
}