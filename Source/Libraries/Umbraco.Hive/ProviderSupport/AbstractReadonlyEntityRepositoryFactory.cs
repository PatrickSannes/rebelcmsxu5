using System;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;

namespace Umbraco.Hive.ProviderSupport
{
    public abstract class AbstractReadonlyEntityRepositoryFactory
        : AbstractReadonlyRepositoryFactory<AbstractReadonlyEntityRepository>, IProviderReadonlyEntityRepositoryFactory
    {
        protected AbstractReadonlyEntityRepositoryFactory(ProviderMetadata providerMetadata, 
            AbstractReadonlySchemaRepositoryFactory schemaRepositoryFactory, 
            AbstractReadonlyRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory, 
            IFrameworkContext frameworkContext, 
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
            SchemaRepositoryFactory = schemaRepositoryFactory;
            RevisionRepositoryFactory = revisionRepositoryFactory;
        }

        /// <summary>
        /// Gets or sets the schema session factory.
        /// </summary>
        /// <value>The schema session factory.</value>
        public AbstractReadonlySchemaRepositoryFactory SchemaRepositoryFactory { get; protected set; }

        /// <summary>
        /// Gets or sets the revision session factory.
        /// </summary>
        /// <value>The revision session factory.</value>
        public AbstractReadonlyRevisionRepositoryFactory<TypedEntity> RevisionRepositoryFactory { get; protected set; }
    }
}