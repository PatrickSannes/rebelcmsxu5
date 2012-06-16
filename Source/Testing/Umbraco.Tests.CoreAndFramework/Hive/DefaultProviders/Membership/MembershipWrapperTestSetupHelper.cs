using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Security;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.Testing;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.Providers.Membership.Config;
using Umbraco.Hive.Providers.Membership.Hive;
using Umbraco.Hive.Providers.Membership.Mapping;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.Membership
{
    public class MembershipWrapperTestSetupHelper
    {
        public MembershipWrapperTestSetupHelper(FakeFrameworkContext frameworkContext = null)
        {
            //clear out the Roles/Users xml files for the test membership provider
            var current = new DirectoryInfo(Common.CurrentAssemblyDirectory);
            while (current.Parent.GetDirectories("App_Data").SingleOrDefault() == null)
            {
                current = current.Parent;
            }
            var appData = current.Parent.GetDirectories("App_Data").Single();
            foreach (var f in appData.GetFiles("*.xml"))
            {
                f.Delete();
            }
            foreach (var f in appData.GetFiles("*.orig"))
            {
                f.CopyTo(Path.Combine(f.Directory.FullName, Path.GetFileNameWithoutExtension(f.Name) + ".xml"));
            }

            _fakeFrameworkContext = frameworkContext ?? new FakeFrameworkContext();
            var attributeTypeRegistry = new CmsAttributeTypeRegistry();
            var mapper = new MembershipWrapperModelMapper(attributeTypeRegistry, _fakeFrameworkContext);
            _fakeFrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { mapper, new FrameworkModelMapper(_fakeFrameworkContext) }));

            var providerMetadata = new ProviderMetadata("r-unit-tester", new Uri("tester://"), true, false);

            //need to reset our custom membership provider before each test
            var membershipProvider = global::System.Web.Security.Membership.Providers.Cast<MembershipProvider>().OfType<CustomXmlMembershipProvider>().Single();
            membershipProvider.Reset();

            var configuredProviders = new List<ProviderElement>(new[] {new ProviderElement() {Name = "test", WildcardCharacter = "*"}});

            var revisionSchemaFactory = new NullProviderRevisionRepositoryFactory<EntitySchema>(providerMetadata, _fakeFrameworkContext);
            var revisionRepositoryFactory = new NullProviderRevisionRepositoryFactory<TypedEntity>(providerMetadata, _fakeFrameworkContext);
            var schemaRepositoryFactory = new NullProviderSchemaRepositoryFactory(providerMetadata, _fakeFrameworkContext);
            var entityRepositoryFactory = new EntityRepositoryFactory(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, _fakeFrameworkContext,
                                                                      new Lazy<IEnumerable<MembershipProvider>>(() => global::System.Web.Security.Membership.Providers.Cast<MembershipProvider>()),
                                                                      configuredProviders);

            var readUnitFactory = new ReadonlyProviderUnitFactory(entityRepositoryFactory);
            var unitFactory = new ProviderUnitFactory(entityRepositoryFactory);

            ProviderSetup = new ProviderSetup(unitFactory, providerMetadata, _fakeFrameworkContext, null, 0);
            ReadonlyProviderSetup = new ReadonlyProviderSetup(readUnitFactory, providerMetadata, _fakeFrameworkContext, null, 0);
        }

        private readonly FakeFrameworkContext _fakeFrameworkContext;

        public ProviderSetup ProviderSetup { get; private set; }
        public ReadonlyProviderSetup ReadonlyProviderSetup { get; private set; }
        
        public FakeFrameworkContext FrameworkContext
        {
            get { return _fakeFrameworkContext; }
        }
    }
}