using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.Examine.Hive
{
    [DemandsDependencies(typeof(ExamineDemandBuilder))]
    public class EntityRepositoryFactory : AbstractEntityRepositoryFactory
    {
        public DependencyHelper ExamineDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }
        public RevisionRepositoryFactory ExamineRevisionRepositoryFactory { get { return base.RevisionRepositoryFactory as RevisionRepositoryFactory; } }
        public SchemaRepositoryFactory ExamineSchemaRepositoryFactory { get { return base.SchemaRepositoryFactory as SchemaRepositoryFactory; } }

        /// <summary>
        /// Internal constructor for testing
        /// </summary>
        /// <param name="providerMetadata"></param>
        /// <param name="revisionRepositoryFactory"></param>
        /// <param name="schemaRepositoryFactory"></param>
        /// <param name="frameworkContext"></param>
        /// <param name="helper"></param>
        internal EntityRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory,
            AbstractSchemaRepositoryFactory schemaRepositoryFactory,
            IFrameworkContext frameworkContext,
            ExamineHelper helper)
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, new DependencyHelper(helper, providerMetadata))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="providerMetadata"></param>
        /// <param name="revisionRepositoryFactory"></param>
        /// <param name="schemaRepositoryFactory"></param>
        /// <param name="frameworkContext"></param>
        /// <param name="dependencyHelper"></param>
        public EntityRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory,
            AbstractSchemaRepositoryFactory schemaRepositoryFactory,
            IFrameworkContext frameworkContext,
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, dependencyHelper)
        {
        }

        protected override void DisposeResources()
        {
            RevisionRepositoryFactory.Dispose();
            SchemaRepositoryFactory.Dispose();
            DependencyHelper.Dispose();
        }

        public override AbstractReadonlyEntityRepository GetReadonlyRepository()
        {
            return GetRepository();
        }

        public override AbstractEntityRepository GetRepository()
        {
            var transaction = new ExamineTransaction(ExamineDependencyHelper.ExamineHelper.ExamineManager, ProviderMetadata, FrameworkContext);

            return new EntityRepository(
                ProviderMetadata, 
                transaction,                
                FrameworkContext,
                ExamineRevisionRepositoryFactory.GetRepository(transaction),
                ExamineSchemaRepositoryFactory.GetRepository(transaction), 
                ExamineDependencyHelper.ExamineHelper);
        }
    }
}
