using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public sealed class NullProviderRevisionRepositoryFactory<T> : AbstractRevisionRepositoryFactory<T>
        where T : class, IVersionableEntity
    {
        public NullProviderRevisionRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext) 
            : base(providerMetadata, frameworkContext, new NullProviderDependencyHelper(providerMetadata))
        {
        }

        private IProviderReadonlyRepositoryFactory<AbstractProviderRepository> _fallbackProviderFactory;

        /// <summary>
        /// Gets or sets the fallback provider which is used to load items as a faked revision, where callers expect to load a revision directly.
        /// </summary>
        /// <value>The fallback provider.</value>
        public IProviderReadonlyRepositoryFactory<AbstractProviderRepository> FallbackProviderFactory
        {
            get
            {
                return this._fallbackProviderFactory;
            }
            set
            {
                this._fallbackProviderFactory = value;
            }
        }

        public override AbstractRevisionRepository<T> GetRepository()
        {
            var fallback = FallbackProviderFactory != null ? FallbackProviderFactory.GetReadonlyRepository() as IReadonlyProviderRepository<T> : null;
            return new NullProviderRevisionRepository<T>(ProviderMetadata, FrameworkContext, fallback);
        }

        protected override void DisposeResources()
        {
            return;
        }

        public override AbstractReadonlyRevisionRepository<T> GetReadonlyRepository()
        {
            return GetRepository();
        }
    }
}