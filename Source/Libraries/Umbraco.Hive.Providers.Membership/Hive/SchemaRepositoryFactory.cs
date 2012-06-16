//using System;
//using System.Collections.Generic;
//using System.Web.Security;
//using Umbraco.Framework.Context;
//using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
//using Umbraco.Framework.Persistence.ProviderSupport._Revised;
//using Umbraco.Hive.ProviderSupport;

//namespace Umbraco.Hive.Providers.Membership.Hive
//{
//    public class SchemaRepositoryFactory : AbstractSchemaRepositoryFactory
//    {
//        public DependencyHelper MembershipDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }

//        /// <summary>
//        /// Constructor used for testing
//        /// </summary>
//        /// <param name="providerMetadata"></param>
//        /// <param name="revisionRepositoryFactory"></param>
//        /// <param name="frameworkContext"></param>
//        /// <param name="membershipProviders"></param>
//        internal SchemaRepositoryFactory(
//            ProviderMetadata providerMetadata,
//            AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory,
//            IFrameworkContext frameworkContext,
//            Lazy<IEnumerable<MembershipProvider>> membershipProviders)
//            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, new DependencyHelper(membershipProviders, providerMetadata))
//        {
//        }

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="providerMetadata"></param>
//        /// <param name="revisionRepositoryFactory"></param>
//        /// <param name="frameworkContext"></param>
//        /// <param name="dependencyHelper"></param>
//        public SchemaRepositoryFactory(
//            ProviderMetadata providerMetadata,
//            AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory, 
//            IFrameworkContext frameworkContext, 
//            ProviderDependencyHelper dependencyHelper)
//            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, dependencyHelper)
//        {
//        }

//        protected override void DisposeResources()
//        {
//            RevisionRepositoryFactory.Dispose();
//            MembershipDependencyHelper.Dispose();
//        }

//        public override AbstractReadonlySchemaRepository GetReadonlyRepository()
//        {
//            return GetRepository();
//        }


//        public override AbstractSchemaRepository GetRepository()
//        {
//            return new SchemaRepository(ProviderMetadata, FrameworkContext);

//        }
//    }
//}
