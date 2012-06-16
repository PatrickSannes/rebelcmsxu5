using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Mapping;
using Umbraco.Cms.Web.Mvc.ModelBinders.BackOffice;
using Umbraco.Framework;
using Umbraco.Framework.TypeMapping;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Cms.Mvc.ModelBinders
{
    [TestFixture]
    public class HiveIdModelBinderTest : ModelBinderTest
    {
        [Test]
        public void HiveIdModelBinder_Bind_Int32_By_Route_Value()
        {
            //create the route values

            var routeData = new RouteData();
            routeData.Values.Add("id", 2);

            //bind

            ModelBindingContext bindingContext;
            ControllerContext controllerContext;
            var nodeId = ExecuteBinding(new FormCollection(), routeData, out bindingContext, out controllerContext);

            //assert

            Assert.IsTrue(nodeId.Value.Type == HiveIdValueTypes.Int32);
            Assert.AreEqual(2, (int)nodeId.Value);
        }

        [Test]
        public void HiveIdModelBinder_Bind_Guid_By_Route_Value()
        {
            //create the route values

            var guid = Guid.NewGuid();
            var routeData = new RouteData();
            routeData.Values.Add("id", guid.ToString());

            //bind

            ModelBindingContext bindingContext;
            ControllerContext controllerContext;
            var nodeId = ExecuteBinding(new FormCollection(), routeData, out bindingContext, out controllerContext);

            //assert

            Assert.IsTrue(nodeId.Value.Type == HiveIdValueTypes.Guid);
            Assert.AreEqual(guid, (Guid)nodeId.Value);
        }

        [Test]
        public void HiveIdModelBinder_Bind_Guid_By_Form_Value()
        {
            //create the form values

            var guid = Guid.NewGuid();
            var form = new FormCollection
                           {
                               { "id" , guid.ToString() }
                           };

            //bind

            ModelBindingContext bindingContext;
            ControllerContext controllerContext;
            var nodeId = ExecuteBinding(form, new RouteData(), out bindingContext, out controllerContext);

            //assert

            Assert.IsTrue(nodeId.Value.Type == HiveIdValueTypes.Guid);
            Assert.AreEqual(guid, (Guid)nodeId.Value);
        }

        [Test]
        public void HiveIdModelBinder_Bind_Id_By_Form_Value()
        {
            //create the form values

            var form = new FormCollection
                           {
                               { "id" , 2.ToString() }
                           };

            //bind

            ModelBindingContext bindingContext;
            ControllerContext controllerContext;
            var nodeId = ExecuteBinding(form, new RouteData(), out bindingContext, out controllerContext);

            //assert

            Assert.IsTrue(nodeId.Value.Type == HiveIdValueTypes.Int32);
            Assert.AreEqual(2, (int)nodeId.Value);
        }

        /// <summary>
        /// Returns a standard binding context for the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private static ModelBindingContext GetBindingContext(HiveId node, IValueProvider values)
        {
            var bindingContext = new ModelBindingContext()
                                     {
                                         ModelName = string.Empty,
                                         ValueProvider = values,
                                         ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => node, node.GetType()),
                                         ModelState = new ModelStateDictionary(),
                                         FallbackToEmptyPrefix = true
                                     };
            return bindingContext;
        }

        private HiveId ExecuteBinding(IValueProvider form, RouteData routeData, out ModelBindingContext bindingContext, out ControllerContext controllerContext)
        {

            var modelBinder = new HiveIdModelBinder();
            var httpContextFactory = new FakeHttpContextFactory("~/Umbraco/Editors/ContentEditor/Edit/1", routeData);



            var fakeFrameworkContext = new FakeFrameworkContext();
            var hive = FakeHiveCmsManager.New(fakeFrameworkContext);
            var appContext = new FakeUmbracoApplicationContext(hive);
            var resolverContext = new MockedMapResolverContext(fakeFrameworkContext, hive, new MockedPropertyEditorFactory(appContext), new MockedParameterEditorFactory());
            var webmModelMapper = new CmsModelMapper(resolverContext);

            fakeFrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { webmModelMapper, new FrameworkModelMapper(fakeFrameworkContext) }));
            var umbracoApplicationContext = new FakeUmbracoApplicationContext(hive);

            controllerContext = new ControllerContext(
                httpContextFactory.RequestContext,
                new ContentEditorController(new FakeBackOfficeRequestContext(umbracoApplicationContext)));

            //put both the form and route values in the value provider
            var routeDataValueProvider = new RouteDataValueProvider(controllerContext);
            var values = new ValueProviderCollection(new List<IValueProvider>()
                                                         {
                                                             form, 
                                                             routeDataValueProvider
                                                         });

            bindingContext = GetBindingContext(new HiveId(1), values);

            //do the binding!

            var model = modelBinder.BindModel(controllerContext, bindingContext);

            //assert!

            Assert.That(model, Is.InstanceOf<HiveId>(), "Model isn't a HiveId");
            var boundModel = (HiveId)model;
            return boundModel;
        }
    }
}