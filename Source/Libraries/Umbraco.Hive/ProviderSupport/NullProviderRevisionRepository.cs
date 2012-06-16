using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public sealed class NullProviderRevisionRepository<T> : AbstractRevisionRepository<T>
        where T : class, IVersionableEntity
    {
        public NullProviderRevisionRepository(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, IReadonlyProviderRepository<T> fallback = null) 
            : base(providerMetadata, new NullProviderTransaction(), frameworkContext)
        {
            CanRead = false;
            CanWrite = false;
            FallbackProvider = fallback;
        }

        /// <summary>
        /// Gets or sets the fallback provider which is used to load items as a faked revision, where callers expect to load a revision directly.
        /// </summary>
        /// <value>The fallback provider.</value>
        public IReadonlyProviderRepository<T> FallbackProvider { get; set; }

        protected override void DisposeResources()
        {
            return;
        }

        public override Revision<TEntity> PerformGet<TEntity>(HiveId entityId, HiveId revisionId)
        {
            return FallbackProvider != null ? new Revision<TEntity>(FallbackProvider.Get<TEntity>(entityId)) : null;
        }

        public override IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>()
        {
            return Enumerable.Empty<Revision<TEntity>>().AsQueryable();
        }

        public override void PerformAdd<TEntity>(Revision<TEntity> revision)
        {
            if (FallbackProvider != null)
            {
                var writeable = FallbackProvider as IProviderRepository<T>;
                if (writeable != null)
                {
                    writeable.AddOrUpdate(revision.Item);
                }
            }
        }

        public override EntitySnapshot<TEntity> PerformGetLatestSnapshot<TEntity>(HiveId hiveId, RevisionStatusType revisionStatusType = null)
        {
            return FallbackProvider != null ? new EntitySnapshot<TEntity>(PerformGet<TEntity>(hiveId, HiveId.Empty)) : null;
        }

        public override Revision<TEntity> PerformGetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null)
        {
            return FallbackProvider != null ? new Revision<TEntity>(FallbackProvider.Get<TEntity>(entityId)) : null;
        }

        public override IEnumerable<Revision<TEntity>> PerformGetAll<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null)
        {
            return Enumerable.Empty<Revision<TEntity>>();
        }

        public override EntitySnapshot<TEntity> GetSnapshot<TEntity>(HiveId hiveId, HiveId revisionId)
        {
            return FallbackProvider != null ? new EntitySnapshot<TEntity>(PerformGet<TEntity>(hiveId, HiveId.Empty)) : null;
        }
    }
}
