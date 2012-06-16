using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.Persistence.RdbmsModel;
using Umbraco.Hive;
using Umbraco.Hive.ProviderSupport;
using AttributeType = Umbraco.Framework.Persistence.Model.Attribution.MetaData.AttributeType;

namespace Umbraco.Framework.Persistence.NHibernate
{
    public class SchemaRepository : AbstractSchemaRepository
    {
        public SchemaRepository(ProviderMetadata providerMetadata, AbstractRevisionRepository<EntitySchema> revisions, IProviderTransaction providerTransaction, ISession nhSession, IFrameworkContext frameworkContext,
            bool isReadOnly) 
            : base(providerMetadata, revisions, providerTransaction, frameworkContext)
        {
            IsReadonly = isReadOnly;
            Transaction = providerTransaction;
            Helper = new NhSessionHelper(nhSession, frameworkContext);
        }

        protected bool IsReadonly { get; set; }
        protected internal NhSessionHelper Helper { get; set; }

        protected override void DisposeResources()
        {
            Helper.IfNotNull(x => x.Dispose());
            Revisions.Dispose();
            Transaction.Dispose();
        }

        protected Type GetDestinationTypeOrThrow<T>()
        {
            Type destinationType;
            if (!FrameworkContext.TypeMappers.TryGetDestinationType<T, IReferenceByGuid>(out destinationType))
                throw new InvalidCastException("Cannot find a destination type for incoming type " + (typeof(T)).FullName);
            return destinationType;
        }

        protected override IEnumerable<TEntity> PerformGet<TEntity>(bool allOrNothing, params HiveId[] ids)
        {
            Mandate.ParameterNotNull(ids, "ids");
            ids.ForEach(x => Mandate.ParameterNotEmpty(x, "id"));

            Type destinationType = GetDestinationTypeOrThrow<TEntity>();

            Guid[] values = ids.Where(x => x.Value.Type == HiveIdValueTypes.Guid).Select(x => (Guid)x.Value).ToArray();
            var entities = Helper.NhSession.CreateCriteria(destinationType).Add(Restrictions.In(Projections.Property<Node>(x => x.Id), values)).List();
            return entities.Cast<IReferenceByGuid>().Select(x => FrameworkContext.TypeMappers.Map<TEntity>(x));
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return Helper.PerformFindRelation(sourceId, destinationId, relationType);
        }

        public override IEnumerable<TEntity> PerformGetAll<TEntity>()
        {
            Type destinationType = GetDestinationTypeOrThrow<TEntity>();

            var entities = Helper.NhSession.CreateCriteria(destinationType).List();

            return entities
                .Cast<IReferenceByGuid>()
                .Distinct() // Need the Distinct() call because Nh can return 2x entities even if they are equal references (e.g. added twice to same session)
                .Select(x => FrameworkContext.TypeMappers.Map<TEntity>(x));
        }

        public override bool CanReadRelations
        {
            get { return true; }
        }

        public override bool Exists<TEntity>(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            var value = (Guid)id.Value;

            var qo = Helper.NhSession.QueryOver<AttributeSchemaDefinition>()
                .Where(x => x.Id == value)
                .Select(Projections.RowCount())
                .SingleOrDefault<int>();

            return qo > 0;
        }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return Helper.PerformGetParentRelations(childId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            return GetParentRelations(descendentId, relationType).SelectRecursive(x => GetParentRelations(x.SourceId, relationType));            
        }

        public override IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            var childRelations = GetChildRelations(ancestorId, relationType).ToArray();
            return childRelations.SelectRecursive(x =>
            {
                var childRelationsSub = GetChildRelations(x.DestinationId, relationType).ToArray();
                return childRelationsSub;
            });
        }

        public override IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            return Helper.PerformGetChildRelations(parentId, relationType);
        }

        public override void PerformAddOrUpdate(AbstractSchemaPart entity)
        {
            Mandate.ParameterNotNull(entity, "entity");

            if (!entity.Id.IsNullValueOrEmpty() && entity.Id.Value.Type != HiveIdValueTypes.Guid) return;

            // BUG: (APN 26/11) The Rdbms mapping code needs a context / scope object to track multiple maps of the same item during a single mapping operation.
            // For example, if a schema comes in to this method with 2x AttributeDefintions both referring to 1x NEW AttributeType, 2x AttributeTypes
            // will be saved because during the mapping operation the AttributeType had no id, and so a new one was created - but there's no way of tracking
            // that the AttributeType has now been "mapped" and is pending an Id, so on the second AttributeDefinition it would cause the AttributeType
            // to get mapped again.
            // For now, we'll split the entities into their consituent parts and call the normal save methods independently for AttributeTypes


            var allItems = entity.GetAllIdentifiableItems();

            List<AbstractSchemaPart> savedItems = new List<AbstractSchemaPart>();
            allItems.OfType<AttributeType>().ForEach(
                x =>
                {
                    if (TryUpdateExisting(x)) return;
                    Helper.MapAndMerge(x, FrameworkContext.TypeMappers);
                    savedItems.Add(x);
                });

            // Do rest
            if (TryUpdateExisting(entity)) return;
            Helper.MapAndMerge(entity, FrameworkContext.TypeMappers);
        }

        

        private bool TryUpdateExisting(AbstractEntity persistedEntity)
        {
            var mappers = FrameworkContext.TypeMappers;

            // Get the entity with matching Id, provided the incoming Id is not null / empty
            if (!persistedEntity.Id.IsNullValueOrEmpty())
            {
                Type rdbmsType;
                if (mappers.TryGetDestinationType(persistedEntity.GetType(), typeof(IReferenceByGuid), out rdbmsType))
                {
                    //// Temp hack for testing
                    //if (typeof(NodeVersion) == rdbmsType && typeof(TypedEntity) == persistedEntity.GetType())
                    //{
                    //    rdbmsType = typeof(Node);

                    //    var nodeVersions = global::NHibernate.Linq.LinqExtensionMethods.Query<NodeVersion>(InnerDataContext.NhibernateSession).Where(x => x.Node.Id == persistedEntity.Id.AsGuid);
                    //    var firstOrDefault = nodeVersions.FirstOrDefault();
                    //    if (firstOrDefault == null) return false;

                    //    var latest = GetMostRecentVersionFromQuery(firstOrDefault.Node);
                    //    if (latest != null)
                    //    {
                    //        mappers.Map(persistedEntity, latest, persistedEntity.GetType(), latest.GetType());
                    //        //InnerDataContext.NhibernateSession.Evict(latest);
                    //        latest = InnerDataContext.NhibernateSession.Merge(latest) as NodeVersion;
                    //        //InnerDataContext.NhibernateSession.SaveOrUpdate(existingEntity);
                    //        mappers.Map(latest, persistedEntity, latest.GetType(), persistedEntity.GetType());
                    //        SetOutgoingId(persistedEntity);
                    //        //_trackNodePostCommits.Add((IReferenceByGuid)existingEntity, persistedEntity);
                    //        return true;
                    //    }
                    //}

                    var existingEntity = Helper.NhSession.Get(rdbmsType, (Guid)persistedEntity.Id.Value);
                    if (existingEntity != null)
                    {
                        mappers.Map(persistedEntity, existingEntity, persistedEntity.GetType(), existingEntity.GetType());
                        existingEntity = Helper.NhSession.Merge(existingEntity);
                        //InnerDataContext.NhibernateSession.SaveOrUpdate(existingEntity);
                        mappers.Map(existingEntity, persistedEntity, existingEntity.GetType(), persistedEntity.GetType());
                        // ##API2: Disabled: SetOutgoingId(persistedEntity);
                        //_trackNodePostCommits.Add((IReferenceByGuid)existingEntity, persistedEntity);
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Delete<T>(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            // We don't issue a direct-to-db deletion because otherwise NH can't keep track
            // of any cascading deletes

            object nhObject;

            if (typeof(EntitySchema).IsAssignableFrom(typeof(T)))
            {
                nhObject = Helper.NhSession.Get<AttributeSchemaDefinition>((Guid)id.Value);
                //get all of the nodes that reference this schema
                var nodes = ((AttributeSchemaDefinition) nhObject).ReferencedNodes.Select(x => x.Node).ToArray();
                foreach (var node in nodes)
                {
                    Helper.RemoveRelationsBiDirectional(node);
                    node.NodeVersions.EnsureClearedWithProxy();
                    Helper.NhSession.Delete(node);
                }
            }
            else
            {
                var destinationType = GetDestinationTypeOrThrow<T>();
                nhObject = Helper.NhSession.Get(destinationType, (Guid)id.Value);
            }

            //All IRelateable Entities are Nodes, remove their relations
            if (typeof(IRelatableEntity).IsAssignableFrom(typeof(T)))
            {
                var node = (Node)nhObject;
                Helper.RemoveRelationsBiDirectional(node);
            }

            Helper.NhSession.Delete(nhObject);
        }



        public override bool CanWriteRelations
        {
            get { return true; }
        }

        protected override void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            Helper.AddRelation(item, this.RepositoryScopedCache);
        }

        protected override void PerformRemoveRelation(IRelationById item)
        {
            Helper.RemoveRelation(item, this.RepositoryScopedCache);
        }
    }
}