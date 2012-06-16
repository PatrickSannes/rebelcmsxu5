using System;
using System.Collections.Generic;
using Umbraco.Framework.Context;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.Diagnostics;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive
{
    using Umbraco.Hive.ProviderSupport;

    public interface IHiveManager : IDisposable
    {
        /// <summary>
        /// Gets the perf counter manager.
        /// </summary>
        /// <value>The perf counter manager.</value>
        HiveCounterManager PerfCounterManager { get; }

        /// <summary>
        /// Gets the provider groups managed by this <see cref="IHiveManager"/>.
        /// </summary>
        /// <value>The provider groups.</value>
        IEnumerable<ProviderMappingGroup> ProviderGroups { get; }

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <value>The framework context.</value>
        IFrameworkContext FrameworkContext { get; }

        /// <summary>
        /// Gets the manager instance id.
        /// </summary>
        /// <value>The manager id.</value>
        Guid ManagerId { get; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        RepositoryContext Context { get; }

        /// <summary>
        /// Gets all the read providers registered with this manager.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        IEnumerable<ReadonlyProviderSetup> GetAllReadProviders();

        /// <summary>
        /// Gets all read write providers registered with this manager.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        IEnumerable<ProviderSetup> GetAllReadWriteProviders();

        /// <summary>
        /// Gets a <see cref="ReadonlyGroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches the value with which <typeparamref name="TFilter"/> has been decorated. <typeparamref name="TFilter"/> must be decorated
        /// with a <see cref="RepositoryTypeAttribute"/> containing the provider group root Uri.
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="IProviderTypeFilter"/> used to create extension methods against this provider group.</typeparam>
        /// <returns></returns>
        ReadonlyGroupUnitFactory<TFilter> GetReader<TFilter>()
            where TFilter : class, IProviderTypeFilter;

        /// <summary>
        /// Gets a <see cref="GroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches the value with which <typeparamref name="TFilter"/> has been decorated. <typeparamref name="TFilter"/> must be decorated
        /// with a <see cref="RepositoryTypeAttribute"/> containing the provider group root Uri.
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="IProviderTypeFilter"/> used to create extension methods against this provider group.</typeparam>
        /// <returns></returns>
        GroupUnitFactory<TFilter> GetWriter<TFilter>()
            where TFilter : class, IProviderTypeFilter;

        /// <summary>
        /// Gets a <see cref="ReadonlyGroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>. <typeparamref name="TFilter"/> is ignored when searching for a matching provider group,
        /// but can still be used to assign specific extension methods to this provider group request.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        ReadonlyGroupUnitFactory<TFilter> GetReader<TFilter>(Uri providerMappingRoot)
            where TFilter : class, IProviderTypeFilter;

        /// <summary>
        /// Gets a <see cref="ReadonlyGroupUnitFactory"/> for a group of providers, by finding a matching group in the <see cref="ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>.
        /// </summary>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        ReadonlyGroupUnitFactory GetReader(Uri providerMappingRoot);

        /// <summary>
        /// Gets a <see cref="GroupUnitFactory{TFilter}"/> for a group of providers, by finding a matching group in the <see cref="ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>. <typeparamref name="TFilter"/> is ignored when searching for a matching provider group,
        /// but can still be used to assign specific extension methods to this provider group request.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        GroupUnitFactory<TFilter> GetWriter<TFilter>(Uri providerMappingRoot)
            where TFilter : class, IProviderTypeFilter;

        /// <summary>
        /// Gets a <see cref="GroupUnitFactory"/> for a group of providers, by finding a matching group in the <see cref="ProviderGroups"/> collection where the
        /// provider's mapping group root Uri matches <paramref name="providerMappingRoot"/>.
        /// </summary>
        /// <param name="providerMappingRoot">The provider mapping root.</param>
        /// <returns></returns>
        GroupUnitFactory GetWriter(Uri providerMappingRoot);
    }
}