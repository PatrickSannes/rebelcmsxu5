using NHibernate;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.NHibernate.Dependencies;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.NHibernate
{
    using System;

    [DemandsDependencies(typeof(NHibernateDemandBuilder))]
    public class EntityRepositoryFactory : AbstractEntityRepositoryFactory
    {
        public DependencyHelper NhDependencyHelper {get { return base.DependencyHelper as DependencyHelper; }}
        public RevisionRepositoryFactory NhRevisionRepositoryFactory { get { return base.RevisionRepositoryFactory as RevisionRepositoryFactory; } }
        public SchemaRepositoryFactory NhSchemaRepositoryFactory { get { return base.SchemaRepositoryFactory as SchemaRepositoryFactory; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRepositoryFactory"/> class. This constructor is used internally for unit testing where a single session must be used
        /// irrespective of the NHibernate session factory mechanisms.
        /// </summary>
        /// <param name="providerMetadata">The provider metadata.</param>
        /// <param name="revisionRepositoryFactory">The revision repository factory.</param>
        /// <param name="schemaRepositoryFactory">The schema repository factory.</param>
        /// <param name="frameworkContext">The framework context.</param>
        /// <param name="singleProvidedSession">The single provided session.</param>
        /// <param name="leaveSessionOpenOnDispose">if set to <c>true</c> [leave session open on dispose].</param>
        /// <remarks></remarks>
        internal EntityRepositoryFactory(ProviderMetadata providerMetadata, 
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory, 
            AbstractSchemaRepositoryFactory schemaRepositoryFactory, 
            IFrameworkContext frameworkContext, ISession singleProvidedSession, bool leaveSessionOpenOnDispose)
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, new DependencyHelper(new NhFactoryHelper(null, singleProvidedSession, leaveSessionOpenOnDispose, false, frameworkContext), providerMetadata))
        {
        }

        //public EntityRepositoryFactory(ProviderMetadata providerMetadata, 
        //    ProviderRevisionSessionFactory<TypedEntity> revisionRepositoryFactory, 
        //    ProviderSchemaSessionFactory schemaRepositoryFactory,
        //    IFrameworkContext frameworkContext, global::NHibernate.Cfg.Configuration nhConfig)
        //    : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext)
        //{
        //    Helper = new NhFactoryHelper(nhConfig, null, false, false, frameworkContext);
        //}

        public EntityRepositoryFactory(ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory,
            AbstractSchemaRepositoryFactory schemaRepositoryFactory,
            IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, dependencyHelper)
        {
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            RevisionRepositoryFactory.Dispose();
            SchemaRepositoryFactory.Dispose();
            DependencyHelper.Dispose();
        }

        public override AbstractReadonlyEntityRepository GetReadonlyRepository()
        {
            return CreateEntityRepository(true);
        }

        public override AbstractEntityRepository GetRepository()
        {
            return CreateEntityRepository(false);
        }

        protected virtual AbstractEntityRepository CreateEntityRepository(bool isReadOnly)
        {
            if (NhDependencyHelper == null)
            {
                var extra = "(null)";

                if (DependencyHelper != null)
                {
                    extra = DependencyHelper.GetType().Name;
                    if (DependencyHelper.ProviderMetadata != null) extra += " with key " + DependencyHelper.ProviderMetadata.Alias.IfNullOrWhiteSpace("(no key)");
                }

                throw new NullReferenceException("NhDependencyHelper is null and DependencyHelper is " + extra);
            }

            if (NhDependencyHelper.FactoryHelper == null)
                throw new NullReferenceException("NhDependencyHelper.FactoryHelper is null");

            ISession session;
            var transaction = NhDependencyHelper.FactoryHelper.GenerateSessionAndTransaction(isReadOnly, out session);

            var schemaRepository = NhSchemaRepositoryFactory != null
                                       ? NhSchemaRepositoryFactory.GetRepository(transaction, isReadOnly)
                                       : SchemaRepositoryFactory.GetRepository();

            var revisionRepository = NhRevisionRepositoryFactory != null
                                         ? NhRevisionRepositoryFactory.GetRepository(transaction, isReadOnly)
                                         : RevisionRepositoryFactory.GetRepository();

            return new EntityRepository(ProviderMetadata, schemaRepository, revisionRepository, transaction, session, FrameworkContext, isReadOnly);
        }
    }
}
