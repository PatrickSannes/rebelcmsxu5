using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.Diagnostics;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web
{
    using Umbraco.Framework;
    using Umbraco.Framework.Persistence.Model.Constants;
    using Umbraco.Hive.ProviderSupport;

    public interface IRenderViewHiveManagerWrapper : IHiveManager
    {
        IQueryable<Content> Content { get; }
    }

    public class RenderViewHiveManagerWrapper : DisposableObject, IRenderViewHiveManagerWrapper
    {
        private readonly IHiveManager _innerHiveManager;

        public RenderViewHiveManagerWrapper(IHiveManager manager)
        {
            _innerHiveManager = manager;
        }

        public IQueryable<Content> Content
        {
            get
            {
                using (var uow = this.OpenReader<IContentStore>())
                {
                    return uow.Repositories.Query<Content>().OfRevisionType(FixedStatusTypes.Published);
                }
            }
        }

        public IQueryable<Media> Media
        {
            get
            {
                using (var uow = this.OpenReader<IMediaStore>())
                {
                    return uow.Repositories.Query<Media>().OfRevisionType(FixedStatusTypes.Published);
                }
            }
        }

        #region Wrapped methods from inner manager
        /// <summary>
        /// Gets the perf counter manager.
        /// </summary>
        /// <value>The perf counter manager.</value>
        public HiveCounterManager PerfCounterManager
        {
            get { return _innerHiveManager.PerfCounterManager; }
        }

        /// <summary>
        /// Gets the provider groups managed by this <see cref="IHiveManager"/>.
        /// </summary>
        /// <value>The provider groups.</value>
        public IEnumerable<ProviderMappingGroup> ProviderGroups
        {
            get { return _innerHiveManager.ProviderGroups; }
        }

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <value>The framework context.</value>
        public IFrameworkContext FrameworkContext
        {
            get { return _innerHiveManager.FrameworkContext; }
        }

        /// <summary>
        /// Gets the manager instance id.
        /// </summary>
        /// <value>The manager id.</value>
        public Guid ManagerId
        {
            get { return _innerHiveManager.ManagerId; }
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public RepositoryContext Context
        {
            get
            {
                return _innerHiveManager.Context;
            }
        }

        /// <summary>
        /// Gets all the read providers registered with this manager.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<ReadonlyProviderSetup> GetAllReadProviders()
        {
            return _innerHiveManager.GetAllReadProviders();
        }

        /// <summary>
        /// Gets all read write providers registered with this manager.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerable<ProviderSetup> GetAllReadWriteProviders()
        {
            return _innerHiveManager.GetAllReadWriteProviders();
        }

        /// <summary>
        /// Gets a <see cref="ReadonlyGroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches the value with which <typeparamref name="TFilter"/> has been decorated. <typeparamref name="TFilter"/> must be decorated
        /// with a <see cref="RepositoryTypeAttribute"/> containing the provider group root Uri.
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="IProviderTypeFilter"/> used to create extension methods against this provider group.</typeparam>
        /// <returns></returns>
        public ReadonlyGroupUnitFactory<TFilter> GetReader<TFilter>() where TFilter : class, IProviderTypeFilter
        {
            return _innerHiveManager.GetReader<TFilter>();
        }

        /// <summary>
        /// Gets a <see cref="GroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches the value with which <typeparamref name="TFilter"/> has been decorated. <typeparamref name="TFilter"/> must be decorated
        /// with a <see cref="RepositoryTypeAttribute"/> containing the provider group root Uri.
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="IProviderTypeFilter"/> used to create extension methods against this provider group.</typeparam>
        /// <returns></returns>
        public GroupUnitFactory<TFilter> GetWriter<TFilter>() where TFilter : class, IProviderTypeFilter
        {
            return _innerHiveManager.GetWriter<TFilter>();
        }

        /// <summary>
        /// Gets a <see cref="ReadonlyGroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>. <typeparamref name="TFilter"/> is ignored when searching for a matching provider group,
        /// but can still be used to assign specific extension methods to this provider group request.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        public ReadonlyGroupUnitFactory<TFilter> GetReader<TFilter>(Uri providerMappingRoot) where TFilter : class, IProviderTypeFilter
        {
            return _innerHiveManager.GetReader<TFilter>(providerMappingRoot);
        }

        /// <summary>
        /// Gets a <see cref="ReadonlyGroupUnitFactory"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>.
        /// </summary>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        public ReadonlyGroupUnitFactory GetReader(Uri providerMappingRoot)
        {
            return _innerHiveManager.GetReader(providerMappingRoot);
        }

        /// <summary>
        /// Gets a <see cref="GroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>. <typeparamref name="TFilter"/> is ignored when searching for a matching provider group,
        /// but can still be used to assign specific extension methods to this provider group request.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        public GroupUnitFactory<TFilter> GetWriter<TFilter>(Uri providerMappingRoot) where TFilter : class, IProviderTypeFilter
        {
            return _innerHiveManager.GetWriter<TFilter>(providerMappingRoot);
        }

        /// <summary>
        /// Gets a <see cref="GroupUnitFactory"/> for a group of providers, by finding a matching group in the <see cref="IHiveManager.ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>.
        /// </summary>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        public GroupUnitFactory GetWriter(Uri providerMappingRoot)
        {
            return _innerHiveManager.GetWriter(providerMappingRoot);
        }

        #endregion

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _innerHiveManager.IfNotNull(x => x.Dispose());
        }

        #endregion
    }
}
