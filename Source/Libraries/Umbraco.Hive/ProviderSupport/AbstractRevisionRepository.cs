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

    public abstract class AbstractRevisionRepository<T>
        : AbstractReadonlyRevisionRepository<T>, IProviderRevisionRepository<T> 
        where T : class, IVersionableEntity
    {
        protected AbstractRevisionRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext) 
            : this(providerMetadata, new NullProviderTransaction(), frameworkContext)
        { }

        protected AbstractRevisionRepository(ProviderMetadata providerMetadata, IProviderTransaction providerTransaction, IFrameworkContext frameworkContext)
            : base(providerMetadata, frameworkContext)
        {
            CanWrite = true;
            Transaction = providerTransaction;
            RevisionDataAddedOrUpdated = new HashSet<RevisionData>();
            EntityDataAddedOrUpdated = new HashSet<T>();
        }

        public bool CanWrite { get; protected set; }
        public IProviderTransaction Transaction { get; protected set; }
        protected HashSet<RevisionData> RevisionDataAddedOrUpdated { get; set; }
        protected HashSet<T> EntityDataAddedOrUpdated { get; set; }

        public Action<IReadonlyRelation<IRelatableEntity, IRelatableEntity>> RegisterRelatedEntities { get; set; }

        private void AutoAddRelationProxies()
        {
            if (RegisterRelatedEntities != null)
            {
                var changedItems = EntityDataAddedOrUpdated;
                var flatList = changedItems.SelectMany(y => y.RelationProxies.GetManualProxies());
                foreach (var relationProxy in flatList)
                {
                    RegisterRelatedEntities.Invoke(relationProxy.Item);
                }
            }
        }

        private void OnAddOrUpdateComplete<TEntity>(Revision<TEntity> revision) where TEntity : class, T
        {
            ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, revision.MetaData);
            ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, revision.Item);
            this.SetRelationProxyLazyLoadDelegate(revision);
            RevisionDataAddedOrUpdated.Add(revision.MetaData);
            EntityDataAddedOrUpdated.Add(revision.Item);
        }

        protected internal void PrepareForCompletion()
        {
            AutoAddRelationProxies();
        }

        public abstract void PerformAdd<TEntity>(Revision<TEntity> revision) where TEntity : class, T;
        
        /// <summary>
        /// Adds or updates a Revision. If an incoming <paramref name="revision"/> does not have an Id already, its attribute ids will be cleared
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="revision"></param>
        /// <remarks>
        /// This does not create a new revision, if an existing revision is passed in then it will simply be updated.
        /// To create a new revision, either pass a new Revision{T} to this method or use the Add extension method.
        /// </remarks>
        public void AddOrUpdate<TEntity>(Revision<TEntity> revision) where TEntity : class, T
        {
            if (TypeFinder.IsTypeAssignableFrom<TypedEntity>(typeof(TEntity)) && revision.MetaData.Id.IsNullValueOrEmpty())
            {
                var entity = revision.Item as TypedEntity;                
                foreach(var t in entity.Attributes)
                {
                    t.Id = HiveId.Empty;
                }
            }

            PerformAdd(revision);
            OnAddOrUpdateComplete(revision);

            Transaction.CacheFlushActions.Add(() =>
            {
                HiveContext.GenerationScopedCache.RemoveWhereKeyMatches<HiveQueryCacheKey>(x =>
                    {
                        return revision.Item != null && (!x.From.RequiredEntityIds.Any() || x.From.RequiredEntityIds.Any(y => y.Value == revision.Item.Id.Value));
                    });

                HiveContext.GenerationScopedCache.RemoveWhereKeyMatches<HiveRelationCacheKey>(x =>
                    {
                        return revision.Item != null && (x.EntityId.Value == revision.Item.Id.Value);
                    });
            });
        }
    }
}