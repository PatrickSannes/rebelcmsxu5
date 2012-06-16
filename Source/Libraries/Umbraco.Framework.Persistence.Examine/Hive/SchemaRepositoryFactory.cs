using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.Examine.Hive
{
    public class SchemaRepositoryFactory : AbstractSchemaRepositoryFactory
    {
        public DependencyHelper ExamineDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }

        /// <summary>
        /// Constructor used for testing
        /// </summary>
        /// <param name="providerMetadata"></param>
        /// <param name="revisionRepositoryFactory"></param>
        /// <param name="frameworkContext"></param>
        /// <param name="helper"></param>
        internal SchemaRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory,
            IFrameworkContext frameworkContext,
            ExamineHelper helper)
            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, new DependencyHelper(helper, providerMetadata))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="providerMetadata"></param>
        /// <param name="revisionRepositoryFactory"></param>
        /// <param name="frameworkContext"></param>
        /// <param name="dependencyHelper"></param>
        public SchemaRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory, 
            IFrameworkContext frameworkContext, 
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, dependencyHelper)
        {
        }

        protected override void DisposeResources()
        {
            RevisionRepositoryFactory.Dispose();
            ExamineDependencyHelper.Dispose();
        }

        public override AbstractReadonlySchemaRepository GetReadonlyRepository()
        {
            return GetRepository();
        }

        /// <summary>
        /// Returns a repository using the given transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public AbstractSchemaRepository GetRepository(ExamineTransaction transaction)
        {
            return new SchemaRepository(
                ProviderMetadata,
                transaction,
                FrameworkContext,
                ExamineDependencyHelper.ExamineHelper);
        }

        public override AbstractSchemaRepository GetRepository()
        {
            var transaction = new ExamineTransaction(ExamineDependencyHelper.ExamineHelper.ExamineManager, ProviderMetadata, FrameworkContext);
            return GetRepository(transaction);

        }
    }
}
