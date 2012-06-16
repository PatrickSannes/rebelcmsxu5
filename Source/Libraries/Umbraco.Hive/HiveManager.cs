using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.Diagnostics;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Framework;

namespace Umbraco.Hive
{
    using Umbraco.Framework.Caching;
    using Umbraco.Hive.ProviderSupport;

    public class HiveManager : DisposableObject, IHiveManager
    {
        public HiveCounterManager PerfCounterManager { get; protected set; }

        protected HiveManager()
        {
            RepositoryContextFactory = hm => new RepositoryContext(new RuntimeCacheProvider(), new DictionaryCacheProvider(), FrameworkContext);
        }

        internal HiveManager(ProviderMappingGroup singleProvider, IFrameworkContext frameworkContext)
            : this(Enumerable.Repeat(singleProvider, 1), frameworkContext)
        {
        }

        public HiveManager(IEnumerable<ProviderMappingGroup> providerGroups, IFrameworkContext frameworkContext)
            : this()
        {
            ProviderGroups = providerGroups;
            FrameworkContext = frameworkContext;
        }

        public HiveManager(IEnumerable<ProviderMappingGroup> providerGroups, HiveCounterManager perfCounterManager, IFrameworkContext frameworkContext)
            : this(providerGroups, frameworkContext)
        {
            PerfCounterManager = perfCounterManager;
        }

        private Func<IHiveManager, RepositoryContext> RepositoryContextFactory { get; set; }

        private RepositoryContext CreateRepositoryContext()
        {
            return RepositoryContextFactory.Invoke(this);
        }

        private RepositoryContext _repositoryContext;

        public RepositoryContext Context
        {
            get
            {
                return _repositoryContext ?? (_repositoryContext = CreateRepositoryContext());
            }
        }

        private readonly Guid _managerId = Guid.NewGuid();

        public IEnumerable<ProviderMappingGroup> ProviderGroups { get; private set; }

        public IFrameworkContext FrameworkContext { get; protected set; }

        public Guid ManagerId { get { return _managerId; } }

        /// <summary>
        /// Gets all the read providers registered with this manager.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<ReadonlyProviderSetup> GetAllReadProviders()
        {
            return ProviderGroups.SelectMany(x => x.Readers).DistinctBy(x => x.ProviderMetadata.Alias);
        }

        /// <summary>
        /// Gets all read write providers registered with this manager.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<ProviderSetup> GetAllReadWriteProviders()
        {
            return ProviderGroups.SelectMany(x => x.Writers).DistinctBy(x => x.ProviderMetadata.Alias);
        }

        /// <summary>
        /// Increments a performance counter for average Hive query duration, if this instance has been given
        /// a reference to a performance counter object.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        /// <remarks>Performance counting is optional and so this instance may have been instantiated without a reference to
        /// a performance counter proxy, in which case this method fails silently.</remarks>
        private void CountResolveProviderMap(long ticks)
        {
            if (PerfCounterManager != null)
                PerfCounterManager.IncrementDuration(HivePreDefinedCounters.AverageHiveQueryDuration,
                                                      HivePreDefinedCounters.AverageHiveQueryDurationBase, ticks);
        }

        public ReadonlyGroupUnitFactory<TFilter> GetReader<TFilter>()
            where TFilter : class, IProviderTypeFilter
        {
            var group = this.GetProviderGroupByType<TFilter>();
            return new ReadonlyGroupUnitFactory<TFilter>(group.Group.Readers, group.UriMatch.Root, Context, FrameworkContext);
        }

        public GroupUnitFactory<TFilter> GetWriter<TFilter>()
            where TFilter : class, IProviderTypeFilter
        {
            var group = this.GetProviderGroupByType<TFilter>();
            return new GroupUnitFactory<TFilter>(group.Group.Writers, group.UriMatch.Root, Context, FrameworkContext);
        }

        public ReadonlyGroupUnitFactory<TFilter> GetReader<TFilter>(Uri providerMappingRoot)
            where TFilter : class, IProviderTypeFilter
        {
            var group = this.GetProviderGroup(providerMappingRoot);
            return new ReadonlyGroupUnitFactory<TFilter>(group.Group.Readers, group.UriMatch.Root, Context, FrameworkContext);
        }

        public ReadonlyGroupUnitFactory GetReader(Uri providerMappingRoot)
        {
            var group = this.GetProviderGroup(providerMappingRoot);
            return new ReadonlyGroupUnitFactory(group.Group.Readers, group.UriMatch.Root, Context, FrameworkContext);
        }

        public GroupUnitFactory<TFilter> GetWriter<TFilter>(Uri providerMappingRoot)
            where TFilter : class, IProviderTypeFilter
        {
            var group = this.GetProviderGroup(providerMappingRoot);
            return new GroupUnitFactory<TFilter>(group.Group.Writers, group.UriMatch.Root, Context, FrameworkContext);
        }

        public GroupUnitFactory GetWriter(Uri providerMappingRoot)
        {
            var group = this.GetProviderGroup(providerMappingRoot);
            return new GroupUnitFactory(group.Group.Writers, group.UriMatch.Root, Context, FrameworkContext);
        }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            Context.IfNotNull(x => x.Dispose());
        }

        #endregion
    }
}