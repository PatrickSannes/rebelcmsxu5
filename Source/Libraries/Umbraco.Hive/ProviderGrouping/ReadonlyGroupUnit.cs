using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    public class ReadonlyGroupUnit<TFilter> : DisposableObject, IReadonlyGroupUnit<TFilter>
        where TFilter : class, IProviderTypeFilter
    {
        public ReadonlyGroupUnit(IEnumerable<ReadonlyProviderUnit> providerUnits, Uri idRoot, AbstractScopedCache scopedCache, bool canDisposeCache, IFrameworkContext frameworkContext, RepositoryContext hiveContext)
        {
            Mandate.ParameterNotNull(providerUnits, "providerUnits");
            Mandate.ParameterNotNull(idRoot, "idRoot");
            Mandate.ParameterNotNull(scopedCache, "scopedCache");

            ProviderUnits = providerUnits;
            IdRoot = idRoot;

            _canDisposeCache = canDisposeCache;
            UnitScopedCache = scopedCache;

            FrameworkContext = frameworkContext ?? providerUnits.First().EntityRepository.FrameworkContext;

            var enumerable = ProviderUnits.ToArray();
            var entityRepositories = enumerable.Select(y =>
            {
                y.EntityRepository.HiveContext = hiveContext;
                return y.EntityRepository;
            }).ToList();

            var providerEntityRevisionRepositories = enumerable.Select(y =>
            {
                y.EntityRepository.Revisions.HiveContext = hiveContext;
                return y.EntityRepository.Revisions;
            }).ToList();

            var schemaRepositories = enumerable.Select(y =>
            {
                y.EntityRepository.Schemas.HiveContext = hiveContext;
                return y.EntityRepository.Schemas;
            }).ToList();

            var providerSchemaRevisionRepositories = enumerable.Select(y =>
            {
                y.EntityRepository.Schemas.Revisions.HiveContext = hiveContext;
                return y.EntityRepository.Schemas.Revisions;
            }).ToList();

            _entitySession = new ReadonlyEntityRepositoryGroup<TFilter>(entityRepositories,
                    providerEntityRevisionRepositories,
                    schemaRepositories,
                    providerSchemaRevisionRepositories, 
                    IdRoot,
                    UnitScopedCache,
                    FrameworkContext,
                    hiveContext);
        }

        public AbstractScopedCache UnitScopedCache { get; protected set; }
        public Uri IdRoot { get; set; }
        protected IEnumerable<ReadonlyProviderUnit> ProviderUnits { get; set; }

        private readonly IReadonlyEntityRepositoryGroup<TFilter> _entitySession;
        private readonly bool _canDisposeCache;

        public IReadonlyEntityRepositoryGroup<TFilter> Repositories
        {
            get { return _entitySession; }
        }

        public void Complete()
        {
            foreach (var providerUnit in ProviderUnits)
            {
                providerUnit.Complete();
            }
        }

        public void Abandon()
        {
            foreach (var providerUnit in ProviderUnits)
            {
                providerUnit.Abandon();
            }
        }

        protected override void DisposeResources()
        {
            //if (_entitySession.IsValueCreated)
            _entitySession.Dispose();

            if (UnitScopedCache != null && _canDisposeCache)
            {
                UnitScopedCache.ScopeComplete();
                UnitScopedCache.Dispose();
            }
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