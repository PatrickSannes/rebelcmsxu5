using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mapping;
using Umbraco.Cms.Web.System;
using Umbraco.Framework.TypeMapping;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Cms.Web.Editors;
using System.Web.Routing;
using Umbraco.Cms.Web.Mvc.ModelBinders.BackOffice;
using Umbraco.Framework;
using Umbraco.Framework.DataManagement;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Cms.Mvc.ModelBinders
{
    [TestClass]
    public class HiveEntityUriModelBinderTest : ModelBinderTest
    {
        [TestMethod]
        public void NodeIdModelBinder_Bind_Int_By_Route_Value()
        {
            //create the route values

            var routeData = new RouteData();
            routeData.Values.Add("id", 2);

            //bind

            ModelBindingContext bindingContext;
            ControllerContext controllerContext;
            var nodeId = ExecuteBinding(new FormCollection(), routeData, out bindingContext, out controllerContext);

            //assert

            Assert.IsTrue(nodeId.SerializationType == DataSerializationTypes.SmallInt || nodeId.SerializationType == DataSerializationTypes.LargeInt);
            Assert.AreEqual(2, nodeId.AsInt);
        }

        [TestMethod]
        public void NodeIdModelBinder_Bind_Guid_By_Route_Value()
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

            Assert.IsTrue(nodeId.SerializationType == DataSerializationTypes.Guid);
            Assert.AreEqual(guid, nodeId.AsGuid);
        }

        [TestMethod]
        public void NodeIdModelBinder_Bind_Guid_By_Form_Value()
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

            Assert.IsTrue(nodeId.SerializationType == DataSerializationTypes.Guid);
            Assert.AreEqual(guid, nodeId.AsGuid);
        }

        [TestMethod]
        public void NodeIdModelBinder_Bind_Id_By_Form_Value()
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

            Assert.IsTrue(nodeId.SerializationType == DataSerializationTypes.SmallInt || nodeId.SerializationType == DataSerializationTypes.LargeInt);
            Assert.AreEqual(2, nodeId.AsInt);
        }

        /// <summary>
        /// Returns a standard binding context for the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private static ModelBindingContext GetBindingContext<T>(T node, IValueProvider values)
            where T : class
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

        private HiveEntityUri ExecuteBinding(IValueProvider form, RouteData routeData, out ModelBindingContext bindingContext, out ControllerContext controllerContext)
        {
            
            var modelBinder = new HiveEntityUriModelBinder();
            var httpContextFactory = new FakeHttpContextFactory("~/Umbraco/Editors/ContentEditor/Edit/1", routeData);


            
            var fakeFrameworkContext = new FakeFrameworkContext();
            var hive = FakeHiveCmsManager.New(fakeFrameworkContext);
            var resolverContext = new MockedMapResolverContext(hive, new MockedPropertyEditorFactory(), new MockedParameterEditorFactory());
            var webmModelMapper = new CmsModelMapper(resolverContext);
            var cmsPersistenceModelMapper = new CmsPersistenceModelMapper(resolverContext);
            var cmsModelMapper = new CmsDomainMapper(resolverContext);

            fakeFrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractTypeMapper[] { webmModelMapper, cmsModelMapper, cmsPersistenceModelMapper }));
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

            bindingContext = GetBindingContext(new HiveEntityUri(1), values);

            //do the binding!

            var model = modelBinder.BindModel(controllerContext, bindingContext);

            //assert!

            Assert.IsInstanceOfType(model, typeof(HiveEntityUri), "Model isn't a HiveEntityUri");
            var boundModel = (HiveEntityUri)model;
            return boundModel;
        }
    }
}
