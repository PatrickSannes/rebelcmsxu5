using System;
using Umbraco.Framework;

namespace Umbraco.Hive.ProviderSupport
{
    public class ReadonlyProviderUnitFactory : DisposableObject
    {
        public ReadonlyProviderUnitFactory(AbstractReadonlyEntityRepositoryFactory readonlyEntityRepositoryFactory, Func<AbstractScopedCache> unitScopedCacheFactory = null)
        {
            Mandate.ParameterNotNull(readonlyEntityRepositoryFactory, "readonlyEntityRepositoryFactory");
            ReadonlyEntityRepositoryFactory = readonlyEntityRepositoryFactory;
            UnitScopedCacheFactory = unitScopedCacheFactory ?? (() => new DictionaryScopedCache());
        }

        public ReadonlyProviderUnit CreateReadonly()
        {
            return new ReadonlyProviderUnit(ReadonlyEntityRepositoryFactory.GetReadonlyRepository(), UnitScopedCacheFactory());
        }

        /// <summary>
        /// Gets or sets the unit-scoped cache factory.
        /// </summary>
        /// <value>The unit-scoped cache factory.</value>
        protected Func<AbstractScopedCache> UnitScopedCacheFactory { get; set; }

        public AbstractReadonlyEntityRepositoryFactory ReadonlyEntityRepositoryFactory { get; protected set; }

        protected override void DisposeResources()
        {
            ReadonlyEntityRepositoryFactory.Dispose();
        }
    }

    // Until C# supports return type covariance, the new keyword is used here.
    // Note that the behaviour of the new keyword, in that anyone who casts to the 
    // base type will end up running the base implemnetation rather than the derived one,
    // is actually precisely what we want. The alternative is to have a harder API
    // where read-write sessions have a different property name to readonly sessions
    // - in which case, if the caller were to be calling that readonly session directly,
    // they'd precisely be getting the base implementation anyway
    // IEnumerable<T> : IEnumerable uses the 'new' keyword to change the return type of GetEnumerator()
    // http://blogs.msdn.com/b/ericlippert/archive/2008/05/21/method-hiding-apologia.aspx
}
