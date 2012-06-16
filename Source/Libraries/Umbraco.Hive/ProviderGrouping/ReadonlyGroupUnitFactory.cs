using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    using Umbraco.Hive.ProviderSupport;

    public class ReadonlyGroupUnitFactory<TFilter> : ReadonlyGroupUnitFactory
        where TFilter : class, IProviderTypeFilter
    {
        public ReadonlyGroupUnitFactory(IEnumerable<ReadonlyProviderSetup> providers, Uri idRoot,RepositoryContext hiveContext, IFrameworkContext frameworkContext)
            : base(providers, idRoot, hiveContext, frameworkContext)
        {
        }

        public virtual IReadonlyGroupUnit<TFilter> CreateReadonly()
        {
            return base.CreateReadonly<TFilter>();
        }
    }


    public class ReadonlyGroupUnitFactory : IRequiresFrameworkContext, IRequiresHiveContext 
    {
        public ReadonlyGroupUnitFactory(IEnumerable<ReadonlyProviderSetup> providers, Uri idRoot, RepositoryContext hiveContext, IFrameworkContext frameworkContext, Func<AbstractScopedCache> unitScopedCacheFactory = null)
        {
            Mandate.ParameterNotNull(idRoot, "idRoot");
            Mandate.ParameterNotNull(providers, "providers");

            IdRoot = idRoot;
            Providers = providers;
            FrameworkContext = frameworkContext ?? providers.First().FrameworkContext;
            UnitScopedCacheFactory = unitScopedCacheFactory ?? (() => new DictionaryScopedCache());
            HiveContext = hiveContext;
        }

        public Uri IdRoot { get; protected set; }

        protected IEnumerable<ReadonlyProviderSetup> Providers { get; set; }

        protected Func<AbstractScopedCache> UnitScopedCacheFactory { get; set; }

        public virtual IReadonlyGroupUnit<TFilter> CreateReadonly<TFilter>(AbstractScopedCache overrideCacheScope = null)
            where TFilter : class, IProviderTypeFilter
        {
            // We need to check for providers that actually have a unit factory; in some cases
            // the unit factory can be null (e.g. during installation when the provider can't yet
            // spin up the unit factory, but can spin up the bootstrapper)
            var readonlyProviderUnits = Providers
                .Where(x => x.ReadonlyUnitFactory != null)
                .Select(x => x.ReadonlyUnitFactory.CreateReadonly())
                .ToArray();
                // MUST enumerable the selection only once, otherwise every time it's enumerated it will call Select again, and create a new unit of work with new sessions etc.

            var unitScopedCacheFactory = overrideCacheScope ?? UnitScopedCacheFactory();
            return new ReadonlyGroupUnit<TFilter>(readonlyProviderUnits, IdRoot, unitScopedCacheFactory, overrideCacheScope == null, FrameworkContext, HiveContext);
        }

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; protected set; }

        #endregion

        #region Implementation of IRequiresHiveContext

        /// <summary>
        /// Gets or sets the repository context.
        /// </summary>
        /// <value>The repository context.</value>
        public RepositoryContext HiveContext { get; protected set; }

        #endregion
    }
}