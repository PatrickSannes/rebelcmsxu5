namespace Umbraco.Hive.InMemoryProvider
{
    using Umbraco.Framework;
    using Umbraco.Framework.Context;
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.ProviderSupport._Revised;
    using Umbraco.Hive.ProviderSupport;

    [DemandsDependencies(typeof(CacheDemandBuilder))]
    public class EntityRepositoryFactory : AbstractEntityRepositoryFactory 
    {
        public EntityRepositoryFactory(ProviderMetadata providerMetadata, AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory, AbstractSchemaRepositoryFactory schemaRepositoryFactory, IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper) 
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, dependencyHelper)
        {
        }

        protected DependencyHelper CacheDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            CacheDependencyHelper.Dispose();
        }

        /// <summary>
        /// Gets an <see cref="AbstractReadonlyEntityRepository"/>. It will have only read operations.
        /// </summary>
        /// <returns></returns>
        public override AbstractReadonlyEntityRepository GetReadonlyRepository()
        {
            return GetRepository();
        }

        /// <summary>
        /// Gets the session from the factory.
        /// </summary>
        /// <returns></returns>
        public override AbstractEntityRepository GetRepository()
        {
            return new EntityRepository(ProviderMetadata, new NullProviderTransaction(), RevisionRepositoryFactory.GetRepository(), SchemaRepositoryFactory.GetRepository(), FrameworkContext, CacheDependencyHelper);
        }
    }
}
