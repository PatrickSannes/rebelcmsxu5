using System;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.Areas;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using System.Web.Routing;
using MvcContrib.TestHelper;
using System.Collections.Generic;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Cms.Mvc.Routing
{
    [TestFixture]
    public class BackOfficeRouting : RoutingTest
    {
        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Tree_Search_Route()
        {
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Trees/ApplicationTree/Search")
                .ShouldMapTo<ApplicationTreeController>(x => x.Search(null));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Default_App_Route()
        {
            RouteTable.Routes.GetDataForRoute("~/Umbraco").ShouldMapTo<DefaultController>(x => x.App("content"));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Default_Standard_Route()
        {
            
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Default/Login").ShouldMapTo<DefaultController>(x => x.Login(LoginDisplayType.StandardPage));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Default_Umbraco_Editors_Routes()
        {

            var entityUri = new HiveId(123);

            RouteTable.Routes.GetDataForRoute("~/Umbraco/Editors/ContentEditor")
                .ShouldMapTo<ContentEditorController>(x => x.Dashboard(null));
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Editors/ContentEditor/Edit/" + entityUri)
                .ShouldMapTo<ContentEditorController>(x => x.Edit(entityUri));

            RouteTable.Routes.GetDataForRoute("~/Umbraco/Editors/DataTypeEditor")
                .ShouldMapTo<DataTypeEditorController>(x => x.Dashboard(null));
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Editors/DataTypeEditor/Edit/" + entityUri)
                .ShouldMapTo<DataTypeEditorController>(x => x.Edit(new HiveId(123)));

            RouteTable.Routes.GetDataForRoute("~/Umbraco/Editors/DocumentTypeEditor")
                .ShouldMapTo<DocumentTypeEditorController>(x => x.Dashboard(null));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Custom_Plugin_Editors_Routes()
        {
            //NOTE: ShouldMapTo sux and doesn't actually check parameters so passing in real object parameters doesn't work.
            // could make our own? The reason is because the default {id} param in the route is UrlParameter.Optional
            // which doesn not match a HiveId object.
            // http://stackoverflow.com/questions/1891526/mvccontrib-shouldmapto-testhelper-throws-assertionexception-unexpectedly

            var contentEditorDashboard = "~/Umbraco/TestPackage/ContentEditor";
            var mediaEditorDashboard = "~/Umbraco/TestPackage/MediaEditor";
            var contentEditorEdit = "~/Umbraco/TestPackage/ContentEditor/Edit";
            var mediaEditorEdit = "~/Umbraco/TestPackage/MediaEditor/Edit";
            //var contentEditorEditWithId = "~/Umbraco/TestPackage/ContentEditor/Edit/" + new HiveId(123); ;
            //var mediaEditorEditWithId = "~/Umbraco/TestPackage/MediaEditor/Edit/" + new HiveId(123); ;

            RouteTable.Routes.GetDataForRoute(contentEditorDashboard)
                .ShouldMapTo<Stubs.Editors.ContentEditorController>(x => x.Dashboard(null));
            RouteTable.Routes.GetDataForRoute(mediaEditorDashboard)
                .ShouldMapTo<Stubs.Editors.MediaEditorController>(x => x.Dashboard(null));
            RouteTable.Routes.GetDataForRoute(contentEditorEdit)
                .ShouldMapTo<Stubs.Editors.ContentEditorController>(x => x.Edit(null));
            RouteTable.Routes.GetDataForRoute(mediaEditorEdit)
                .ShouldMapTo<Stubs.Editors.MediaEditorController>(x => x.Edit(null));
            //RouteTable.Routes.GetDataForRoute(contentEditorEditWithId)
            //    .ShouldMapTo<Stubs.Editors.ContentEditorController>(x => x.Edit(new HiveId(123)));
            //RouteTable.Routes.GetDataForRoute(mediaEditorEditWithId)
            //    .ShouldMapTo<Stubs.Editors.MediaEditorController>(x => x.Edit(new HiveId(123)));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Custom_Plugin_Surface_Routes()
        {
            var surfaceRoute1 = "~/Umbraco/TestPackage/CustomSurface";
            var surfaceRoute2 = "~/Umbraco/TestPackage/BlahSurface";
            var surfaceRouteWithCustomAction1 = "~/Umbraco/TestPackage/CustomSurface/SomeAction";
            var surfaceRouteWithCustomAction2 = "~/Umbraco/TestPackage/BlahSurface/AnotherAction";

            RouteTable.Routes.GetDataForRoute(surfaceRoute1)
                .ShouldMapTo<Stubs.Surface.CustomSurfaceController>(x => x.Index(null));
            RouteTable.Routes.GetDataForRoute(surfaceRoute2)
                .ShouldMapTo<Stubs.Surface.BlahSurfaceController>(x => x.Index(null));

            RouteTable.Routes.GetDataForRoute(surfaceRouteWithCustomAction1)
                .ShouldMapTo<Stubs.Surface.CustomSurfaceController>(x => x.SomeAction(null));
            RouteTable.Routes.GetDataForRoute(surfaceRouteWithCustomAction2)
                .ShouldMapTo<Stubs.Surface.BlahSurfaceController>(x => x.AnotherAction(null));
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Default_Umbraco_Tree_Routes()
        {
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Trees/ApplicationTree/Index")
                .ShouldMapTo<ApplicationTreeController>(x => x.Index("content"));
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Trees/ApplicationTree/Index/media")
                .ShouldMapTo<ApplicationTreeController>(x => x.Index("media"));

            //test the default action/id
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Trees/ContentTree")
                .ShouldMapTo<ContentTreeController>(x => x.Index(HiveId.Empty, null));
            //test a full path
            var e2 = new HiveId(123);
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Trees/ContentTree/Index/" + e2)
                .ShouldMapTo<ContentTreeController>(x => x.Index(new HiveId(123), null));
            //test a second controller type
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Trees/MediaTree")
                .ShouldMapTo<MediaTreeController>(x => x.Index(HiveId.Empty, null));
            //test a second controller type with full path
            var e3 = new HiveId(456);
            RouteTable.Routes.GetDataForRoute("~/Umbraco/Trees/MediaTree/Index/" + e3)
                .ShouldMapTo<MediaTreeController>(x => x.Index(new HiveId(456), null));
            
            ////test a default id
            //var guid = Guid.NewGuid();
            //RouteTable.Routes.GetDataForRoute("~/Umbraco/Trees/DataTypeTree/Index/" + guid.ToString("N"))
            //    .ShouldMapTo<DataTypeTreeController>(x => x.Index(new HiveId(guid), null));
        }


        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Custom_Plugin_Tree_Routes()
        {
            var contentUrl = "~/Umbraco/TestPackage/ContentTree";
            var mediaUrl = "~/Umbraco/TestPackage/MediaTree";

            RouteTable.Routes.GetDataForRoute(contentUrl)
                .ShouldMapTo<Stubs.Trees.ContentTreeController>(x => x.Index(HiveId.Empty, null));
            RouteTable.Routes.GetDataForRoute(mediaUrl)
                .ShouldMapTo<Stubs.Trees.MediaTreeController>(x => x.Index(HiveId.Empty, null));

        }

     
        /// <summary>
        /// Validates that the URLs generated from the UrlHelper are what is expected
        /// </summary>
        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Ensure_Default_Editor_Url_Structures()
        {

            //Arrange

            var context = new FakeHttpContextFactory("~/empty", new RouteData());

            var contentEditor = new ContentEditorController(new FakeBackOfficeRequestContext());
            var contentControllerName = UmbracoController.GetControllerName(contentEditor.GetType());
            var contentControllerId = UmbracoController.GetControllerId<EditorAttribute>(contentEditor.GetType());

            var dataTypeEditor = new DataTypeEditorController(new FakeBackOfficeRequestContext());
            var dataTypeControllerName = UmbracoController.GetControllerName(dataTypeEditor.GetType());
            var dataTypeControllerId = UmbracoController.GetControllerId<EditorAttribute>(dataTypeEditor.GetType());

            var docTypeEditor = new DocumentTypeEditorController(new FakeBackOfficeRequestContext());
            var docTypeControllerName = UmbracoController.GetControllerName(docTypeEditor.GetType());
            var docTypeControllerId = UmbracoController.GetControllerId<EditorAttribute>(docTypeEditor.GetType());
            
            const string customAction = "Index";
            const string defaultAction = "Dashboard";
            const int id = -1;
            const string area = "Umbraco";

            //Act

            //ensure the area is passed in because we're matchin a URL in an area, otherwise it will not work

            var contentEditorDefaultUrl = UrlHelper.GenerateUrl(null, defaultAction, contentControllerName,
                new RouteValueDictionary(new { area, id = UrlParameter.Optional, editorId = contentControllerId.ToString("N") }),
                RouteTable.Routes, context.RequestContext, true);

            var dataTypeEditorDefaultUrl = UrlHelper.GenerateUrl(null, defaultAction, dataTypeControllerName,
                    new RouteValueDictionary(new { area, id = UrlParameter.Optional, editorId = dataTypeControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            var docTypeEditorDefaultUrl = UrlHelper.GenerateUrl(null, defaultAction, docTypeControllerName,
                    new RouteValueDictionary(new { area, id = UrlParameter.Optional, editorId = docTypeControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);  

            var contentEditorCustomUrl = UrlHelper.GenerateUrl(null, customAction, contentControllerName,
                new RouteValueDictionary(new { area, id, editorId = contentControllerId.ToString("N") }),                 
                RouteTable.Routes, context.RequestContext, true);
            
            var dataTypeEditorCustomUrl = UrlHelper.GenerateUrl(null, customAction, dataTypeControllerName,
                    new RouteValueDictionary(new { area, id, editorId = dataTypeControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            var docTypeEditorCustomUrl = UrlHelper.GenerateUrl(null, customAction, docTypeControllerName,
                    new RouteValueDictionary(new { area, id, editorId = docTypeControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);  

            //Assert

            Assert.AreEqual(string.Format("/Umbraco/Editors/{0}", contentControllerName), contentEditorDefaultUrl);
            Assert.AreEqual(string.Format("/Umbraco/Editors/{0}", dataTypeControllerName), dataTypeEditorDefaultUrl);
            Assert.AreEqual(string.Format("/Umbraco/Editors/{0}", docTypeControllerName), docTypeEditorDefaultUrl);
            Assert.AreEqual(string.Format("/Umbraco/Editors/{0}/{1}/{2}", contentControllerName, customAction, id), contentEditorCustomUrl);
            Assert.AreEqual(string.Format("/Umbraco/Editors/{0}/{1}/{2}", dataTypeControllerName, customAction, id), dataTypeEditorCustomUrl);
            Assert.AreEqual(string.Format("/Umbraco/Editors/{0}/{1}/{2}", docTypeControllerName, customAction, id), docTypeEditorCustomUrl);
        }

        /// <summary>
        /// Ensure the correct URL structures for plugin editors
        /// </summary>
        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Ensure_Plugin_Editor_Url_Structures()
        {

            //Arrange

            var context = new FakeHttpContextFactory("~/empty", new RouteData());

            var contentEditor = new Stubs.Editors.ContentEditorController(new FakeBackOfficeRequestContext());
            var contentControllerName = UmbracoController.GetControllerName(contentEditor.GetType());
            var contentControllerId = UmbracoController.GetControllerId<EditorAttribute>(contentEditor.GetType());

            var mediaEditorController = new Stubs.Editors.MediaEditorController(new FakeBackOfficeRequestContext());
            var mediaControllerName = UmbracoController.GetControllerName(mediaEditorController.GetType());
            var mediaControllerId = UmbracoController.GetControllerId<EditorAttribute>(mediaEditorController.GetType());

            const string customAction = "Index";
            const string defaultAction = "Dashboard";
            const int id = -1;
            const string area = "TestPackage";

            //Act

            //ensure the area is passed in because we're matchin a URL in an area, otherwise it will not work

            var contentEditorDefaultUrl = UrlHelper.GenerateUrl(null, defaultAction, contentControllerName,
                new RouteValueDictionary(new { area, id = UrlParameter.Optional, editorId = contentControllerId.ToString("N") }),
                RouteTable.Routes, context.RequestContext, true);

            var dataTypeEditorDefaulturl = UrlHelper.GenerateUrl(null, defaultAction, mediaControllerName,
                    new RouteValueDictionary(new { area, id = UrlParameter.Optional, editorId = mediaControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            var contentEditorCustomUrl = UrlHelper.GenerateUrl(null, customAction, contentControllerName,
                new RouteValueDictionary(new { area, id, editorId = contentControllerId.ToString("N") }),
                RouteTable.Routes, context.RequestContext, true);

            var dataTypeEditorCustomurl = UrlHelper.GenerateUrl(null, customAction, mediaControllerName,
                    new RouteValueDictionary(new { area, id, editorId = mediaControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            //Assert

            //Assert.AreEqual(string.Format("/Umbraco/TestPackage/Editors/{0}/{1}", contentControllerId.ToString("N"), contentControllerName), contentEditorDefaultUrl);
            //Assert.AreEqual(string.Format("/Umbraco/TestPackage/Editors/{0}/{1}", mediaControllerId.ToString("N"), mediaControllerName), dataTypeEditorDefaulturl);
            //Assert.AreEqual(string.Format("/Umbraco/TestPackage/Editors/{0}/{1}/{2}/{3}", contentControllerId.ToString("N"), contentControllerName, customAction, id), contentEditorCustomUrl);
            //Assert.AreEqual(string.Format("/Umbraco/TestPackage/Editors/{0}/{1}/{2}/{3}", mediaControllerId.ToString("N"), mediaControllerName, customAction, id), dataTypeEditorCustomurl);

            Assert.AreEqual(string.Format("/Umbraco/TestPackage/{0}", contentControllerName), contentEditorDefaultUrl);
            Assert.AreEqual(string.Format("/Umbraco/TestPackage/{0}", mediaControllerName), dataTypeEditorDefaulturl);
            Assert.AreEqual(string.Format("/Umbraco/TestPackage/{0}/{1}/{2}", contentControllerName, customAction, id), contentEditorCustomUrl);
            Assert.AreEqual(string.Format("/Umbraco/TestPackage/{0}/{1}/{2}", mediaControllerName, customAction, id), dataTypeEditorCustomurl);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Ensure_Default_Tree_Url_Structures()
        {

            //Arrange

            var context = new FakeHttpContextFactory("~/empty", new RouteData());

            var contentTree = new ContentTreeController(new FakeBackOfficeRequestContext());
            var contentControllerName = UmbracoController.GetControllerName(contentTree.GetType());
            var contentControllerId = UmbracoController.GetControllerId<TreeAttribute>(contentTree.GetType());

            var dataTypeTree = new DataTypeTreeController(new FakeBackOfficeRequestContext());
            var dataTypeControllerName = UmbracoController.GetControllerName(dataTypeTree.GetType());
            var dataTypeControllerId = UmbracoController.GetControllerId<TreeAttribute>(dataTypeTree.GetType());

            const string action = "Index";
            var customId = HiveId.ConvertIntToGuid(123);
            const string area = "Umbraco";

            //Act

            //ensure the area is passed in because we're matchin a URL in an area, otherwise it will not work
            
            var contentTreeDefaultUrl = UrlHelper.GenerateUrl(null, action, contentControllerName,
                new RouteValueDictionary(new { area, id = HiveId.Empty, treeId = contentControllerId.ToString("N") }),
                RouteTable.Routes, context.RequestContext, true);
            
            var dataTypeTreeDefaultUrl = UrlHelper.GenerateUrl(null, action, dataTypeControllerName,
                    new RouteValueDictionary(new { area, id = HiveId.Empty, treeId = dataTypeControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            var contentTreeCustomUrl = UrlHelper.GenerateUrl(null, action, contentControllerName,
                    new RouteValueDictionary(new { area, id = customId, treeId = contentControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            var dataTypeTreeCustomUrl = UrlHelper.GenerateUrl(null, action, dataTypeControllerName,
                    new RouteValueDictionary(new { area, id = customId, treeId = dataTypeControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            //Assert

            Assert.AreEqual(string.Format("/Umbraco/Trees/{0}", contentControllerName), contentTreeDefaultUrl);
            Assert.AreEqual(string.Format("/Umbraco/Trees/{0}", dataTypeControllerName), dataTypeTreeDefaultUrl);
            Assert.AreEqual(string.Format("/Umbraco/Trees/{0}/{1}/{2}", contentControllerName, action, HttpUtility.UrlEncode(customId.ToString())), contentTreeCustomUrl);
            Assert.AreEqual(string.Format("/Umbraco/Trees/{0}/{1}/{2}", dataTypeControllerName, action, HttpUtility.UrlEncode(customId.ToString())), dataTypeTreeCustomUrl);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Routing)]
        public void BackOfficeRouting_Ensure_Plugin_Tree_Url_Structures()
        {

            //Arrange

            var context = new FakeHttpContextFactory("~/empty", new RouteData());

            var contentTree = new Stubs.Trees.ContentTreeController(new FakeBackOfficeRequestContext());
            var contentControllerName = UmbracoController.GetControllerName(contentTree.GetType());
            var contentControllerId = UmbracoController.GetControllerId<TreeAttribute>(contentTree.GetType());

            var mediaTree = new Stubs.Trees.MediaTreeController(new FakeBackOfficeRequestContext());
            var mediaTreeControllerName = UmbracoController.GetControllerName(mediaTree.GetType());
            var mediaTreeControllerId = UmbracoController.GetControllerId<TreeAttribute>(mediaTree.GetType());

            const string action = "Index";
            var defaultId = HiveId.Empty;
            const int customId = 123;
            const string area = "TestPackage";

            //Act

            //ensure the area is passed in because we're matchin a URL in an area, otherwise it will not work

            var contentTreeDefaultUrl = UrlHelper.GenerateUrl(null, action, contentControllerName,
                new RouteValueDictionary(new { area, id = defaultId, treeId = contentControllerId.ToString("N") }),
                RouteTable.Routes, context.RequestContext, true);

            var mediaTreeDefaultUrl = UrlHelper.GenerateUrl(null, action, mediaTreeControllerName,
                    new RouteValueDictionary(new { area, id = defaultId, treeId = mediaTreeControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            var contentTreeCustomUrl = UrlHelper.GenerateUrl(null, action, contentControllerName,
                    new RouteValueDictionary(new { area, id = customId, treeId = contentControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            var mediaTreeCustomUrl = UrlHelper.GenerateUrl(null, action, mediaTreeControllerName,
                    new RouteValueDictionary(new { area, id = customId, treeId = mediaTreeControllerId.ToString("N") }),
                    RouteTable.Routes, context.RequestContext, true);

            //Assert

            //Assert.AreEqual(string.Format("/Umbraco/TestPackage/Trees/{0}/{1}", contentControllerId.ToString("N"), contentControllerName), contentTreeDefaultUrl);
            //Assert.AreEqual(string.Format("/Umbraco/TestPackage/Trees/{0}/{1}", mediaTreeControllerId.ToString("N"), mediaTreeControllerName), mediaTreeDefaultUrl);
            //Assert.AreEqual(string.Format("/Umbraco/TestPackage/Trees/{0}/{1}/{2}/{3}", contentControllerId.ToString("N"), contentControllerName, action, customId), contentTreeCustomUrl);
            //Assert.AreEqual(string.Format("/Umbraco/TestPackage/Trees/{0}/{1}/{2}/{3}", mediaTreeControllerId.ToString("N"), mediaTreeControllerName, action, customId), mediaTreeCustomUrl);

            Assert.AreEqual(string.Format("/Umbraco/TestPackage/{0}", contentControllerName), contentTreeDefaultUrl);
            Assert.AreEqual(string.Format("/Umbraco/TestPackage/{0}", mediaTreeControllerName), mediaTreeDefaultUrl);
            Assert.AreEqual(string.Format("/Umbraco/TestPackage/{0}/{1}/{2}", contentControllerName, action, customId), contentTreeCustomUrl);
            Assert.AreEqual(string.Format("/Umbraco/TestPackage/{0}/{1}/{2}", mediaTreeControllerName, action, customId), mediaTreeCustomUrl);
        }

    }
}
