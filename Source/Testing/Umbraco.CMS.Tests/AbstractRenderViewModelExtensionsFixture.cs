namespace Umbraco.Tests.Cms
{
    using System;

    using NUnit.Framework;

    using Umbraco.Cms.Web.Context;

    using Umbraco.Cms.Web.Mapping;

    using Umbraco.Framework;

    using Umbraco.Framework.Persistence.Model;

    using Umbraco.Framework.Persistence.Model.Constants;

    using Umbraco.Framework.TypeMapping;

    using Umbraco.Hive;

    using Umbraco.Hive.Configuration;

    using Umbraco.Hive.RepositoryTypes;

    using Umbraco.Tests.Extensions;

    [TestFixture]
    public class AbstractRenderViewModelExtensionsFixture
    {
        protected HiveManager HiveManager;

        private ProviderMappingGroup _singleProvider;

        protected NhibernateTestSetupHelper Setup;

        private IUmbracoApplicationContext _appContext;

        private MockedMapResolverContext _resolverContext;

        [SetUp]
        public void TestSetup()
        {
            this.Setup = new NhibernateTestSetupHelper();
            this._singleProvider = new ProviderMappingGroup("default", new WildcardUriMatch(new Uri("content://")), this.Setup.ReadonlyProviderSetup, this.Setup.ProviderSetup, this.Setup.FakeFrameworkContext);
            this.HiveManager = new HiveManager(this._singleProvider, this._singleProvider.FrameworkContext);

            this._appContext = new FakeUmbracoApplicationContext(this.HiveManager, false);

            this._resolverContext = new MockedMapResolverContext(this.HiveManager.FrameworkContext, this.HiveManager, new MockedPropertyEditorFactory(this._appContext), new MockedParameterEditorFactory());

            //mappers
            var cmsModelMapper = new CmsModelMapper(this._resolverContext);
            var persistenceToRenderModelMapper = new RenderTypesModelMapper(this._resolverContext);
            
            this.Setup.FakeFrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { cmsModelMapper, persistenceToRenderModelMapper }));
        }

        protected TypedEntity AddChildNode(TypedEntity parent, TypedEntity child, int sortOrder = 0)
        {
            using (var uow = this.HiveManager.OpenWriter<IContentStore>())
            {
                parent.RelationProxies.EnlistChild(child, FixedRelationTypes.DefaultRelationType, sortOrder);
                uow.Repositories.AddOrUpdate(parent);
                uow.Repositories.AddOrUpdate(child);
                uow.Complete();
            }
            return child;
        }

        protected TypedEntity AddChildNodeWithId(TypedEntity parent, HiveId childGuid, int sortOrder = 0)
        {
            var child = HiveModelCreationHelper.MockTypedEntity(childGuid);
            return AddChildNode(parent, child, sortOrder);
        }
    }
}