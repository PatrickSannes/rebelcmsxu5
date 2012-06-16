using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Hive.ProviderSupport
{
    using Umbraco.Hive.Caching;

    public abstract class AbstractEntityRepository 
        : AbstractReadonlyEntityRepository, IProviderRepository<TypedEntity>
    {
        protected AbstractEntityRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext)
            : this(providerMetadata, new NullProviderTransaction(), frameworkContext)
        {
        }

        protected AbstractEntityRepository(ProviderMetadata providerMetadata, AbstractSchemaRepository schemas, IFrameworkContext frameworkContext)
            : this(providerMetadata, new NullProviderTransaction(), new NullProviderRevisionRepository<TypedEntity>(providerMetadata, frameworkContext), schemas, frameworkContext)
        {
        }

        protected AbstractEntityRepository(ProviderMetadata providerMetadata, IProviderTransaction providerTransaction, IFrameworkContext frameworkContext)
            : base(providerMetadata, frameworkContext)
        {
            Transaction = providerTransaction;
            Revisions = new NullProviderRevisionRepository<TypedEntity>(providerMetadata, frameworkContext);
            Schemas = new NullProviderSchemaRepository(providerMetadata, frameworkContext);
        }

        protected AbstractEntityRepository(ProviderMetadata providerMetadata, IProviderTransaction providerTransaction, AbstractRevisionRepository<TypedEntity> revisions, AbstractSchemaRepository schemas, IFrameworkContext frameworkContext)
            : base(providerMetadata, revisions, schemas, frameworkContext)
        {
            Transaction = providerTransaction;
            Revisions = revisions;
            Schemas = schemas;
            Revisions.RelatedEntitiesLoader = x => ProviderRepositoryHelper.CreateRelationLazyLoadDelegate(this, x).Invoke(x);
            Revisions.RegisterRelatedEntities = relation =>
                {
                    if (this.CanWriteRelations)
                        this.AddRelation(relation);
                };
        }

        protected abstract void PerformAddOrUpdate(TypedEntity entity);
        public void AddOrUpdate(TypedEntity entity)
        {
            // First, call Schemas.AddOrUpdate for the schema
            if (Schemas != null && Schemas.CanWrite && entity.EntitySchema != null)
                Schemas.AddOrUpdate(entity.EntitySchema);

            entity.Id = new HiveId((Uri)null, entity.Id.ProviderId,  entity.Id.Value);

            if (Revisions.CanWrite)
            {
                var newRevision = new Revision<TypedEntity>(entity);
                Revisions.AddOrUpdate(newRevision);
                return;
            }
            else
            {
                PerformAddOrUpdate(entity);
                OnAddOrUpdateComplete(entity);
            }

            AddCacheFlushesToTransaction(entity.Id);
        }

        protected virtual void AddCacheFlushesToTransaction(HiveId hiveId)
        {
            Transaction.CacheFlushActions.Add(
                () =>
                    {
                        if (!ContextCacheAvailable())
                        {
                            return;
                        }
                        HiveContext.GenerationScopedCache.RemoveWhereKeyMatches<HiveQueryCacheKey>(
                            x =>
                                {
                                    return !x.From.RequiredEntityIds.Any()
                                           || x.From.RequiredEntityIds.Any(y => y.Value == hiveId.Value);
                                });

                        HiveContext.GenerationScopedCache.RemoveWhereKeyMatches<HiveRelationCacheKey>(
                            x =>
                                {
                                    return x.EntityId.Value == hiveId.Value;
                                });
                    });
        }

        protected abstract void PerformDelete<T>(HiveId id) where T : TypedEntity;
        public void Delete<T>(HiveId id) where T : TypedEntity
        {
            PerformDelete<T>(id);
            AddCacheFlushesToTransaction(id);
        }

        public IProviderTransaction Transaction { get; protected set; }

        private void AutoAddRelationProxies()
        {
            if (!CanWriteRelations) return;
            var changedItems = EntitiesAddedOrUpdated;
            var flatList = changedItems.SelectMany(y => y.RelationProxies.GetManualProxies());
            foreach (var relationProxy in flatList)
            {
                AddRelation(relationProxy.Item);
            }
        }

        protected void OnAddOrUpdateComplete(TypedEntity entity)
        {
            this.SetRelationProxyLazyLoadDelegate(entity);
            ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, entity);
            EntitiesAddedOrUpdated.Add(entity);
        }

        protected internal void PrepareForCompletion()
        {
            AutoAddRelationProxies();
        }

        private AbstractRevisionRepository<TypedEntity> _revisions;
        public new AbstractRevisionRepository<TypedEntity> Revisions
        {
            get { return _revisions; }
            set
            {
                _revisions = value;
                base.Revisions = value;
            }
        }

        private AbstractSchemaRepository _schemas;
        public new AbstractSchemaRepository Schemas
        {
            get { return _schemas; }
            set
            {
                _schemas = value;
                base.Schemas = value;
            }
        }

        public abstract bool CanWriteRelations { get; }
        protected abstract void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item);
        public void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            Action actionToExecute = () =>
                {
                    PerformAddRelation(item);
                    if (!ContextCacheAvailable()) return;
                    Transaction.CacheFlushActions.Add(() =>
                    {
                        HiveContext.GenerationScopedCache.RemoveWhereKeyMatches<HiveRelationCacheKey>(x => x.EntityId.Value == item.SourceId.Value || x.EntityId.Value == item.DestinationId.Value);
                    });
                };
            FrameworkContext.TaskManager.ExecuteInCancellableTask(
                this,
                item,
                TaskTriggers.Hive.Relations.PreRelationAdded,
                TaskTriggers.Hive.Relations.PostRelationAdded,
                actionToExecute,
                x => new HiveRelationPreActionEventArgs(x, RepositoryScopedCache),
                x => new HiveRelationPostActionEventArgs(x, RepositoryScopedCache),
                FrameworkContext);
        }

        protected abstract void PerformRemoveRelation(IRelationById item);
        public void RemoveRelation(IRelationById item)
        {
            PerformRemoveRelation(item);
            Transaction.CacheFlushActions.Add(() =>
            {
                if (!ContextCacheAvailable()) return;
                HiveContext.GenerationScopedCache.RemoveWhereKeyMatches<HiveRelationCacheKey>(x => x.EntityId.Value == item.SourceId.Value || x.EntityId.Value == item.DestinationId.Value);
            });
        }
    }
}