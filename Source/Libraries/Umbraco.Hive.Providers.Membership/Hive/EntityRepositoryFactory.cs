using System;
using System.Collections.Generic;
using System.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.Providers.Membership.Config;

namespace Umbraco.Hive.Providers.Membership.Hive
{
    [DemandsDependencies(typeof(MembershipDemandBuilder))]
    public class EntityRepositoryFactory : AbstractEntityRepositoryFactory
    {
        public DependencyHelper MembershipDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }
        //public SchemaRepositoryFactory MembershipSchemaRepositoryFactory { get { return base.SchemaRepositoryFactory as SchemaRepositoryFactory; } }

        /// <summary>
        /// Internal constructor for testing
        /// </summary>
        /// <param name="providerMetadata"></param>
        /// <param name="revisionRepositoryFactory"></param>
        /// <param name="schemaRepositoryFactory"></param>
        /// <param name="frameworkContext"></param>
        /// <param name="membershipProviders"></param>
        /// <param name="configuredProviders"></param>
        internal EntityRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory,
            AbstractSchemaRepositoryFactory schemaRepositoryFactory,
            IFrameworkContext frameworkContext,
            Lazy<IEnumerable<MembershipProvider>> membershipProviders,
            IEnumerable<ProviderElement> configuredProviders)
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, new DependencyHelper(configuredProviders, membershipProviders, providerMetadata))
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
            return new EntityRepository(
                ProviderMetadata, 
                FrameworkContext,
                MembershipDependencyHelper.MembershipProviders.Value,
                MembershipDependencyHelper.ConfiguredProviders);
        }
    }
}
