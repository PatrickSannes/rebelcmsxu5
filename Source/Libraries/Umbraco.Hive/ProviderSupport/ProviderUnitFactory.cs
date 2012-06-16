using System;
using Umbraco.Framework;

namespace Umbraco.Hive.ProviderSupport
{
    public class ProviderUnitFactory : DisposableObject
    {
        public ProviderUnitFactory(AbstractEntityRepositoryFactory entityRepositoryFactory, Func<AbstractScopedCache> unitScopedCacheFactory = null)
        {
            Mandate.ParameterNotNull(entityRepositoryFactory, "entityRepositoryFactory");
            EntityRepositoryFactory = entityRepositoryFactory;
            UnitScopedCacheFactory = unitScopedCacheFactory ?? (() => new DictionaryScopedCache());
        }

        public ProviderUnit Create()
        {
            return new ProviderUnit(EntityRepositoryFactory.GetRepository(), UnitScopedCacheFactory());
        }

        /// <summary>
        /// Gets or sets the unit-scoped cache factory.
        /// </summary>
        /// <value>The unit-scoped cache factory.</value>
        protected Func<AbstractScopedCache> UnitScopedCacheFactory { get; set; }

        public AbstractEntityRepositoryFactory EntityRepositoryFactory { get; protected set; }

        protected override void DisposeResources()
        {
            EntityRepositoryFactory.Dispose();
        }
    }
}