using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    using Umbraco.Hive.ProviderSupport;

    public class GroupUnitFactory<TFilter> : GroupUnitFactory
        where TFilter : class, IProviderTypeFilter
    {
        public GroupUnitFactory(IEnumerable<ProviderSetup> providers, Uri idRoot, RepositoryContext hiveContext, IFrameworkContext frameworkContext)
            : base(providers, idRoot, hiveContext, frameworkContext)
        {
        }

        public virtual IGroupUnit<TFilter> Create()
        {
            return base.Create<TFilter>();
        }
    }

    public class GroupUnitFactory : IRequiresFrameworkContext, IRequiresHiveContext
    {
        public GroupUnitFactory(ProviderSetup singleProvider, Uri idRoot, RepositoryContext hiveContext, IFrameworkContext frameworkContext = null)
            : this(Enumerable.Repeat(singleProvider, 1), idRoot, hiveContext, frameworkContext ?? singleProvider.FrameworkContext)
        {
            Mandate.ParameterNotNull(singleProvider, "singleProvider");
        }

        public GroupUnitFactory(IEnumerable<ProviderSetup> providers, Uri idRoot, RepositoryContext hiveContext, IFrameworkContext frameworkContext, Func<AbstractScopedCache> unitScopedCacheFactory = null)
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

        protected IEnumerable<ProviderSetup> Providers { get; set; }

        protected Func<AbstractScopedCache> UnitScopedCacheFactory { get; set; }

        public virtual IGroupUnit<TFilter> Create<TFilter>(AbstractScopedCache overrideCacheScope = null)
            where TFilter : class, IProviderTypeFilter
        {
            // We need to check for providers that actually have a unit factory; in some cases
            // the unit factory can be null (e.g. during installation when the provider can't yet
            // spin up the unit factory, but can spin up the bootstrapper)
            var providerUnits = Providers
                .Where(x => x.UnitFactory != null)
                .Select(x => x.UnitFactory.Create())
                .ToArray();
                // MUST enumerable the selection only once, otherwise every time it's enumerated it will call Select again, and create a new unit of work with new sessions etc.

            var unitScopedCacheFactory = overrideCacheScope ?? UnitScopedCacheFactory();
            return new GroupUnit<TFilter>(providerUnits, IdRoot, unitScopedCacheFactory, overrideCacheScope == null, FrameworkContext, HiveContext);
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