using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public abstract class AbstractReadonlyRevisionRepositoryFactory<T> 
        : AbstractReadonlyRepositoryFactory<AbstractReadonlyRevisionRepository<T>>
        where T : class, IVersionableEntity
    {
        protected AbstractReadonlyRevisionRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper) 
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
        }
    }
}