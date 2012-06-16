using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.Examine.Hive
{
    public class RevisionRepositoryFactory : AbstractRevisionRepositoryFactory<TypedEntity>
    {
        public DependencyHelper ExamineDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }

        /// <summary>
        /// Internal constructor for testing
        /// </summary>
        /// <param name="providerMetadata"></param>
        /// <param name="frameworkContext"></param>
        /// <param name="helper"></param>
        internal RevisionRepositoryFactory(
            ProviderMetadata providerMetadata,
            IFrameworkContext frameworkContext,
            ExamineHelper helper)
            : base(providerMetadata, frameworkContext, new DependencyHelper(helper, providerMetadata))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="providerMetadata"></param>
        /// <param name="frameworkContext"></param>
        /// <param name="dependencyHelper"></param>
        public RevisionRepositoryFactory(
            ProviderMetadata providerMetadata,
            IFrameworkContext frameworkContext,
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
        }

        /// <summary>
        /// Returns a respository with the given transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public AbstractRevisionRepository<TypedEntity> GetRepository(ExamineTransaction transaction)
        {
            return new RevisionRepository(
                ProviderMetadata,
                transaction,
                FrameworkContext,
                ExamineDependencyHelper.ExamineHelper);
        }

        public override AbstractRevisionRepository<TypedEntity> GetRepository()
        {
            var transaction = new ExamineTransaction(ExamineDependencyHelper.ExamineHelper.ExamineManager, ProviderMetadata, FrameworkContext);
            return GetRepository(transaction);
        }

        protected override void DisposeResources()
        {
            ExamineDependencyHelper.Dispose();
        }
        
        public override AbstractReadonlyRevisionRepository<TypedEntity> GetReadonlyRepository()
        {
            return GetRepository();
        }
    }
}