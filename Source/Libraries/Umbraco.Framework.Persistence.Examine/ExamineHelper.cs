using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Examine.Hive;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive;

namespace Umbraco.Framework.Persistence.Examine
{
    /// <summary>
    /// A helper class to perform common indexing operations
    /// </summary>
    public class ExamineHelper : DisposableObject
    {
        public ExamineManager ExamineManager { get; private set; }
        private readonly IFrameworkContext _frameworkContext;
        private readonly bool _useCache;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        private const string ScopedCacheKey = "exa-";
        private const string RevisionStatusCacheKey = "exa-rev-stat-";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExamineHelper"/> class.
        /// </summary>
        /// <param name="examineManager">The examine manager.</param>
        /// <param name="frameworkContext">The framework context.</param>
        /// <param name="useCache">Whether or not to use Scoped and Application cache, generally only set to false for unit tests</param>
        public ExamineHelper(ExamineManager examineManager, IFrameworkContext frameworkContext, bool useCache = true)
        {
            Mandate.ParameterNotNull(examineManager, "examineManager");
            Mandate.ParameterNotNull(frameworkContext, "frameworkContext");
            ExamineManager = examineManager;
            _frameworkContext = frameworkContext;
            _useCache = useCache;
        }

        public void ClearCache(bool clearRevisionStatusCache, bool clearScopedCache)
        {
            if (clearRevisionStatusCache)
            {
                _frameworkContext.ApplicationCache.InvalidateItems(RevisionStatusCacheKey + ".*");
            }
            if (clearScopedCache)
            {
                _frameworkContext.ScopedCache.InvalidateItems(ScopedCacheKey + ".*");
            }
        }

        /// <summary>
        /// This ensures that the Ids for each item are set properly, if there is no id on the entity then we need to generate one.
        /// This will recursively ensure Ids are set on all related entities.
        /// </summary>
        /// <param name="ops"></param>
        internal static void EnsureIds(params LinearHiveIndexOperation[] ops)
        {

            foreach (var o in ops)
            {
                if (o.Entity.GetType().IsOfGenericType(typeof(Revision<>)))
                {
                    var entity = (IReferenceByHiveId)((dynamic)o.Entity).Item;
                    var revData = (IReferenceByHiveId)((dynamic)o.Entity).MetaData;
                    entity.AssignIds(() => new HiveId(Guid.NewGuid()));
                    revData.AssignIds(() => new HiveId(Guid.NewGuid()));
                }
                else if (o.Entity is IReferenceByHiveId)
                {
                    var entity = (IReferenceByHiveId)o.Entity;
                    entity.AssignIds(() => new HiveId(Guid.NewGuid()));
                }
            }

        }

        /// <summary>
        /// Adds/Updates the timestamps for any IEntity and reflects those values in the index fields
        /// </summary>
        /// <param name="d"></param>
        /// <param name="entity"></param>
        /// <param name="utcCreatedField">The field to store the created value in</param>
        /// <remarks>
        /// This also stores the offset for the DateTimeOffset in the index, though the offset is stored as non INDEXED but STORED, meaning
        /// that its not searchable but will be returned in results.
        /// </remarks>
        internal static void SetTimestampsOnEntityAndIndexFields(LazyDictionary<string, ItemField> d, IEntity entity, string utcCreatedField = FixedIndexedFields.UtcCreated)
        {
            //always update the modified date
            entity.UtcModified = DateTimeOffset.UtcNow;
            StoreDateTimeOffset(FixedIndexedFields.UtcModified, d, entity.UtcModified);

            //update date created if its not specified
            if (entity.UtcCreated == default(DateTimeOffset))
            {
                entity.UtcCreated = entity.UtcModified;
            }
            StoreDateTimeOffset(utcCreatedField, d, entity.UtcCreated);

            //update status changed if its not specified
            if (entity.UtcStatusChanged == default(DateTimeOffset))
            {
                entity.UtcStatusChanged = entity.UtcModified;
            }
            StoreDateTimeOffset(FixedIndexedFields.UtcStatusChanged, d, entity.UtcStatusChanged);
        }

        private static void StoreDateTimeOffset(string fieldName, LazyDictionary<string, ItemField> d, DateTimeOffset dt)
        {
            //store the date
            d.AddOrUpdate(fieldName, new ItemField(dt.DateTime) { DataType = FieldDataType.DateTime });
            //store the offset
            d.AddOrUpdate(fieldName + FixedIndexedFields.DateTimeOffsetSuffix, new ItemField(dt.Offset.TotalHours));
        }

        /// <summary>
        /// Returns a DateTimeOffset object from the field in the search result specified or null if conversion couldn't be done or the 
        /// required fields could not be found.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        internal static DateTimeOffset? FromExamineDateTime(IDictionary<string, string> r, string fieldName)
        {
            long output;
            var dateTime = r.GetValue(fieldName, "notfound");
            if (long.TryParse(dateTime, out output))
            {
                if (!r.ContainsKey(fieldName + FixedIndexedFields.DateTimeOffsetSuffix))
                {
                    return null;
                }
                int offset;
                if (!Int32.TryParse(r[fieldName + FixedIndexedFields.DateTimeOffsetSuffix], out offset))
                {
                    return null;
                }
                return new DateTimeOffset(LuceneIndexer.DateTimeFromTicks(output), new TimeSpan(0, offset, 0));
            }
            return null;
        }

        /// <summary>
        /// Returns a DateTimeOffset with an offset of zero (UTC) for the value passed in, if it cannot be parsed a null is returned.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        internal static DateTimeOffset? FromExamineDateTime(string val)
        {
            long output;
            if (long.TryParse(val, out output))
            {
                return new DateTimeOffset(LuceneIndexer.DateTimeFromTicks(output), new TimeSpan(0, 0, 0));
            }
            return null;
        }

        /// <summary>
        /// Updates the date fields for an IEntity from a SearchResult
        /// </summary>
        /// <param name="e"></param>
        /// <param name="r"></param>
        internal static void SetEntityDatesFromSearchResult(IEntity e, SearchResult r)
        {
            e.UtcCreated = FromExamineDateTime(r.Fields, FixedIndexedFields.UtcCreated).Value;
            e.UtcModified = FromExamineDateTime(r.Fields, FixedIndexedFields.UtcModified).Value;
            e.UtcStatusChanged = FromExamineDateTime(r.Fields, FixedIndexedFields.UtcStatusChanged).Value;
        }


        /// <summary>
        /// Ensures that the revision status type exists in the index
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public void EnsureRevisionStatus(RevisionStatusType status)
        {
            using (new WriteLockDisposable(_locker))
            {
                var found = GetRevisionStatusType((Guid)status.Id.Value);

                if (found == null)
                {
                    //we need to create it
                    var op = new IndexOperation()
                        {
                            Item = new IndexItem
                                {
                                    Id = status.Id.Value.ToString(),
                                    Fields = new Dictionary<string, ItemField>
                                        {
                                            {FixedRevisionIndexFields.RevisionStatusAlias, new ItemField(status.Alias)},
                                            {FixedRevisionIndexFields.RevisionStatusName, new ItemField(status.Name)},
                                            {
                                                FixedRevisionIndexFields.RevisionStatusIsSystem, new ItemField(Convert.ToInt32(status.IsSystem))
                                                    {
                                                        DataType = FieldDataType.Int
                                                    }
                                                }
                                        },
                                    ItemCategory = "RevisionStatus"
                                },
                            Operation = IndexOperationType.Add
                        };
                    ExamineManager.PerformIndexing(op);

                    ClearCache(true, false);
                }
            }
        }

        /// <summary>
        /// Gets a RevisionStatusType object based on the revision status id
        /// </summary>
        /// <param name="statusId"></param>
        /// <returns></returns>
        public RevisionStatusType GetRevisionStatusType(Guid statusId)
        {
            //return from cache
            return GetOrCreateFromAppCache(RevisionStatusCacheKey + statusId.ToString("N"), () =>
                {
                    var found = ExamineManager.Search(
                        ExamineManager.CreateSearchCriteria()
                            .Must().Field(LuceneIndexer.IndexCategoryFieldName, "RevisionStatus".Escape())
                            .Must().Id(statusId.ToString("N")).Compile());

                    if (!found.Any())
                        return null;

                    var revStatus = found.First();

                    return new HttpRuntimeCacheParameters<RevisionStatusType>(
                        new RevisionStatusType(
                            new HiveId(statusId),
                            revStatus.Fields[FixedRevisionIndexFields.RevisionStatusAlias],
                            revStatus.Fields[FixedRevisionIndexFields.RevisionStatusName],
                            revStatus.Fields[FixedRevisionIndexFields.RevisionStatusIsSystem] == "1"));
                });
        }

        /// <summary>
        /// Returns ancestor relations
        /// </summary>
        /// <param name="repo">The repository to recursively select parent relations from</param>
        /// <param name="descendentId"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public IEnumerable<IRelationById> PerformGetAncestorRelations(ICoreReadonlyRelationsRepository repo, HiveId descendentId, RelationType relationType = null)
        {
            return repo.GetParentRelations(descendentId, relationType).SelectRecursive(x => repo.GetParentRelations(x.SourceId, relationType));
        }

        /// <summary>
        /// Returns descendent relations
        /// </summary>
        /// <param name="repo">The repository to recusively select child relations from</param>
        /// <param name="ancestorId"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public IEnumerable<IRelationById> PerformGetDescendentRelations(ICoreReadonlyRelationsRepository repo, HiveId ancestorId, RelationType relationType = null)
        {
            var childRelations = repo.GetChildRelations(ancestorId, relationType).ToArray();
            return childRelations.SelectRecursive(x =>
            {
                var childRelationsSub = repo.GetChildRelations(x.DestinationId, relationType).ToArray();
                return childRelationsSub;
            });
        }

        /// <summary>
        /// Finds a relation
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="destinationId"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            Mandate.ParameterNotEmpty(sourceId, "sourceId");
            Mandate.ParameterNotEmpty(destinationId, "destinationId");
            Mandate.ParameterNotNull(relationType, "relationType");
            Mandate.ParameterNotNullOrEmpty(relationType.RelationName, "relationType.RelationName");

            var key = ScopedCacheKey + "PerformFindRelation-" +
                (sourceId.Value.ToString() + destinationId.Value.ToString() + relationType.RelationName).ToMd5();

            return GetOrCreateFromScopedCache<IRelationById>(key, () =>
                {
                    //lookup all relations with this destination
                    var criteria = ExamineManager.CreateSearchCriteria()
                        .Must().Field(LuceneIndexer.IndexCategoryFieldName, "Relation".Escape())
                        .Must().HiveId(sourceId, FixedRelationIndexFields.SourceId)
                        .Must().HiveId(destinationId, FixedRelationIndexFields.DestinationId)
                        .Must().Field(FixedRelationIndexFields.RelationType, relationType.RelationName.Escape());

                    var result = ExamineManager.Search(criteria.Compile());
                    return result.Select(x => _frameworkContext.TypeMappers.Map<SearchResult, IRelationById>(x)).SingleOrDefault();
                });


        }

        /// <summary>
        /// Gets the child relations
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            var key = ScopedCacheKey + "PerformGetChildRelations-" + 
                (parentId.Value + (relationType != null ? relationType.RelationName : "")).ToMd5();

            return GetOrCreateFromScopedCache<IEnumerable<IRelationById>>(key, () =>
                {
                    if (parentId.IsNullValueOrEmpty())
                        return Enumerable.Empty<RelationById>();

                    //lookup all relations with this destination
                    var criteria = ExamineManager.CreateSearchCriteria()
                        .Must().Field(LuceneIndexer.IndexCategoryFieldName, "Relation".Escape())
                        .Must().HiveId(parentId, FixedRelationIndexFields.SourceId);

                    if (relationType != null)
                    {
                        criteria = criteria
                            .Must().Field(FixedRelationIndexFields.RelationType, relationType.RelationName.Escape());
                    }

                    var result = ExamineManager.Search(criteria.Compile());
                    
                    //NOTE: This isn't lazy since we need to store a real object in cache but with Lucene, we already have the results
                    return result.Select(x => _frameworkContext.TypeMappers.Map<SearchResult, IRelationById>(x)).ToArray();
                });


        }

        /// <summary>
        /// Gets the parent relations
        /// </summary>
        /// <param name="childId"></param>
        /// <param name="relationType"></param>
        /// <returns></returns>
        public IEnumerable<IRelationById> PeformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            var key = ScopedCacheKey + "PeformGetParentRelations-" + 
                (childId.Value + (relationType != null ? relationType.RelationName : "")).ToMd5();

            return GetOrCreateFromScopedCache<IEnumerable<IRelationById>>(key, () =>
                {
                    if (childId.IsNullValueOrEmpty())
                        return Enumerable.Empty<RelationById>();

                    //lookup all relations with this destination
                    var criteria = ExamineManager.CreateSearchCriteria()
                        .Must().Field(LuceneIndexer.IndexCategoryFieldName, "Relation".Escape())
                        .Must().HiveId(childId, FixedRelationIndexFields.DestinationId);

                    if (relationType != null)
                    {
                        criteria = criteria
                            .Must().Field(FixedRelationIndexFields.RelationType, relationType.RelationName.Escape());
                    }

                    var result = ExamineManager.Search(criteria.Compile());
                    //NOTE: This isn't lazy since we need to store a real object in cache but with Lucene, we already have the results
                    return result.Select(x => _frameworkContext.TypeMappers.Map<SearchResult, IRelationById>(x)).ToArray();
                });

        }

        /// <summary>
        /// Removes the relation.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="transaction"></param>
        public void PerformRemoveRelation(IRelationById item, ExamineTransaction transaction)
        {
            Mandate.ParameterNotNull(item, "item");

            var delete = new LinearHiveIndexOperation()
            {
                OperationType = IndexOperationType.Delete,
                Id = new Lazy<string>(item.GetCompositeId)
            };
            transaction.EnqueueIndexOperation(delete);
        }

        /// <summary>
        /// Adds the relation.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="transaction"></param>
        public void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item, ExamineTransaction transaction)
        {
            Mandate.ParameterNotNull(item, "item");

            var r = _frameworkContext.TypeMappers.MapToIntent<NestedHiveIndexOperation>(item);
            transaction.EnqueueIndexOperation(r);
        }

        /// <summary>
        /// Performs the delete.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="transaction"></param>
        public void PerformDelete(HiveId id, ExamineTransaction transaction)
        {
            Mandate.ParameterNotEmpty(id, "id");

            PerformDelete(id.Value.ToString(), transaction);
        }

        /// <summary>
        /// Performs the delete.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="transaction"></param>
        /// <remarks>
        /// This will lookup all all related entities and remove them from the index
        /// </remarks>
        public void PerformDelete(string id, ExamineTransaction transaction)
        {
            Mandate.ParameterNotNullOrEmpty(id, "id");

            //NOTE: The below deletion process relies on having unique GUID ids across the board,
            // if you want to support int Ids, then we'll need to lookup the item type first
            // and delete different things based on that, i chose not to do that because 
            // if for some reason the item with the Id that we're deleting isn't there then 
            // any related instances will never be deleted whereas doing the below will delete all
            // related entities regardless.

            Action<IEnumerable<SearchResult>> addItemsToDeleteQueue = x =>
                {
                    foreach (var r in x)
                    {
                        var r1 = r;
                        transaction.EnqueueIndexOperation(new LinearHiveIndexOperation
                        {
                            OperationType = IndexOperationType.Delete,
                            Id = new Lazy<string>(() => r1.Id)
                        });
                    }
                };

            //First, check if there's relations for this id and remove any relations found
            var relations = ExamineManager.Search(
                ExamineManager.CreateSearchCriteria()
                    .Should()
                    .Id(id, FixedRelationIndexFields.SourceId)
                    .Should()
                    .Id(id, FixedRelationIndexFields.DestinationId).Compile());
            addItemsToDeleteQueue(relations);

            //next, check if there's any items (TypedEntity, AttributeDefinition, AttributeGroup) assigned to a schema by this id,
            // this will also delete all revisions of TypedEntity too
            var entities = ExamineManager.Search(
                ExamineManager.CreateSearchCriteria()
                    .Should()
                    .Id(id, FixedIndexedFields.SchemaId)
                    .Should()
                    .Id(id, FixedIndexedFields.EntityId)
                    .Compile());
            addItemsToDeleteQueue(entities);

            //now, check if the item being deleted is an attribute type, if it is we need to remove all AttributeDefinitions associated
            // with it and then all TypedAttribute fields belonging to the TypedEntity that reference these AttributeDefinitions
            var attributeDefs = ExamineManager.Search(
                ExamineManager.CreateSearchCriteria()
                    .Must()
                    .Id(id, FixedIndexedFields.AttributeTypeId)
                    .Must()
                    .EntityType<AttributeDefinition>()
                    .Compile());
            addItemsToDeleteQueue(attributeDefs);
            //now that we have the attribute defintions related to the typed attribute being deleted, we need to actually create new revisions 
            // for any entity that had a schemas with these definitions on them.
            var schemaIds = attributeDefs.Select(x => x.Fields[FixedIndexedFields.SchemaId]).Distinct();
            foreach (var schemaId in schemaIds)
            {
                var criteria = ExamineManager.CreateSearchCriteria()
                    .Must().EntityType<TypedEntity>()
                    .Must().Id(schemaId, FixedIndexedFields.SchemaId)
                    //need to lookup latest because when we're supporting revisions, we will have more than one version of a TypedEntity
                    .Must().Range(FixedRevisionIndexFields.IsLatest, 1, 1);

                var latest = FilterLatestTypedEntities(
                    ExamineManager.Search(criteria.Compile()));

                //now that we have the latest TypedEntity for any Schema that had an AttributeDefinition on it that we are deleting
                // because we are deleting it's TypedAttribute, we need to make a new revision of
                //NOTE: If for some reason Revisions are disabled, this wont work
                foreach (var l in latest)
                {
                    //now that we have an entity, we'll loop through the attribute defs were removing an ensure it's fields are removed
                    foreach (var d in attributeDefs)
                    {
                        //remove all attributes starting with the prefix and attribute def alias
                        var d1 = d;
                        l.Fields.RemoveAll(x => x.Key.StartsWith(FixedAttributeIndexFields.AttributePrefix + d1.Fields[FixedIndexedFields.Alias]));
                        //conver the fields to be used in an index operation
                        var opFields = l.Fields.ToLazyDictionary(x => new ItemField(x));
                        //update some manual fields like the dates and revision ids
                        StoreDateTimeOffset(FixedIndexedFields.UtcModified, opFields, DateTimeOffset.UtcNow);
                        var revId = Guid.NewGuid().ToString("N");
                        opFields.AddOrUpdate(FixedRevisionIndexFields.RevisionId,
                                             new Lazy<ItemField>(() => new ItemField(revId)),
                                             (key, o) => new Lazy<ItemField>(() => new ItemField(revId)));

                        //need to generate a new id (which is composite)
                        var l1 = l;
                        var op = new LinearHiveIndexOperation
                        {
                            OperationType = IndexOperationType.Add,
                            Id = new Lazy<string>(() => l1.Fields[FixedIndexedFields.EntityId] + "," + revId),
                            Fields = opFields
                        };
                        //create the new revision without the fields
                        transaction.EnqueueIndexOperation(op);
                    }

                }
            }

            //finally, lookup the item itself and remove it
            var item = ExamineManager.Search(ExamineManager.CreateSearchCriteria().Must().Id(id).Compile());
            addItemsToDeleteQueue(item);
        }

        /// <summary>
        /// Adds a revision
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="revision"></param>
        /// <param name="transaction"></param>
        public void PerformAddRevision<TEntity>(Revision<TEntity> revision, ExamineTransaction transaction)
            where TEntity : class, IVersionableEntity
        {
            Mandate.ParameterNotNull(revision, "revision");

            var e = _frameworkContext.TypeMappers.MapToIntent<NestedHiveIndexOperation>(revision);
            transaction.EnqueueIndexOperation(e);

            //we need to recursively add all operations and sub operations to the queue
            foreach (var r in e.SubIndexOperations.SelectRecursive(x => x.SubIndexOperations))
            {
                transaction.EnqueueIndexOperation(r);
            }
        }

        /// <summary>
        /// Ensures all of the revision fields and data is stored in the index operation
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>        
        internal void EnsureRevisionDataForIndexOperation<TEntity>(Revision<TEntity> s, LinearHiveIndexOperation t)
            where TEntity : class, IVersionableEntity
        {
            //now we need to update the ids, NOTE: the __NodeId is a composite id!
            t.Id = new Lazy<string>(() => s.Item.Id.Value.ToString() + "," + s.MetaData.Id.Value.ToString());
            t.AddOrUpdateField(FixedRevisionIndexFields.RevisionId, new Lazy<string>(() => s.MetaData.Id.Value.ToString()));

            //we also need to make sure the entity property is set to the Revision<TypedEntity> not just TypedEntity
            t.Entity = s;

            //ensure that the revision status type is stored in a document
            EnsureRevisionStatus(s.MetaData.StatusType);

            //add the revision status id
            t.AddOrUpdateField(FixedRevisionIndexFields.RevisionStatusId, s.MetaData.StatusType.Id.Value.ToString());
            t.AddOrUpdateField(FixedRevisionIndexFields.RevisionStatusAlias, s.MetaData.StatusType.Alias);
        }

        /// <summary>
        /// Method to add delete queue items for each schema association since we're not tracking
        /// revisions with schemas, we need to first remove the associations like groups/attribute defs and then re-add them
        /// with the up-to-date info.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="manager"></param>
        /// <param name="transaction"></param>
        internal static void RemoveSchemaAssociations(EntitySchema schema, ExamineManager manager, ExamineTransaction transaction)
        {
            //find all associations previously added to the transaction queue
            transaction.RemoveSchemaAssociations(schema);

            //if there's no id already assigned to the schema, then we can't look it up
            if (schema.Id.IsNullValueOrEmpty()) return;

            //lookup all associated: AttributeGroup, AttributeDefinition that already exist in the index
            var toRemove = manager.Search(
                manager.CreateSearchCriteria()
                    .Must().EntityType<AttributeGroup>()
                    .Must().HiveId(schema.Id, FixedIndexedFields.SchemaId).Compile())
                .Concat(manager.Search(manager.CreateSearchCriteria()
                                           .Must().EntityType<AttributeDefinition>()
                                           .Must().HiveId(schema.Id, FixedIndexedFields.SchemaId).Compile()));
            foreach (var r in toRemove)
            {
                //need to copy to closure
                var r1 = r;
                transaction.EnqueueIndexOperation(new LinearHiveIndexOperation
                {
                    Id = new Lazy<string>(() => r1.Id),
                    OperationType = IndexOperationType.Delete
                });
            }
        }

        /// <summary>
        /// Performs the add or update.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="transaction"></param>
        public void PerformAddOrUpdate(AbstractEntity entity, ExamineTransaction transaction)
        {
            Mandate.ParameterNotNull(entity, "entity");

            //use the mappers to map this entity (whatever it might be) to an IndexCategoryOperation
            var indexOperation = _frameworkContext.TypeMappers.MapToIntent<NestedHiveIndexOperation>(entity);

            if (TypeFinder.IsTypeAssignableFrom<TypedEntity>(entity.GetType()))
            {
                //since we're updating the schema when its an entity, need to ensure associations are removed
                RemoveSchemaAssociations(((TypedEntity)entity).EntitySchema, ExamineManager, transaction);
            }
            if (TypeFinder.IsTypeAssignableFrom<EntitySchema>(entity.GetType()))
            {
                //need to ensure schema associations are removed
                RemoveSchemaAssociations((EntitySchema)entity, ExamineManager, transaction);
            }

            transaction.EnqueueIndexOperation(indexOperation);

            //we need to recursively add all operations and sub operations to the queue
            foreach (var operation in indexOperation.SubIndexOperations.SelectRecursive(x => x.SubIndexOperations))
            {
                transaction.EnqueueIndexOperation(operation);
            }
        }

        /// <summary>
        /// Check if an entity exists
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The id.</param>
        /// <param name="idField"></param>
        /// <returns></returns>
        public bool Exists<TEntity>(HiveId id, string idField)
        {
            if (id.IsNullValueOrEmpty())
                return false;

            return PerformGet<TEntity>(true, idField, id).Any();
        }

        /// <summary>
        /// Gets the latest entity version
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityId"></param>
        /// <param name="revisionStatusType"></param>
        /// <returns></returns>
        public Revision<TEntity> PerformGetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null)
            where TEntity : class, IVersionableEntity
        {
            var key = ScopedCacheKey + "PerformGetLatestRevision-" + 
                (typeof(TEntity).FullName + entityId.Value + (revisionStatusType != null ? revisionStatusType.Alias : "")).ToMd5();

            return GetOrCreateFromScopedCache<Revision<TEntity>>(key, () =>
                {
                    var criteria = ExamineManager.CreateSearchCriteria()
                        .Must().HiveId(entityId, FixedIndexedFields.EntityId)
                        .Must().Range(FixedRevisionIndexFields.IsLatest, 1, 1);

                    if (revisionStatusType != null)
                    {
                        criteria = criteria
                            .Must().HiveId(revisionStatusType.Id, FixedRevisionIndexFields.RevisionStatusId);
                    }

                    var result = ExamineManager.Search(criteria.Compile());

                    //now we need to get the absolute latest since there maybe be a couple IsLatest items
                    //this also needs to ensure that its only returning real revisions since a TypedEntity 'might' exist
                    //on its own if it was added in a relation.
                    var latest = result
                        .Where(x => x.Fields.ContainsKey(FixedRevisionIndexFields.RevisionId))
                        .OrderBy(x => x.Fields[FixedIndexedFields.UtcModified]).LastOrDefault();
                    if (latest == null) return null;

                    return _frameworkContext.TypeMappers.Map<Revision<TEntity>>(latest);
                });


        }

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> PerformGetAll<T>(string category)
        {
            IEnumerable<SearchResult> result;

            if (TypeFinder.IsTypeAssignableFrom<TypedEntity>(typeof(T)))
            {
                //we need to lookup the latest Typed entities

                var criteria = ExamineManager.CreateSearchCriteria()
                    .Must().Field(LuceneIndexer.IndexCategoryFieldName, category.Escape())
                    //need to lookup latest because when we're supporting revisions, we will have more than one version of a TypedEntity
                    .Must().Range(FixedRevisionIndexFields.IsLatest, 1, 1);

                result = FilterLatestTypedEntities(
                    ExamineManager.Search(criteria.Compile()));
            }
            else
            {
                //everything else

                var criteria = ExamineManager.CreateSearchCriteria()                    
                    .Must().Field(LuceneIndexer.IndexCategoryFieldName, category.Escape());
                result = ExamineManager.Search(criteria.Compile());
            }

            //map the entities
            return result.Select(r => _frameworkContext.TypeMappers.Map<T>(r));
        }

        /// <summary>
        /// Filters the search results to the latest by UtcModified
        /// </summary>
        /// <param name="searchResults"></param>
        /// <returns></returns>
        private IEnumerable<SearchResult> FilterLatestTypedEntities(IEnumerable<SearchResult> searchResults)
        {
            return (from r in searchResults
                    group r by r.Fields[FixedIndexedFields.EntityId]
                        into g
                        let max = g.Max(x => x.Fields[FixedIndexedFields.UtcModified])
                        select g.Where(x => x.Fields[FixedIndexedFields.UtcModified] == max).First());
        }

        /// <summary>
        /// Gets all revision entities
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="category"></param>
        /// <param name="entityId"></param>
        /// <param name="revisionStatusType"></param>
        /// <returns></returns>
        public IEnumerable<Revision<TEntity>> PerformGetAllRevisions<TEntity>(string category, HiveId entityId = default(HiveId), RevisionStatusType revisionStatusType = null)
            where TEntity : class, IVersionableEntity
        {

            var key = ScopedCacheKey + "PerformGetAllRevisions-" + 
                (typeof(TEntity).FullName + category + entityId.Value + (revisionStatusType == null ? "" : revisionStatusType.Alias)).ToMd5();

            return GetOrCreateFromScopedCache<IEnumerable<Revision<TEntity>>>(key, () =>
                {
                    var criteria = ExamineManager.CreateSearchCriteria()
                        .Must().Field(LuceneIndexer.IndexCategoryFieldName, category.Escape());

                    if (entityId != default(HiveId))
                    {
                        criteria = criteria.Must().HiveId(entityId, FixedIndexedFields.EntityId);
                    }

                    if (revisionStatusType != null)
                    {
                        criteria = criteria.Must().HiveId(revisionStatusType.Id, FixedRevisionIndexFields.RevisionStatusId);
                    }

                    //get the results from Examine and filter to only contain revisions since
                    // there may be solo TypedEntities in the index due to the way that relations work.
                    // when a relation is detected via another entity, it creates a TypedEntity without the revision
                    // data.
                    var result = ExamineManager.Search(criteria.Compile())
                        .Where(x => x.Fields.ContainsKey(FixedRevisionIndexFields.RevisionId));

                    return result.Select(r => _frameworkContext.TypeMappers.Map<Revision<TEntity>>(r)).ToArray();
                });
        }

        /// <summary>
        /// Gets a revision matching the revision id, entity id and entity type
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        public Revision<TEntity> PerformGetRevision<TEntity>(HiveId entityId, HiveId revisionId)
            where TEntity : class, IVersionableEntity
        {
            Mandate.ParameterNotEmpty(entityId, "entityId");
            Mandate.ParameterNotEmpty(revisionId, "revisionId");

            var key = ScopedCacheKey + "PerformGetRevision-" + 
                (typeof(TEntity).FullName + entityId.Value + revisionId.Value).ToMd5();

            return GetOrCreateFromScopedCache<Revision<TEntity>>(key, () =>
                {
                    var criteria = ExamineManager.CreateSearchCriteria()
                        .Must().HiveId(revisionId, FixedRevisionIndexFields.RevisionId)
                        .Must().HiveId(entityId, FixedIndexedFields.EntityId)
                        .Must().EntityType<TEntity>();
                    var result = ExamineManager.Search(criteria.Compile());
                    if (result.TotalItemCount > 1)
                        throw new IndexOutOfRangeException("The search result returned more than one item");

                    if (result.TotalItemCount == 0)
                        return null;

                    return _frameworkContext.TypeMappers.Map<Revision<TEntity>>(result.Single());
                });


        }

        public IEnumerable<SearchResult> GetAttributeDefinitionsForSchema(HiveId schemaId)
        {
            //NOTE: putting scoped cache on this causes errors

            return ExamineManager.Search(
                ExamineManager.CreateSearchCriteria()
                    .Must().EntityType<AttributeDefinition>()
                    .Must().HiveId(schemaId, FixedIndexedFields.SchemaId)
                    .Compile());
        }

        public IEnumerable<Tuple<AttributeGroup, SearchResult>> GetMappedGroupsForSchema(HiveId schemaId)
        {
            var key = ScopedCacheKey + "GetMappedGroupsForSchema-" + schemaId.Value;

            //find all attribute groups with this schema id
            return GetOrCreateFromScopedCache<IEnumerable<Tuple<AttributeGroup, SearchResult>>>(key, () => ExamineManager.Search(
                ExamineManager.CreateSearchCriteria()
                    .Must().EntityType<AttributeGroup>()
                    .Must().HiveId(schemaId, FixedIndexedFields.SchemaId)
                    .Compile()).Select(x => new Tuple<AttributeGroup, SearchResult>(_frameworkContext.TypeMappers.Map<AttributeGroup>(x), x))
                    .ToArray());
        }

        /// <summary>
        /// Gets entities matching the id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allOrNothing">if set to <c>true</c> [all or nothing].</param>
        /// <param name="idField">The id field to search on</param>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public IEnumerable<T> PerformGet<T>(bool allOrNothing, string idField, params HiveId[] ids)
        {
            Mandate.ParameterNotNull(ids, "ids");
            ids.ForEach(x => Mandate.ParameterNotEmpty(x, "id"));

            var key = ScopedCacheKey + "PerformGet-" + 
                (typeof(T).FullName + allOrNothing + idField + string.Join(".", ids.Select(x => x.Value))).ToMd5();

            return GetOrCreateFromScopedCache<IEnumerable<T>>(key, () =>
            {
                //NOTE: Because of allOrNothing, we can't yield return anything.
                var toReturn = new List<T>();

                foreach (var i in ids)
                {
                    IQuery criteria;
                    if (TypeFinder.IsTypeAssignableFrom<TypedEntity>(typeof(T)))
                    {
                        criteria = ExamineManager.CreateSearchCriteria()
                            .Must().HiveId(i, idField)
                            .Must().Range(FixedRevisionIndexFields.IsLatest, 1, 1)
                            .Must().EntityType<T>().Compile();
                    }
                    else
                    {
                        //anything else...
                        criteria = ExamineManager.CreateSearchCriteria()
                            .Must().HiveId(i, idField)
                            .Must().EntityType<T>().Compile();
                    }

                    // NOTE: We currently return the 'newest', not sure if that is the correct approach. SD.
                    var result = ExamineManager.Search(criteria)
                        .OrderBy(x => x.Fields[FixedIndexedFields.UtcModified])
                        .LastOrDefault();

                    if (result == null)
                        if (allOrNothing)
                            return Enumerable.Empty<T>();

                    var mapped = _frameworkContext.TypeMappers.Map<T>(result);

                    toReturn.Add(mapped);
                }
                return toReturn;
            });
        }

        protected T GetOrCreateFromAppCache<T>(string key, Func<HttpRuntimeCacheParameters<T>> data)
            where T : class
        {
            if (_useCache)
            {
                return _frameworkContext.ApplicationCache.GetOrCreate<T>(key, data);
            }
            var d = data();
            return d != null ? data().Value : null;
        }

        protected T GetOrCreateFromScopedCache<T>(string key, Func<T> data) 
            where T : class
        {
            if (_useCache)
            {
                return _frameworkContext.ScopedCache.GetOrCreateTyped<T>(key, data);    
            }
            return data();
        }


        protected override void DisposeResources()
        {
            ExamineManager.Dispose();
        }
    }
}