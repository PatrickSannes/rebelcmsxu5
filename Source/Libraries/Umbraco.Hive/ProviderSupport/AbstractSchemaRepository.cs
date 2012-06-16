using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Hive.ProviderSupport
{
    public abstract class AbstractSchemaRepository : AbstractReadonlySchemaRepository, IProviderRepository<AbstractSchemaPart>
    {
        protected AbstractSchemaRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext)
            : this(providerMetadata, new NullProviderTransaction(), frameworkContext)
        {
        }

        protected AbstractSchemaRepository(ProviderMetadata providerMetadata, IProviderTransaction providerTransaction, IFrameworkContext frameworkContext)
            : this(providerMetadata, new NullProviderRevisionRepository<EntitySchema>(providerMetadata, frameworkContext), providerTransaction, frameworkContext)
        {
            Transaction = providerTransaction;
        }

        protected AbstractSchemaRepository(ProviderMetadata providerMetadata, AbstractRevisionRepository<EntitySchema> revisions, IProviderTransaction providerTransaction, IFrameworkContext frameworkContext)
            : base(providerMetadata, revisions, frameworkContext)
        {
            CanWrite = true;
            Transaction = providerTransaction;
            Revisions = revisions;
            Revisions.RelatedEntitiesLoader = x => ProviderRepositoryHelper.CreateRelationLazyLoadDelegate(this, x).Invoke(x);
        }

        public bool CanWrite { get; protected set; }

        public abstract void PerformAddOrUpdate(AbstractSchemaPart entity);
        public void AddOrUpdate(AbstractSchemaPart entity)
        {
            PerformAddOrUpdate(entity);
            OnAddOrUpdateComplete(entity);
        }

        private void AutoAddRelationProxies()
        {
            if (!CanWriteRelations) return;

            var changedItems = EntitiesAddedOrUpdated;
            var flatList = changedItems
                .Where(x => typeof(IRelatableEntity).IsAssignableFrom(x.GetType()))
                .Cast<IRelatableEntity>()
                .SelectMany(y => y.RelationProxies.GetManualProxies());

            foreach (var relationProxy in flatList)
            {
                AddRelation(relationProxy.Item);
            }
        }

        private void OnAddOrUpdateComplete(AbstractSchemaPart entity)
        {
            this.SetRelationProxyLazyLoadDelegate(entity as IRelatableEntity);
            ProviderRepositoryHelper.SetProviderAliasOnId(ProviderMetadata, entity);
            EntitiesAddedOrUpdated.Add(entity);
        }

        protected internal void PrepareForCompletion()
        {
            AutoAddRelationProxies();
        }

        public abstract void Delete<T>(HiveId id) where T : AbstractSchemaPart;

        public IProviderTransaction Transaction { get; protected set; }
        private AbstractRevisionRepository<EntitySchema> _revisions;
        public new AbstractRevisionRepository<EntitySchema> Revisions
        {
            get { return _revisions; }
            set
            {
                _revisions = value;
                base.Revisions = value;
            }
        }

        public abstract bool CanWriteRelations { get; }

        protected abstract void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item);
        public void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            FrameworkContext.TaskManager.ExecuteInCancellableTask(
                this,
                item,
                TaskTriggers.Hive.Relations.PreRelationAdded,
                TaskTriggers.Hive.Relations.PostRelationAdded,
                () => PerformAddRelation(item),
                x => new HiveRelationPreActionEventArgs(x, RepositoryScopedCache),
                x => new HiveRelationPostActionEventArgs(x, RepositoryScopedCache),
                FrameworkContext);
        }
        protected abstract void PerformRemoveRelation(IRelationById item);
        public void RemoveRelation(IRelationById item)
        {
            PerformRemoveRelation(item);
        }

    }
}