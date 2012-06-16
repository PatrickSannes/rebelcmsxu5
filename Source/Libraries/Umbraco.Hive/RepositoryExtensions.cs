using System;
using System.Collections.Generic;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using System.Linq;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive
{
    using Umbraco.Framework.Data;

    public static class RepositoryExtensions
    {
        public static TEntity Get<TEntity>(this ICoreReadonlyRepository<TEntity> session, HiveId id)
            where TEntity : class, IReferenceByHiveId
        {
            return session.Get<TEntity>(true, new[] { id }).FirstOrDefault();
        }

        public static CompositeEntitySchema GetComposite<TSchema>(this ICoreReadonlyRepository<AbstractSchemaPart> repo, HiveId id)
            where TSchema : EntitySchema
        {
            var schema = repo.Get<TSchema>(true, new[] { id }).FirstOrDefault();
            if (schema == null) return null;
            var ancestors = repo.Get<TSchema>(true, repo.GetAncestorRelations(id, FixedRelationTypes.DefaultRelationType).Select(x => x.SourceId).ToArray());
            return new CompositeEntitySchema(schema, ancestors);
        }

        public static CompositeEntitySchema GetComposite<TSchema>(this IReadonlyProviderRepository<AbstractSchemaPart> repo, HiveId id)
            where TSchema : EntitySchema
        {
            var schema = repo.Get<TSchema>(id);
            if (schema == null) return null;
            var ancestors = repo.Get<TSchema>(true, repo.GetAncestorRelations(id, FixedRelationTypes.DefaultRelationType).Select(x => x.SourceId).ToArray());
            return new CompositeEntitySchema(schema, ancestors);
        }

        /// <summary>
        /// Returns the collection of ancestor ids
        /// </summary>
        /// <param name="mappingGroup"></param>
        /// <param name="source"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public static EntityPath GetEntityPath<T>(this ICoreReadonlyRepository<T> mappingGroup, T source, RelationType relationType)
            where T : class, IRelatableEntity
        {
            Mandate.ParameterNotNull(source, "source");
            return mappingGroup.GetEntityPath(source.Id, relationType);
        }

        /// <summary>
        /// Returns the collection of ancestor ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingGroup"></param>
        /// <param name="id"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public static EntityPath GetEntityPath<T>(this ICoreReadonlyRepository<T> mappingGroup, HiveId id, RelationType relationType)
            where T : class, IRelatableEntity
        {
            var ancestorOrSelfIds = mappingGroup.GetAncestorsIdsOrSelf(id, relationType);
            return new EntityPath(ancestorOrSelfIds.Reverse());
        }

        /// <summary>
        /// Returns a collection of paths for the entity with the given id
        /// </summary>
        /// <param name="mappingGroup"></param>
        /// <param name="source"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public static EntityPathCollection GetEntityPaths<T>(this ICoreReadonlyRepository<T> mappingGroup, T source, RelationType relationType)
            where T : class, IRelatableEntity
        {
            Mandate.ParameterNotNull(source, "source");
            return mappingGroup.GetEntityPaths(source.Id, relationType);
        }

        /// <summary>
        /// Returns a collection of paths for the entity with the given id 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mappingGroup">The mapping group.</param>
        /// <param name="id">The id.</param>
        /// <param name="relationType">Type of the relation.</param>
        /// <returns></returns>
        public static EntityPathCollection GetEntityPaths<T>(this ICoreReadonlyRepository<T> mappingGroup, HiveId id, RelationType relationType)
            where T : class, IRelatableEntity
        {
            var ancestorRelations = mappingGroup.GetAncestorRelations(id, relationType);
            var pathsList = new List<List<HiveId>> { new List<HiveId>() };

            ProcessPaths(ancestorRelations, id, 0, ref pathsList);

            return new EntityPathCollection(id, pathsList.Select(x => new EntityPath(x)));
        }

        /// <summary>
        /// Recursive method for processing paths from a relations collection
        /// </summary>
        /// <param name="relations">The relations.</param>
        /// <param name="destinationId">The destination id.</param>
        /// <param name="pathIndex">Index of the path.</param>
        /// <param name="paths">The paths.</param>
        private static void ProcessPaths(IEnumerable<IRelationById> relations, HiveId destinationId, int pathIndex, ref List<List<HiveId>> paths)
        {
            // Add the current destination
            paths[pathIndex].Add(destinationId);

            // Process parents
            var parentIds = relations.Where(x => x.DestinationId == destinationId).Select(x => x.SourceId);
            var parentIdsLength = parentIds.Count();
            if (parentIdsLength >= 1)
            {
                // If there is a fork, then clone the current path and add to the newly cloned path
                if (parentIdsLength > 1)
                {
                    for (var i = 1; i < parentIdsLength; i++)
                    {
                        paths.Add(paths[pathIndex].Select(x => HiveId.Parse(x.ToString())).ToList());
                        ProcessPaths(relations, parentIds.ElementAt(i), paths.Count - 1, ref paths);
                    }
                }

                // For the first index, just keep appending to the current path
                ProcessPaths(relations, parentIds.ElementAt(0), pathIndex, ref paths);
            }
            else
            {
                paths[pathIndex].Reverse();
            }
        }

        /// <summary>
        /// Adds/Updates many entities at one time
        /// </summary>
        /// <param name="readWriter"></param>
        /// <param name="entities"></param>
        public static void AddOrUpdate<T>(this ICoreRepository<T> readWriter, IEnumerable<T> entities)
            where T : class, IReferenceByHiveId
        {
            Mandate.ParameterNotNull(entities, "entities");
            entities.ForEach(readWriter.AddOrUpdate);
        }

        /// <summary>
        /// Creates a branch new revision based on the revision passed in with the specified status if one is supplied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readWriter"></param>
        /// <param name="revision">The Revision in which to create a new one from</param>
        /// <param name="status">Default is Draft</param>
        /// <returns>
        /// Returns the newly created revision object
        /// </returns>
        public static Revision<T> AddNew<T>(this ICoreRevisionRepository<TypedEntity> readWriter, Revision<T> revision, RevisionStatusType status = null)
            where T : TypedEntity
        {
            if (status == null)
                status = FixedStatusTypes.Draft;

            var newRev = revision.CopyToNewRevision(status);
            readWriter.AddOrUpdate(newRev);
            return newRev;
        }

        /// <summary>
        /// Adds/Updates many revisions at one time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="readWriter"></param>
        /// <param name="revisions"></param>
        public static void AddOrUpdate<T>(this ICoreRevisionRepository<TypedEntity> readWriter, IEnumerable<Revision<T>> revisions) where T : TypedEntity
        {
            Mandate.ParameterNotNull(revisions, "revisions");
            revisions.ForEach(readWriter.AddOrUpdate);
        }

        public static T GetEntityByPath<T>(this IReadonlyEntityRepositoryGroup<IProviderTypeFilter> repositoryGroup, HiveId sourceId, string path, bool acceptNearestMatch = false)
            where T : TypedEntity
        {
            return repositoryGroup.GetEntityByPath<T>(sourceId, path, FixedStatusTypes.Published, acceptNearestMatch);
        }

        public static T GetEntityByPath<T>(this IReadonlyEntityRepositoryGroup<IProviderTypeFilter> repositoryGroup, HiveId sourceId, string path, RevisionStatusType revisionStatusType, bool acceptNearestMatch = false)
            where T : TypedEntity
        {
            T found = null;

            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                //TODO: We need to enable more on the query context to do this properly, however there seems 
                // to be a limitation on our business models since there's no current way to query by a version
                // since the query context only exposes TypedEntity and not revisions.
                // For now, this is going to be a really nasty lookup to do what we want.

                //var test1 =
                //    repositoryGroup.ToList().Select(
                //        x =>
                //        new
                //            {
                //                Entity = x,
                //                Attribs = x.Attributes.Select(y => new { y.AttributeDefinition.Alias, Values = y.Values.ToList() }).ToArray()
                //            })
                //        .ToArray();

                var entityRevisionsByAlias = repositoryGroup
                    .Where(x => x.Attribute<string>(NodeNameAttributeDefinition.AliasValue, "UrlName") == part ||
                        ((x.Attribute<string>(NodeNameAttributeDefinition.AliasValue, "UrlName") == null || x.Attribute<string>(NodeNameAttributeDefinition.AliasValue, "UrlName") == string.Empty)
                        && x.Attribute<string>(NodeNameAttributeDefinition.AliasValue, "Name") == part))
                    .ToArray();

                var childRelations =
                    repositoryGroup.GetChildRelations(
                        sourceId, FixedRelationTypes.DefaultRelationType)
                        .Select(x => x.DestinationId)
                        .ToArray();

                var entityWithCorrectRelation = entityRevisionsByAlias.FirstOrDefault(x => childRelations.Contains(x.Id));

                if (entityWithCorrectRelation == null)
                    continue;

                //TODO: until the above issue is resolved, we actually need to lookup the entity individually by revision status
                var foundWithRevisionStatus = repositoryGroup.Revisions.GetLatestSnapshot<TypedEntity>(entityWithCorrectRelation.Id, revisionStatusType);
                if (foundWithRevisionStatus == null || foundWithRevisionStatus.Revision == null || foundWithRevisionStatus.Revision.Item == null)
                    continue;

                found = (T)foundWithRevisionStatus.Revision.Item;

                sourceId = found.Id;
            }

            if (found != null && (found.Attribute<string>(NodeNameAttributeDefinition.AliasValue, "UrlName").InvariantEquals(path.Split('/').Last()) || acceptNearestMatch))
                return found;

            return null;
        }

        public static IQueryable<T> QueryChildEntitiesByRelationType<T>(this IEntityRepositoryGroup<IProviderTypeFilter> repository, RelationType relationType, HiveId sourceId)
            where T : TypedEntity, new()
        {
            var childIds = repository.GetChildRelations(sourceId, relationType).Select(x => x.DestinationId).ToArray();
            return repository.Query<T>().InIds(childIds);
        }

        public static IEnumerable<T> GetEntityByRelationType<T>(this ICoreReadonlyRepository<TypedEntity> repository, RelationType relationType, HiveId sourceId, params RelationMetaDatum[] metaDatum)
            where T : TypedEntity, new()
        {
            var relations = repository.GetChildRelations(sourceId, relationType);
            var destinationIds = relations.Select(x => x.DestinationId).ToArray();
            return repository.Get<T>(true, destinationIds);

            //var destinations = relations.Select(x => x.Destination); // Cannot cast to T here because caller may have asked for a subclass of T
            //// TODO: Consider ways to improve this, such as passing T to GetChildRelations for it to use the TypeMappers collection
            //foreach (var relatableEntity in destinations)
            //{
            //    if (typeof(TypedEntity).IsAssignableFrom(typeof(T)))
            //    {
            //        var toReturn = new T();
            //        var toReturnCasted = toReturn as TypedEntity;
            //        toReturnCasted.SetupFromEntity(relatableEntity as TypedEntity);
            //        yield return toReturn;
            //    }
            //    else
            //    {
            //        var item = relatableEntity as T;
            //        if (item != null) yield return item;
            //    }
            //}
        }

        public static IEnumerable<T> GetEntityByRelationType<T>(this ICoreReadonlyRepository<AbstractSchemaPart> repository, RelationType relationType, HiveId sourceId, params RelationMetaDatum[] metaDatum)
            where T : AbstractSchemaPart, new()
        {
            var relations = repository.GetChildRelations(sourceId, relationType);
            var destinationIds = relations.Select(x => x.DestinationId).ToArray();
            return repository.Get<T>(true, destinationIds);

            //var relations = repository.GetChildRelations(sourceId, relationType);
            ////var destinationIds = relations.Select(x => x.DestinationId).ToArray();
            //var destinations = relations.Select(x => x.Destination); // Cannot cast to T here because caller may have asked for a subclass of T
            //// TODO: Consider ways to improve this, such as passing T to GetChildRelations for it to use the TypeMappers collection
            //foreach (var relatableEntity in destinations)
            //{
            //    if (typeof(TypedEntity).IsAssignableFrom(typeof(T)))
            //    {
            //        var toReturn = new T();
            //        var toReturnCasted = toReturn as TypedEntity;
            //        toReturnCasted.SetupFromEntity(relatableEntity as TypedEntity);
            //        yield return toReturn;
            //    }
            //    else
            //    {
            //        var item = relatableEntity as T;
            //        if (item != null) yield return item;
            //    }
            //}
        }

        public static void AddRelation(this ICoreRelationsRepository session, IRelatableEntity source, IRelatableEntity destination, AbstractRelationType relationType, int ordinal, params RelationMetaDatum[] metaData)
        {
            session.AddRelation(new Relation(relationType, source, destination, ordinal, metaData));
        }

        public static void AddRelation(this ICoreRelationsRepository session, HiveId sourceId, HiveId destinationId, AbstractRelationType relationType, int ordinal, params RelationMetaDatum[] metaData)
        {
            session.AddRelation(new Relation(relationType, sourceId, destinationId, ordinal, metaData));
        }

        //public static IEnumerable<IRelatableEntity> GetAncestorsOrSelf<T>(this IEntityRepositoryGroup<IProviderTypeFilter> session, T source, RelationType relationType = null)
        //    where T : TypedEntity
        //{
        //    Mandate.ParameterNotNull(source, "source");
        //    var ancestorRelationSourceIds = session.GetAncestorRelations(source.Id, relationType).Select(x => x.SourceId).ToArray();
        //    var ancestors = session.Get<T>(true, ancestorRelationSourceIds);
        //    return new[] {source}.Union(ancestors);
        //}

        //public static IEnumerable<IRelatableEntity> GetAncestorsOrSelf<T>(this ISchemaRepositoryGroup<IProviderTypeFilter> session, T source, RelationType relationType = null)
        //    where T : AbstractSchemaPart
        //{
        //    Mandate.ParameterNotNull(source, "source");
        //    var ancestorRelationSourceIds = session.GetAncestorRelations(source.Id, relationType).Select(x => x.SourceId).ToArray();
        //    var ancestors = session.Get<T>(true, ancestorRelationSourceIds);
        //    return new[] { source }.Union(ancestors).Cast<IRelatableEntity>();
        //}

        public static IEnumerable<IRelatableEntity> GetAncestorsOrSelf<T>(this ICoreReadonlyRepository<T> session, T source, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            Mandate.ParameterNotNull(source, "source");
            return GetAncestorsOrSelf(session, source.Id, relationType);
        }

        public static IEnumerable<IRelatableEntity> GetAncestorsOrSelf<T>(this ICoreReadonlyRepository<T> session, HiveId sourceId, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            var source = session.Get<T>(sourceId);
            var ancestorRelationSourceIds = GetAncestorRelationSourceIds(session, relationType, source.Id);
            var ancestors = session.Get<T>(true, ancestorRelationSourceIds);
            return new[] { source }.Union(ancestors);
        }

        public static IEnumerable<IRelatableEntity> GetAncestors<T>(this ICoreReadonlyRepository<T> session, HiveId sourceId, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            var ancestorRelationSourceIds = GetAncestorRelationSourceIds(session, relationType, sourceId);
            var ancestors = session.Get<T>(true, ancestorRelationSourceIds);
            return ancestors;
        }

        public static IEnumerable<HiveId> GetAncestorsIdsOrSelf<T>(this ICoreReadonlyRepository<T> session, HiveId sourceId, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            var ancestorRelationSourceIds = GetAncestorRelationSourceIds(session, relationType, sourceId);
            return new[] { sourceId }.Union(ancestorRelationSourceIds);
        }

        public static IEnumerable<HiveId> GetAncestorIds<T>(this ICoreReadonlyRepository<T> session, HiveId sourceId, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            return GetAncestorRelationSourceIds(session, relationType, sourceId);
        }

        private static HiveId[] GetAncestorRelationSourceIds(this ICoreReadonlyRelationsRepository session, RelationType relationType, HiveId sourceId)
        {
            return session.GetAncestorRelations(sourceId, relationType).Select(x => x.SourceId).ToArray();
        }

        public static HiveId[] GetDescendantIds(this ICoreReadonlyRelationsRepository session, HiveId sourceId, RelationType relationType)
        {
            return session.GetDescendentRelations(sourceId, relationType).Select(x => x.DestinationId).ToArray();
        }

        public static IEnumerable<IRelationById> GetParentRelations(this ICoreReadonlyRelationsRepository session, IRelatableEntity child, RelationType relationType = null)
        {
            Mandate.ParameterNotNull(child, "child");
            return session.GetParentRelations(child.Id, relationType);
        }

        public static void ChangeOrCreateRelationMetadata(this ICoreRelationsRepository session, HiveId withSourceId, HiveId withDestinationId, RelationType withRelationType, params RelationMetaDatum[] newMetadata)
        {
            var findRelation = session.FindRelation(withSourceId, withDestinationId, withRelationType) ??
                               new Relation(withRelationType, withSourceId, withDestinationId, 0, newMetadata);

            findRelation.MetaData.Clear();
            newMetadata.ForEach(x => findRelation.MetaData.Add(x));
            session.ChangeRelation(findRelation, findRelation.SourceId, findRelation.DestinationId);
        }

        /// <summary>
        /// Changes an existing relation's source, destination and ordinal
        /// </summary>
        /// <param name="session"></param>
        /// <param name="originalSourceId"></param>
        /// <param name="originalDestinationId"></param>
        /// <param name="withRelationType"></param>
        /// <param name="newSourceId"></param>
        /// <param name="newDestinationId"></param>
        /// <param name="newOrdinal"></param>
        public static void ChangeRelation(this ICoreRelationsRepository session, HiveId originalSourceId, HiveId originalDestinationId, RelationType withRelationType, HiveId newSourceId, HiveId newDestinationId, int newOrdinal = int.MinValue)
        {
            var findRelation = session.FindRelation(originalSourceId, originalDestinationId, withRelationType);
            if (findRelation != null)
            {
                session.ChangeRelation(findRelation, newSourceId, newDestinationId, newOrdinal);
            }
        }

        /// <summary>
        /// Changes an existing relation's source, destination and ordinal
        /// </summary>
        /// <param name="session"></param>
        /// <param name="originalRelation"></param>
        /// <param name="newSourceId"></param>
        /// <param name="newDestinationId"></param>
        /// <param name="newOrdinal"></param>
        public static void ChangeRelation(this ICoreRelationsRepository session, IRelationById originalRelation, HiveId newSourceId, HiveId newDestinationId, int newOrdinal = int.MinValue)
        {
            session.RemoveRelation(originalRelation);
            var useOrdinal = newOrdinal == int.MinValue ? originalRelation.Ordinal : newOrdinal;
            session.AddRelation(new Relation(originalRelation.Type, newSourceId, newDestinationId, useOrdinal, originalRelation.MetaData.ToArray()));
        }

        /// <summary>
        /// Updates a relation to be of a new relation type
        /// </summary>
        /// <param name="session"></param>
        /// <param name="originalRelation"></param>
        /// <param name="newRelationType"></param>
        public static void ChangeRelationType(this ICoreRelationsRepository session, IRelationById originalRelation, RelationType newRelationType)
        {
            session.RemoveRelation(originalRelation);
            session.AddRelation(new Relation(newRelationType, originalRelation.SourceId, originalRelation.DestinationId, originalRelation.Ordinal, originalRelation.MetaData.ToArray()));
        }

        public static IEnumerable<IRelationById> GetSiblingRelations(this ICoreReadonlyRelationsRepository session, IRelatableEntity sibling, RelationType relationType = null)
        {
            Mandate.ParameterNotNull(sibling, "sibling");
            return GetSiblingRelations(session, sibling.Id, relationType);
        }

        public static IEnumerable<IRelationById> GetSiblingRelations(this ICoreReadonlyRelationsRepository session, HiveId siblingId, RelationType relationType = null)
        {
            Mandate.ParameterNotEmpty(siblingId, "siblingId");
            return session.GetBranchRelations(siblingId, relationType).Where(x => !x.DestinationId.EqualsIgnoringProviderId(siblingId));
        }

        public static IEnumerable<IRelationById> GetAncestorRelations(this ICoreReadonlyRelationsRepository session, IRelatableEntity descendent, RelationType relationType = null)
        {
            Mandate.ParameterNotNull(descendent, "descendent");
            return session.GetAncestorRelations(descendent.Id, relationType);
        }

        public static IEnumerable<IRelationById> GetDescendentRelations(this ICoreReadonlyRelationsRepository session, IRelatableEntity ancestor, RelationType relationType = null)
        {
            Mandate.ParameterNotNull(ancestor, "ancestor");
            return session.GetDescendentRelations(ancestor.Id, relationType);
        }

        public static IEnumerable<IRelationById> GetChildRelations(this ICoreReadonlyRelationsRepository session, IRelatableEntity parent, RelationType relationType = null)
        {
            Mandate.ParameterNotNull(parent, "parent");
            return session.GetChildRelations(parent.Id, relationType);
        }

        public static IEnumerable<IRelationById> GetParentFileRelations(this IReadonlyEntityRepositoryGroup<IFileStore> unit, IRelatableEntity source)
        {
            Mandate.ParameterNotNull(source, "source");
            return unit.GetParentRelations(source.Id, FixedRelationTypes.DefaultRelationType);
        }

        /// <summary>
        /// Returns all files, excluding folders ordered by name
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static IEnumerable<File> GetAllNonContainerFiles(this IReadonlyEntityRepositoryGroup<IFileStore> unit)
        {
            return unit.GetAll<File>()
                .OrderBy(x => x.Name)
                .Where(x => !x.IsContainer);
        }

        public static IEnumerable<IRelationById> GetParentFileRelations(this IEntityRepositoryGroup<IFileStore> unit, IRelatableEntity source)
        {
            Mandate.ParameterNotNull(source, "source");
            return unit.GetParentRelations(source.Id, FixedRelationTypes.DefaultRelationType);
        }

        public static IEnumerable<IRelationById> GetRelations(this ICoreReadonlyRelationsRepository session, IRelatableEntity source, Direction direction, RelationType relationType = null)
        {
            Mandate.ParameterNotNull(source, "source");
            return GetRelations(session, source.Id, direction, relationType);
        }

        public static IEnumerable<IRelationById> GetRelations(this ICoreReadonlyRelationsRepository session, HiveId sourceId, Direction direction, RelationType relationType = null)
        {
            switch (direction)
            {
                case Direction.Ancestors:
                    return session.GetAncestorRelations(sourceId, relationType);
                case Direction.Children:
                    return session.GetChildRelations(sourceId, relationType);
                case Direction.Descendents:
                    return session.GetDescendentRelations(sourceId, relationType);
                case Direction.Parents:
                    return session.GetParentRelations(sourceId, relationType);
            }
            return Enumerable.Empty<IReadonlyRelation<IRelatableEntity, IRelatableEntity>>();
        }

        public static IEnumerable<IReadonlyRelation<IRelatableEntity, IRelatableEntity>> GetLazyRelations<T>(this ICoreReadonlyRepository<T> session, IRelatableEntity source, Direction direction, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            Mandate.ParameterNotNull(source, "source");
            return GetLazyRelations(session, source.Id, direction, relationType);
        }

        public static IEnumerable<IReadonlyRelation<IRelatableEntity, IRelatableEntity>> GetLazyRelations<T>(this ICoreReadonlyRepository<T> session, HiveId sourceId, Direction direction, RelationType relationType = null)
            where T : class, IRelatableEntity
        {
            switch (direction)
            {
                case Direction.Ancestors:
                    return session.GetLazyAncestorRelations(sourceId, relationType);
                case Direction.Children:
                    return session.GetLazyChildRelations(sourceId, relationType);
                case Direction.Descendents:
                    return session.GetLazyDescendentRelations(sourceId, relationType);
                case Direction.Parents:
                    return session.GetLazyParentRelations(sourceId, relationType);
            }
            return Enumerable.Empty<IReadonlyRelation<IRelatableEntity, IRelatableEntity>>();
        }
    }
}