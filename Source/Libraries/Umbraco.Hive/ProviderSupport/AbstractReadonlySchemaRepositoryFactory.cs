using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public abstract class AbstractReadonlySchemaRepositoryFactory
        : AbstractReadonlyRepositoryFactory<AbstractReadonlySchemaRepository>
    {
        protected AbstractReadonlySchemaRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractReadonlyRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory,
            IFrameworkContext frameworkContext,
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
            RevisionRepositoryFactory = revisionRepositoryFactory;
        }

        public AbstractReadonlyRevisionRepositoryFactory<EntitySchema> RevisionRepositoryFactory { get; set; }
    }
}