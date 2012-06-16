using System;
using System.IO;
using System.Web.Mvc;
using Rhino.Mocks;
using Umbraco.Cms;
using Umbraco.Cms.Packages.DevDataset;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Mapping;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.Metadata;
using Umbraco.Cms.Web.Mvc.ModelBinders;
using Umbraco.Cms.Web.Mvc.ModelBinders.BackOffice;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.Providers.IO;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Cms
{
    /// <summary>
    /// An abstract test class that ensures that all framework/application contexts are setup properly
    /// </summary>
    public abstract class StandardWebTest
    {
        protected FakeUmbracoApplicationContext UmbracoApplicationContext { get; private set; }
        protected DevDataset DevDataset { get; private set; }
        protected FakeFrameworkContext FrameworkContext { get; private set; }
        protected FixedPropertyEditors FixedPropertyEditors { get; private set; }

        /// <summary>
        /// Creates a new routable request context with everything wired up
        /// </summary>
        protected virtual IBackOfficeRequestContext GetBackOfficeRequestContext()
        {
            return new FakeBackOfficeRequestContext(UmbracoApplicationContext);
        }

        protected virtual void Init()
        {
            var attributeTypeRegistry = new CmsAttributeTypeRegistry();
            AttributeTypeRegistry.SetCurrent(attributeTypeRegistry);

            FrameworkContext = new FakeFrameworkContext();

            var hive = CreateHiveManager();
            
            UmbracoApplicationContext = CreateApplicationContext(hive);

            var resolverContext = new MockedMapResolverContext(FrameworkContext, hive, new MockedPropertyEditorFactory(UmbracoApplicationContext), new MockedParameterEditorFactory());
            var webmModelMapper = new CmsModelMapper(resolverContext);

            FrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { webmModelMapper, new FrameworkModelMapper(FrameworkContext) }));

            DevDataset = DemoDataHelper.GetDemoData(UmbracoApplicationContext, attributeTypeRegistry);
            FixedPropertyEditors = new FixedPropertyEditors(UmbracoApplicationContext);

            //ensure model binders
            ModelBinders.Binders.Remove(typeof(HiveId));
            ModelBinders.Binders.Add(typeof(HiveId), new HiveIdModelBinder());
            ModelBinders.Binders.Remove(typeof(DocumentTypeEditorModel));
            ModelBinders.Binders.Add(typeof(DocumentTypeEditorModel), new DocumentTypeModelBinder());
            ModelBinders.Binders.Remove(typeof(SizeModelBinder));
            ModelBinders.Binders.Add(typeof(SizeModelBinder), new SizeModelBinder());

            //set the model meta data provider
            ModelMetadataProviders.Current = new UmbracoModelMetadataProvider();
        }
        
        /// <summary>
        /// Returns a FakeUmbracoApplicationContext by default with the Root node created
        /// </summary>
        /// <param name="hive"></param>
        /// <returns></returns>
        protected virtual FakeUmbracoApplicationContext CreateApplicationContext(IHiveManager hive)
        {
            return new FakeUmbracoApplicationContext(hive);
        }

        /// <summary>
        /// Returns a HiveManager configured with an in memory NH provider by default
        /// </summary>
        /// <returns></returns>
        protected virtual IHiveManager CreateHiveManager()
        {            
            return FakeHiveCmsManager.NewWithNhibernate(new[] { FakeHiveCmsManager.CreateFakeTemplateMappingGroup(FrameworkContext)}, FrameworkContext);
        }
    }
}