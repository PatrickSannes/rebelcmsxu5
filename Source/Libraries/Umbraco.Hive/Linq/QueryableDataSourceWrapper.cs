using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Tasks;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Hive.Linq
{
    using Umbraco.Framework.Caching;
    using Umbraco.Framework.Linq;

    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;
    using Umbraco.Hive.Caching;

    public class QueryableDataSourceWrapper : IQueryableDataSource, IRequiresHiveContext
    {
        private readonly IEnumerable<AbstractReadonlyEntityRepository> _repositoryPool;
        private readonly ICoreReadonlyRelationsRepository _groupRepo;
        private readonly Uri _idRoot;

        public QueryableDataSourceWrapper(IEnumerable<AbstractReadonlyEntityRepository> repositoryPool, 
            ICoreReadonlyRelationsRepository groupRepo, 
            Uri idRoot,
            IFrameworkContext context,
            AbstractScopedCache containerScopedCache,
            RepositoryContext hiveContext)
        {
            _idRoot = idRoot;
            _groupRepo = groupRepo;
            _repositoryPool = repositoryPool;
            FrameworkContext = context;
            ContainerScopedCache = containerScopedCache;
            HiveContext = hiveContext;
        }

        /// <summary>
        /// Gets or sets the repository context.
        /// </summary>
        /// <value>The repository context.</value>
        public RepositoryContext HiveContext { get; set; }

        /// <summary>
        /// Gets or sets the cache supplied by the container of this query.
        /// </summary>
        /// <value>The container-scoped cache.</value>
        public AbstractScopedCache ContainerScopedCache { get; protected set; }

        public T ExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            // Run query on all providers, check for result type and sum / count etc. on all results

            // The below is broken, need a way of knowing the return type is int etc., summing them up and still returning as T
            // whilst being able to compile... Relinq is great but this one thing really riles me
            return _repositoryPool
                .Select(entityRepositoryReader => entityRepositoryReader.ExecuteScalar<T>(query, objectBinder))
                .FirstOrDefault();
        }

        public T ExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            //return ContainerScopedCache.GetOrCreateTyped(
            //    CacheKey.Create(new HiveQueryCacheKey(query)),
            //    () =>
            //    {
                    // Run query on all providers, check for result type and sum / count etc. on all results
                    var result = _repositoryPool.Select(reader => reader.ExecuteSingle<T>(query, objectBinder)).FirstOrDefault();

                    // Raise event with the result of the query
                    FrameworkContext.TaskManager.ExecuteInContext(TaskTriggers.Hive.PostQueryResultsAvailable, this, new TaskEventArgs(FrameworkContext, new HiveQueryResultEventArgs(result, query, ContainerScopedCache)));

                    // Return
                    return result;
                //});
        }

        public IEnumerable<T> ExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            //// Check if the container-scoped cache already has results for this exact query in which case return the same
            //return ContainerScopedCache.GetOrCreateTyped<IEnumerable<T>>(
            //    CacheKey.Create(new HiveQueryCacheKey(query)),
            //    () =>
            //        {
                        var totalOutput = new HashSet<T>();
                        foreach (var reader in _repositoryPool)
                        {
                            reader.ExecuteMany<T>(query, objectBinder)
                                .Cast<IReferenceByHiveId>()
                                .SkipWhile(RepositoryGroupExtensions.SkipAndMergeFromProviders(reader.ProviderMetadata, totalOutput.Cast<IReferenceByHiveId>()))
                                .ForEach(x => totalOutput.Add((T)x));
                        }
                        var results = totalOutput.Cast<IReferenceByHiveId>().DistinctBy(x => x.Id)
                            .Select(x => RepositoryGroupExtensions.ProcessIdsAndGroupRelationProxyDelegate(_groupRepo, _idRoot, x))
                            .Cast<T>();

                        // Raise event with the result of the query
                        FrameworkContext.TaskManager.ExecuteInContext(TaskTriggers.Hive.PostQueryResultsAvailable, this, new TaskEventArgs(FrameworkContext, new HiveQueryResultEventArgs(results, query, ContainerScopedCache)));

                        // Return
                        return results;
                    //});
        }

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; protected set; }

        #endregion
        }
}