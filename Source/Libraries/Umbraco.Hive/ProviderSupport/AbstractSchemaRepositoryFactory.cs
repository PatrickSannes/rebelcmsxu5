using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public abstract class AbstractSchemaRepositoryFactory
        : AbstractReadonlySchemaRepositoryFactory,
          IProviderRepositoryFactory<AbstractSchemaRepository, AbstractReadonlySchemaRepository>
    {
        protected AbstractSchemaRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory,
            IFrameworkContext frameworkContext,
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, dependencyHelper)
        {
            RevisionRepositoryFactory = revisionRepositoryFactory;
        }

        public new AbstractRevisionRepositoryFactory<EntitySchema> RevisionRepositoryFactory { get; set; }

        #region IProviderRepositoryFactory<AbstractSchemaRepository,AbstractReadonlySchemaRepository> Members

        public abstract AbstractSchemaRepository GetRepository();

        #endregion
    }
}