using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Hive.ProviderGrouping
{
    public static class GroupSessionHelper
    {
        public static T MakeIdsAbsolute<T>(T entity, Uri root) where T : class, IReferenceByHiveId
        {
            if (entity == null) return null;
            var items = entity.GetAllIdentifiableItems();
            foreach (var referenceByHiveId in items)
            {
                if (referenceByHiveId != null) referenceByHiveId.Id = new HiveId(root, referenceByHiveId.Id.ProviderId, referenceByHiveId.Id.Value);
            }
            return entity;
        }

        public static IReadonlyRelation<IRelatableEntity, IRelatableEntity> MakeIdsAbsolute(IReadonlyRelation<IRelatableEntity, IRelatableEntity> relation, Uri root)
        {
            if (relation == null) return null;
            MakeIdsAbsolute(relation.Destination, root);
            MakeIdsAbsolute(relation.Source, root);
            return relation;
        }

        public static IRelationById CreateRelationByAbsoluteId(IRelationById relation, Uri root)
        {
            if (relation == null) return null;
            return new RelationById(
                new HiveId(root, relation.SourceId.ProviderId, relation.SourceId.Value),
                new HiveId(root, relation.DestinationId.ProviderId, relation.DestinationId.Value),
                relation.Type,
                relation.Ordinal,
                relation.MetaData.ToArray());
        }

        public static Revision<T> MakeIdsAbsolute<T>(Revision<T> revision, Uri root) where T : class, IVersionableEntity
        {
            if (revision == null) return null;
            MakeIdsAbsolute(revision.Item, root);
            MakeIdsAbsolute(revision.MetaData, root);
            return revision;
        }

        public static EntitySnapshot<T> MakeIdsAbsolute<T>(EntitySnapshot<T> snapshot, Uri root) where T : class, IVersionableEntity
        {
            if (snapshot == null) return null;
            MakeIdsAbsolute(snapshot.Revision, root);
            return snapshot;
        }

        internal static T SetGroupRelationProxyLazyLoadDelegate<T>(this ICoreReadonlyRelationsRepository providerSession, T entity) where T : class, IRelatableEntity
        {
            if (entity == null) return null;
            entity.RelationProxies.LazyLoadDelegate = CreateGroupRelationLazyLoadDelegate(providerSession, entity.Id);
            return entity;
        }

        public static Func<HiveId, RelationProxyBucket> CreateGroupRelationLazyLoadDelegate(ICoreReadonlyRelationsRepository providerSession, HiveId id)
        {
            var idCopy = id;
            return x =>
            {
                var parents = providerSession.GetParentRelations(idCopy).Select(y => new RelationById(y.SourceId, y.DestinationId, y.Type, y.Ordinal, y.MetaData.ToArray()));
                var children = providerSession.GetChildRelations(idCopy).Select(y => new RelationById(y.SourceId, y.DestinationId, y.Type, y.Ordinal, y.MetaData.ToArray()));

                return new RelationProxyBucket(parents, children);
            };
        }
    }
}