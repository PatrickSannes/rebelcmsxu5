using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public abstract class AbstractRevisionRepositoryFactory<T>
        : AbstractReadonlyRevisionRepositoryFactory<T>, 
          IProviderRepositoryFactory<AbstractRevisionRepository<T>, AbstractReadonlyRevisionRepository<T>>
        where T : class, IVersionableEntity
    {
        protected AbstractRevisionRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
        }

        public abstract AbstractRevisionRepository<T> GetRepository();
    }
}