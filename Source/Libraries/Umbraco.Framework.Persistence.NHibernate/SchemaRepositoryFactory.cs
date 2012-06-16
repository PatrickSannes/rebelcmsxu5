using NHibernate;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.NHibernate.Dependencies;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.NHibernate
{
    public class SchemaRepositoryFactory : AbstractSchemaRepositoryFactory
    {
        public DependencyHelper NhDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }


        internal SchemaRepositoryFactory(ProviderMetadata providerMetadata, AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory, 
            IFrameworkContext frameworkContext, ISession singleProvidedSession, bool leaveSessionOpenOnDispose)
            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, new DependencyHelper(new NhFactoryHelper(null, singleProvidedSession, leaveSessionOpenOnDispose, false, frameworkContext), providerMetadata))
        {
        }

        public SchemaRepositoryFactory(ProviderMetadata providerMetadata, AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory, IFrameworkContext frameworkContext,
            ProviderDependencyHelper dependencyHelper) 
            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, dependencyHelper)
        {
        }

        protected override void DisposeResources()
        {
            NhDependencyHelper.Dispose();
        }

        public override AbstractReadonlySchemaRepository GetReadonlyRepository()
        {
            return CreateSchemaRepository(true);
        }

        /// <summary>
        /// Get the repository with an existing transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="isReadonly"> </param>
        /// <returns></returns>
        public AbstractSchemaRepository GetRepository(NhProviderTransaction transaction, bool isReadonly)
        {
            var session = NhDependencyHelper.FactoryHelper.GetSessionFromTransaction(transaction, isReadonly);
            return new SchemaRepository(ProviderMetadata, RevisionRepositoryFactory.GetRepository(), transaction, session, FrameworkContext, isReadonly);
        }

        public override AbstractSchemaRepository GetRepository()
        {
            return CreateSchemaRepository(false);
        }

        protected virtual AbstractSchemaRepository CreateSchemaRepository(bool isReadOnly)
        {
            ISession session;
            var transaction = NhDependencyHelper.FactoryHelper.GenerateSessionAndTransaction(isReadOnly, out session);

            //TODO: When we support schema revisions, we'll have to pass the existing transaction into the GetRepository method just 
            // like we do in the EntityRepositoryFactory

            return new SchemaRepository(ProviderMetadata, RevisionRepositoryFactory.GetRepository(), transaction, session, FrameworkContext, isReadOnly);
        }
    }
}