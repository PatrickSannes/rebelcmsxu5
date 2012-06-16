using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;

    public static class RepositoryGroupExtensions
    {
        public static IGroupUnit<IProviderTypeFilter> Create(this GroupUnitFactory factory)
        {
            return factory.Create<IProviderTypeFilter>();
        }

        public static IReadonlyGroupUnit<IProviderTypeFilter> Create(this ReadonlyGroupUnitFactory factory)
        {
            return factory.CreateReadonly<IProviderTypeFilter>();
        }

        internal static void Dispose<T>(this IEnumerable<ICoreReadonlyRepository<T>> readonlyRepositories)
            where T : class, IReferenceByHiveId
        {
            foreach (var readonlyRepository in readonlyRepositories)
            {
                readonlyRepository.Dispose();
            }
        }

        internal static IEnumerable<T> Get<T>(this IEnumerable<IReadonlyProviderRepository<T>> readonlyRepositories, ICoreReadonlyRelationsRepository groupRepo, bool allOrNothing, Uri idRoot, params HiveId[] ids)
            where T : class, IReferenceByHiveId
        {
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            var totalOutput = new HashSet<T>();
            foreach (var reader in readonlyRepositories)
            {
                reader.Get<T>(allOrNothing, ids).SkipWhile(SkipAndMergeFromProviders(reader.ProviderMetadata, totalOutput)).ForEach(x => totalOutput.Add(x));
            }
            return totalOutput.DistinctBy(x => x.Id).Select(x => ProcessIdsAndGroupRelationProxyDelegate(groupRepo, idRoot, x));
        }

        internal static T ProcessIdsAndGroupRelationProxyDelegate<T>(ICoreReadonlyRelationsRepository groupRepo, Uri idRoot, T returnValue)
            where T : class, IReferenceByHiveId
        {
            if (returnValue == null) return null;
            GroupSessionHelper.MakeIdsAbsolute(returnValue, idRoot);
            if (TypeFinder.IsTypeAssignableFrom<IRelatableEntity>(returnValue.GetType()))
            {
                var casted = (IRelatableEntity)returnValue;
                groupRepo.SetGroupRelationProxyLazyLoadDelegate(casted);
            }
            return returnValue;
        }

        internal static IOrderedEnumerable<IRelationById> GetParentRelations(this IEnumerable<IReadonlyProviderRelationsRepository> allProviderRepositories, HiveId childId, Uri idRoot, RelationType relationType = null)
        {
            var totalOutput = new HashSet<IRelationById>();
            foreach (var relationsRepository in allProviderRepositories)
            {
                if (relationsRepository.CanReadRelations)
                {
                    var provider = relationsRepository;
                    var relations = relationsRepository.RepositoryScopedCache.GetOrCreateTyped("rprr_GetParentRelations_" + childId + (relationType != null ? relationType.RelationName : "any_relationtype"),
                        () => provider.GetParentRelations(childId, relationType).ToArray());
                    
                    relations
                        .SkipWhile(SkipAndMergeRelationsFromProviders(relationsRepository.ProviderMetadata, totalOutput))
                        .ForEach(x => totalOutput.Add(x));
                }
            }
            return totalOutput.Distinct().Select(x => GroupSessionHelper.CreateRelationByAbsoluteId(x, idRoot)).OrderBy(x => x.Ordinal);
        }

        internal static IEnumerable<IRelationById> GetAncestorRelations(this IEnumerable<IReadonlyProviderRelationsRepository> allProviderRepositories, HiveId descendentId, Uri idRoot, RelationType relationType = null)
        {
            var totalOutput = new HashSet<IRelationById>();
            foreach (var relationsRepository in allProviderRepositories)
            {
                if (relationsRepository.CanReadRelations)
                {
                    var provider = relationsRepository;
                    var relations = relationsRepository.RepositoryScopedCache.GetOrCreateTyped("rprr_GetAncestorRelations_" + descendentId + (relationType != null ? relationType.RelationName : "any_relationtype"),
                        () => provider.GetAncestorRelations(descendentId, relationType).ToArray());

                    relations
                        .SkipWhile(SkipAndMergeRelationsFromProviders(relationsRepository.ProviderMetadata, totalOutput))
                        .ForEach(x => totalOutput.Add(x));
                }
            }
            return totalOutput.Distinct().Select(x => GroupSessionHelper.CreateRelationByAbsoluteId(x, idRoot));
        }

        internal static IRelationById FindRelation(this IEnumerable<IReadonlyProviderRelationsRepository> sessions, HiveId sourceId, HiveId destinationId, Uri idRoot, RelationType relationType = null)
        {
            IRelationById result = null;
            foreach (var readonlyRelationsRepository in sessions)
            {
                if (readonlyRelationsRepository.CanReadRelations)
                    result = readonlyRelationsRepository.FindRelation(sourceId, destinationId, relationType);
                if (result != null) break;
            }
            return GroupSessionHelper.CreateRelationByAbsoluteId(result, idRoot);
        }

        internal static IOrderedEnumerable<IRelationById> GetChildRelations(this IEnumerable<IReadonlyProviderRelationsRepository> session, HiveId parentId, Uri idRoot, RelationType relationType = null)
        {
            var totalOutput = new HashSet<IRelationById>();
            foreach (var readonlyRelationsRepository in session)
            {
                if (readonlyRelationsRepository.CanReadRelations)
                    readonlyRelationsRepository.GetChildRelations(parentId, relationType)
                        .SkipWhile(SkipAndMergeRelationsFromProviders(readonlyRelationsRepository.ProviderMetadata, totalOutput))
                        .ForEach(x => totalOutput.Add(x));
            }
            return totalOutput.Distinct().Select(x => GroupSessionHelper.CreateRelationByAbsoluteId(x, idRoot)).OrderBy(x => x.Ordinal);
        }

        internal static IOrderedEnumerable<IRelationById> GetBranchRelations(this IEnumerable<IReadonlyProviderRelationsRepository> session, HiveId siblingId, Uri idRoot, RelationType relationType = null)
        {
            var totalOutput = new HashSet<IRelationById>();
            foreach (var readonlyRelationsRepository in session)
            {
                if (readonlyRelationsRepository.CanReadRelations)
                    readonlyRelationsRepository.GetBranchRelations(siblingId, relationType)
                        .SkipWhile(SkipAndMergeRelationsFromProviders(readonlyRelationsRepository.ProviderMetadata, totalOutput))
                        .ForEach(x => totalOutput.Add(x));
            }
            return totalOutput.Distinct().Select(x => GroupSessionHelper.CreateRelationByAbsoluteId(x, idRoot)).OrderBy(x => x.Ordinal);
        }

        internal static IEnumerable<IRelationById> GetDescendentRelations(this IEnumerable<IReadonlyProviderRelationsRepository> session, HiveId ancestorId, Uri idRoot, RelationType relationType = null)
        {
            var totalOutput = new HashSet<IRelationById>();
            foreach (var readonlyRelationsRepository in session)
            {
                if (readonlyRelationsRepository.CanReadRelations)
                    readonlyRelationsRepository.GetDescendentRelations(ancestorId, relationType)
                        .SkipWhile(SkipAndMergeRelationsFromProviders(readonlyRelationsRepository.ProviderMetadata, totalOutput))
                        .ForEach(x => totalOutput.Add(x));
            }
            return totalOutput.Distinct().Select(x => GroupSessionHelper.CreateRelationByAbsoluteId(x, idRoot));
        }

        internal static void AddRelation(this IEnumerable<ICoreRelationsRepository> session, IReadonlyRelation<IRelatableEntity, IRelatableEntity> relation, Uri idRoot)
        {
            foreach (var relationsRepository in session)
            {
                relationsRepository.AddRelation(relation);
            }
            GroupSessionHelper.MakeIdsAbsolute(relation, idRoot);
        }

        internal static void RemoveRelation(this IEnumerable<ICoreRelationsRepository> session, IRelationById relation)
        {
            foreach (var relationsRepository in session)
            {
                relationsRepository.RemoveRelation(relation);
            }
        }

        private static Func<IRelationById, bool> SkipAndMergeRelationsFromProviders(ProviderMetadata providerMetadata, IEnumerable<IRelationById> totalOutput)
        {
            return x => x == null || totalOutput.Any(y => providerMetadata.IsPassthroughProvider ? y.EqualsIgnoringProviderId(x) : y.Equals(x));
        }

        internal static bool Exists<T>(this IEnumerable<IReadonlyProviderRepository<T>> readonlyRepositoryies, HiveId id)
            where T : class, IReferenceByHiveId
        {
            return readonlyRepositoryies.Any(readonlyProviderRepository => readonlyProviderRepository.Exists<T>(id));
        }

        public static IEnumerable<T> ExecuteMany<T>(this IEnumerable<IReadonlyEntityRepository> readonlyRepositories, ICoreReadonlyRelationsRepository groupRepo, Uri idRoot, QueryDescription query, ObjectBinder objectBinder)
            where T : class, IReferenceByHiveId
        {
            var totalOutput = new HashSet<T>();
            foreach (var reader in readonlyRepositories)
            {
                reader.ExecuteMany<T>(query, objectBinder).SkipWhile(SkipAndMergeFromProviders(reader.ProviderMetadata, totalOutput)).ForEach(x => totalOutput.Add(x));
            }
            return totalOutput.DistinctBy(x => x.Id).Select(x => ProcessIdsAndGroupRelationProxyDelegate(groupRepo, idRoot, x));
        }

        internal static IEnumerable<T> GetAll<T>(this IEnumerable<IReadonlyProviderRepository<T>> readonlyRepositories, ICoreReadonlyRelationsRepository groupRepo, Uri idRoot)
            where T : class, IReferenceByHiveId
        {
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            var totalOutput = new HashSet<T>();
            foreach (var reader in readonlyRepositories)
            {
                reader.GetAll<T>().SkipWhile(SkipAndMergeFromProviders(reader.ProviderMetadata, totalOutput)).ForEach(x => totalOutput.Add(x));
            }
            return totalOutput.DistinctBy(x => x.Id).Select(x => ProcessIdsAndGroupRelationProxyDelegate(groupRepo, idRoot, x));
        }

        internal static void Dispose<T>(this IEnumerable<IReadonlyProviderRevisionRepository<T>> readonlyRepositories)
            where T : class, IVersionableEntity
        {
            foreach (var readonlyRepository in readonlyRepositories)
            {
                readonlyRepository.Dispose();
            }
        }

        internal static void Add<T>(this IEnumerable<IProviderRevisionRepository<T>> repositories, Revision<T> item, Uri idRoot)
            where T : class, IVersionableEntity
        {
            foreach (var revisionRepository in repositories)
            {
                revisionRepository.AddOrUpdate(item);
            }
            GroupSessionHelper.MakeIdsAbsolute(item, idRoot);
        }

        internal static Revision<T> Get<T>(this IEnumerable<IReadonlyProviderRevisionRepository<T>> readonlyRepositories, HiveId entityId, HiveId revisionId, Uri idRoot)
            where T : class, IVersionableEntity
        {
            Revision<T> returnValue = null;
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            foreach (var reader in readonlyRepositories)
            {
                returnValue = reader.Get<T>(entityId, revisionId);
                if (returnValue != null) break;
            }
            return GroupSessionHelper.MakeIdsAbsolute(returnValue, idRoot);
        }

        internal static IEnumerable<Revision<T>> GetAll<T>(this IEnumerable<IReadonlyProviderRevisionRepository<T>> readonlyRepositories, Uri idRoot)
            where T : class, IVersionableEntity
        {
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            var totalOutput = new HashSet<Revision<T>>();
            foreach (var reader in readonlyRepositories)
            {
                reader.GetAll<T>().SkipWhile(SkipAndMerge(reader, totalOutput)).ForEach(x => totalOutput.Add(x));
            }
            return totalOutput.DistinctBy(x => x.MetaData.Id).Select(x => GroupSessionHelper.MakeIdsAbsolute(x, idRoot));
        }

        internal static IEnumerable<Revision<T>> GetAll<T>(this IEnumerable<IReadonlyProviderRevisionRepository<T>> readonlyRepositories, HiveId entityId, Uri idRoot, RevisionStatusType revisionStatusType = null)
            where T : class, IVersionableEntity
        {
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            var totalOutput = new HashSet<Revision<T>>();
            foreach (var reader in readonlyRepositories)
            {
                reader.GetAll<T>(entityId).SkipWhile(SkipAndMerge(reader, totalOutput)).ForEach(x => totalOutput.Add(x));
            }
            return totalOutput.DistinctBy(x => x.MetaData.Id).Select(x => GroupSessionHelper.MakeIdsAbsolute(x, idRoot));
        }

        internal static EntitySnapshot<T> GetLatestSnapshot<T>(this IEnumerable<IReadonlyProviderRevisionRepository<T>> readonlyRepositories, HiveId hiveId, Uri idRoot, RevisionStatusType revisionStatusType = null)
            where T : class, IVersionableEntity
        {
            EntitySnapshot<T> returnValue = null;
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            foreach (var reader in readonlyRepositories)
            {
                returnValue = reader.GetLatestSnapshot<T>(hiveId, revisionStatusType);
                if (returnValue != null) break;
            }
            return GroupSessionHelper.MakeIdsAbsolute(returnValue, idRoot);
        }

        internal static Revision<T> GetLatestRevision<T>(this IEnumerable<IReadonlyProviderRevisionRepository<T>> readonlyRepositories, HiveId hiveId, Uri idRoot, RevisionStatusType revisionStatusType = null)
            where T : class, IVersionableEntity
        {
            Revision<T> returnValue = null;
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            foreach (var reader in readonlyRepositories)
            {
                returnValue = reader.GetLatestRevision<T>(hiveId, revisionStatusType);
                if (returnValue != null) break;
            }
            return GroupSessionHelper.MakeIdsAbsolute(returnValue, idRoot);
        }

        internal static IEnumerable<Revision<TEntity>> GetLatestRevisions<TEntity>(this IEnumerable<IReadonlyProviderRevisionRepository<TEntity>> readonlyRepositories, bool allOrNothing, Uri idRoot, RevisionStatusType revisionStatusType = null, params HiveId[] entityIds) 
            where TEntity : class, IVersionableEntity
        {
            Revision<TEntity> returnValue = null;
            var totalOutput = new HashSet<Revision<TEntity>>();
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            foreach (var reader in readonlyRepositories)
            {
                reader.GetLatestRevisions<TEntity>(allOrNothing, revisionStatusType, entityIds).SkipWhile(SkipAndMerge(reader, totalOutput)).ForEach(x => totalOutput.Add(x));
            }
            return totalOutput.DistinctBy(x => x.MetaData.Id).Select(x => GroupSessionHelper.MakeIdsAbsolute(x, idRoot));
        }

        private static Func<Revision<T>, bool> SkipAndMerge<T>(IReadonlyProviderRevisionRepository<T> reader, IEnumerable<Revision<T>> totalOutput)
            where T : class, IVersionableEntity
        {
            return x => x == null || totalOutput.Any(y => reader.ProviderMetadata.IsPassthroughProvider ? y.MetaData.Id.EqualsIgnoringProviderId(x.MetaData.Id) : y.MetaData.Id == x.MetaData.Id);
        }

        internal static void AddOrUpdate<T>(this IEnumerable<ICoreRepository<T>> repositories, T item, Uri idRoot)
            where T : class, IReferenceByHiveId
        {
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            foreach (var repository in repositories)
            {
                repository.AddOrUpdate(item);
            }
            GroupSessionHelper.MakeIdsAbsolute(item, idRoot);
        }

        internal static void Delete<T>(this IEnumerable<ICoreRepository<T>> repositories, HiveId id)
            where T : class, IReferenceByHiveId
        {
            //TODO: Determine which repo is the primary one
            //TODO: Ensure population of the incoming queue is sorted by Ordinal
            foreach (var repository in repositories)
            {
                repository.Delete<T>(id);
            }
        }

        internal static Func<T, bool> SkipAndMergeFromProviders<T>(ProviderMetadata providerMetadata, IEnumerable<T> unionBuilder)
            where T : class, IReferenceByHiveId
        {
            return x => x == null || unionBuilder.Any(y => providerMetadata.IsPassthroughProvider ? y.Id.EqualsIgnoringProviderId(x.Id) : y.Id == x.Id);
        }

        public static IEnumerable<IReadonlyRelation<IRelatableEntity, IRelatableEntity>> GetLazyChildRelations<T>(this ICoreReadonlyRepository<T> readonlyRepository, HiveId parentId, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            var items = readonlyRepository.GetChildRelations(parentId, relationType);
            return items
                .Select(relationById => CreateLazyOrUseExistingRelation(readonlyRepository, relationById));
        }

        private static IReadonlyRelation<IRelatableEntity, IRelatableEntity> CreateLazyOrUseExistingRelation<T>(
            ICoreReadonlyRepository<T> readonlyRepository, IRelationById relationById) where T : class, IRelatableEntity
        {
            // Check if the repository has actually already eagerly loaded the relation in which case we can just return that
            var upCast = relationById as IReadonlyRelation<IRelatableEntity, IRelatableEntity>;
            if (upCast != null && upCast.Source != null && upCast.Destination != null)
                return upCast;

            // Otherwise, return a LazyRelation to load on demand
            return new LazyRelation<T>(
                readonlyRepository,
                relationById.Type,
                relationById.SourceId,
                relationById.DestinationId,
                relationById.Ordinal,
                relationById.MetaData.ToArray());
        }

        public static IEnumerable<IReadonlyRelation<IRelatableEntity, IRelatableEntity>> GetLazyParentRelations<T>(this ICoreReadonlyRepository<T> readonlyRepository, HiveId parentId, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            var items = readonlyRepository.GetParentRelations(parentId, relationType);
            return items
                .Select(relationById => CreateLazyOrUseExistingRelation(readonlyRepository, relationById));
        }

        public static IEnumerable<IReadonlyRelation<IRelatableEntity, IRelatableEntity>> GetLazyAncestorRelations<T>(this ICoreReadonlyRepository<T> readonlyRepository, HiveId descendentId, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            var items = readonlyRepository.GetAncestorRelations(descendentId, relationType);
            return items
                .Select(relationById => CreateLazyOrUseExistingRelation(readonlyRepository, relationById));
        }

        public static IEnumerable<IReadonlyRelation<IRelatableEntity, IRelatableEntity>> GetLazyDescendentRelations<T>(this ICoreReadonlyRepository<T> readonlyRepository, HiveId ascendentId, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            var items = readonlyRepository.GetDescendentRelations(ascendentId, relationType);
            return items
                .Select(relationById => CreateLazyOrUseExistingRelation(readonlyRepository, relationById));
        }
    }
}
