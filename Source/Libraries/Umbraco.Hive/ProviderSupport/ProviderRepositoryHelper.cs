using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public static class ProviderRepositoryHelper
    {
        internal static IReadonlyRelation<IRelatableEntity, IRelatableEntity> SetProviderAliasOnId(ProviderMetadata providerMetadata, IReadonlyRelation<IRelatableEntity, IRelatableEntity> relation)
        {
            if (relation.Source != null)
                SetProviderAliasOnId(providerMetadata, relation.Source);
            if (relation.Destination != null)
                SetProviderAliasOnId(providerMetadata, relation.Destination);
            return relation;
        }

        internal static IRelationById CreateRelationByIdWithProviderId(ProviderMetadata providerMetadata, IRelationById relation)
        {
            if (relation == null) return null;
            return new RelationById(
                CreateMappedProviderId(providerMetadata, relation.SourceId),
                CreateMappedProviderId(providerMetadata, relation.DestinationId),
                relation.Type,
                relation.Ordinal,
                relation.MetaData.ToArray());
        }

        internal static T SetProviderAliasOnId<T>(ProviderMetadata providerMetadata, T entity) where T : class, IReferenceByHiveId
        {
            if (entity == null) return null;
            var items = entity.GetAllIdentifiableItems();
            foreach (var referenceByHiveId in items)
            {
                if (referenceByHiveId != null) referenceByHiveId.Id = CreateMappedProviderId(providerMetadata, referenceByHiveId.Id);
            }
            return entity;
        }

        internal static HiveId CreateMappedProviderId(ProviderMetadata providerMetadata, HiveId id)
        {
            // We should only override the provider id, if this is not a passthrough provider AND if the provider id is not already set
            string providerId = null;
            if (providerMetadata != null)
                if (!providerMetadata.IsPassthroughProvider && string.IsNullOrEmpty(id.ProviderId))
                    providerId = providerMetadata.Alias;
                else
                    providerId = id.ProviderId;

            return new HiveId(id.ProviderGroupRoot, providerId, id.Value);
        }

        internal static Revision<T> SetProviderAliasOnId<T>(ProviderMetadata providerMetadata, Revision<T> entity) where T : class, IVersionableEntity
        {
            if (entity == null) return null;
            SetProviderAliasOnId(providerMetadata, entity.Item);
            SetProviderAliasOnId(providerMetadata, entity.MetaData);
            return entity;
        }

        internal static EntitySnapshot<T> SetProviderAliasOnId<T>(ProviderMetadata providerMetadata, EntitySnapshot<T> entity) where T : class, IVersionableEntity
        {
            if (entity == null) return null;
            SetProviderAliasOnId(providerMetadata, entity.Revision);
            return entity;
        }

        internal static Revision<T> SetRelationProxyLazyLoadDelegate<T>(this IReadonlyProviderRevisionRepository<T> providerSession, Revision<T> entity) where T : class, IVersionableEntity
        {
            if (entity == null) return null;
            entity.Item.RelationProxies.LazyLoadDelegate = x =>
                {
                    if (providerSession.RelatedEntitiesLoader == null)
                        throw new NotSupportedException("Dynamically loading relations via a Revision<T> object is not supported because a Revision repository does not have a reference to its parent ICoreRelationsRepository via the RelatedEntitiesDelegate property.");
                    return providerSession.RelatedEntitiesLoader.Invoke(entity.Item.Id);
                };
            return entity;
        }

        internal static EntitySnapshot<T> SetSnapshotProxyLazyLoadDelegate<T>(this IReadonlyProviderRevisionRepository<T> providerSession, EntitySnapshot<T> entity) where T : class, IVersionableEntity
        {
            if (entity == null) return null;
            entity.Revision.Item.RelationProxies.LazyLoadDelegate = x =>
                {
                    if (providerSession.RelatedEntitiesLoader == null)
                        throw new NotSupportedException("Dynamically loading relations via an EntitySnapshot<T> object is not supported because a Revision repository does not have a reference to its parent ICoreRelationsRepository via the RelatedEntitiesDelegate property.");
                    return providerSession.RelatedEntitiesLoader.Invoke(entity.Revision.Item.Id);
                };
            return entity;
        }

        internal static T SetRelationProxyLazyLoadDelegate<T>(this IReadonlyProviderRelationsRepository providerSession, T entity) where T : class, IRelatableEntity
        {
            if (entity == null) return null;
            entity.RelationProxies.LazyLoadDelegate = CreateRelationLazyLoadDelegate(providerSession, entity.Id);
            return entity;
        }

        public static Func<HiveId, RelationProxyBucket> CreateRelationLazyLoadDelegate(IReadonlyProviderRelationsRepository providerSession, HiveId id)
        {
            var idCopy = id;
            return x =>
                {
                    if (!providerSession.CanReadRelations) return new RelationProxyBucket();

                    var parents = providerSession.GetParentRelations(idCopy).Select(y => new RelationById(y.SourceId, y.DestinationId, y.Type, y.Ordinal, y.MetaData.ToArray()));
                    var children = providerSession.GetChildRelations(idCopy).Select(y => new RelationById(y.SourceId, y.DestinationId, y.Type, y.Ordinal, y.MetaData.ToArray()));

                    return new RelationProxyBucket(parents, children);
                };
        }

        internal static IReadonlyRelation<IRelatableEntity, IRelatableEntity> SetRelationProxyLazyLoadDelegate(this IReadonlyProviderRelationsRepository providerSession, IReadonlyRelation<IRelatableEntity, IRelatableEntity> relation)
        {
            if (relation.Source != null)
                providerSession.SetRelationProxyLazyLoadDelegate(relation.Source);
            if (relation.Destination != null)
                providerSession.SetRelationProxyLazyLoadDelegate(relation.Destination);
            return relation;
        }

        internal static IEnumerable<IRelationById> ProcessRelations(this IReadonlyProviderRelationsRepository providerSession, IEnumerable<IRelationById> performGetRelations, ProviderMetadata providerMetadata)
        {
            return performGetRelations == null 
                ? Enumerable.Empty<IReadonlyRelation<IRelatableEntity, IRelatableEntity>>() 
                : performGetRelations.Select(x => CreateRelationByIdWithProviderId(providerMetadata, x));
        }
    }
}