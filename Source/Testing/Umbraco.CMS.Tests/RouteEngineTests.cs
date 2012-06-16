using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Routing;
using Umbraco.Cms.Web.System;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;
using Umbraco.Framework.Persistence;
using Umbraco.Hive;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class RouteEngineTests : StandardWebTest
    {

        protected Umbraco.Cms.Web.Context.IBackOfficeRequestContext GetBackOfficeRequestContext(Uri uri)
        {
            var httpContext = new FakeHttpContextFactory(uri).HttpContext;
            return new FakeBackOfficeRequestContext(UmbracoApplicationContext, new DefaultRoutingEngine(UmbracoApplicationContext, httpContext));
        }

        protected GroupUnitFactory GetHive()
        {
            return UmbracoApplicationContext.Hive.GetWriter<IContentStore>();
        }

        protected IRoutingEngine GetEngine(Uri uri)
        {
            return GetBackOfficeRequestContext(uri).RoutingEngine;
        }

        protected TypedEntity CreateNewEntity()
        {
            var contentSchema = HiveModelCreationHelper.MockEntitySchema("test", "test");
            var nodeNameDef = new NodeNameAttributeDefinition(FixedGroupDefinitions.GeneralGroup);
            contentSchema.AttributeDefinitions.Add(nodeNameDef);

            contentSchema.RelationProxies.EnlistParentById(FixedHiveIds.ContentRootSchema, FixedRelationTypes.DefaultRelationType, 0);

            var entity = HiveModelCreationHelper.CreateTypedEntity(contentSchema,
                                                                           new[]
                                                                                   {
                                                                                       new TypedAttribute(nodeNameDef),
                                                                                   });
            entity.Id = new HiveId(Guid.NewGuid());

            return entity;

        }

        [Test]
        public void RouteEngine_FindEntityByUrl_WithHostname()
        {
            TypedEntity multiGrandChild;
            TypedEntity firstChild;
            TypedEntity firstGrandChild;
            TypedEntity secondChild;

            var domainRoot = SetupTestStructure(out multiGrandChild, out firstChild, out firstGrandChild, out secondChild);

            AssertFoundEntityByUrl(domainRoot, new Uri("http://hello.com/"));
            AssertFoundEntityByUrl(firstChild, new Uri("http://hello.com/first-child"));
            AssertFoundEntityByUrl(firstChild, new Uri("http://hello.com/first-child/"));
            AssertFoundEntityByUrl(firstGrandChild, new Uri("http://hello.com/first-child/first-grandchild"));
            AssertFoundEntityByUrl(firstGrandChild, new Uri("http://hello.com/first-child/first-grandchild/"));
            AssertFoundEntityByUrl(multiGrandChild, new Uri("http://hello.com/first-child/multi-grandchild"));
            AssertFoundEntityByUrl(multiGrandChild, new Uri("http://hello.com/first-child/multi-grandchild/"));
            AssertFoundEntityByUrl(multiGrandChild, new Uri("http://hello.com/second-child/multi-grandchild"));
            AssertFoundEntityByUrl(multiGrandChild, new Uri("http://hello.com/second-child/multi-grandchild/"));
            AssertFoundEntityByUrl(secondChild, new Uri("http://hello.com/second-child"));
            AssertFoundEntityByUrl(secondChild, new Uri("http://hello.com/second-child/"));
        }

        private TypedEntity SetupTestStructure(
            out TypedEntity multiGrandChild,
            out TypedEntity firstChild,
            out TypedEntity firstGrandChild,
            out TypedEntity secondChild)
        {
            var domainRoot = CreateNewEntity();
            firstChild = CreateNewEntity();
            secondChild = CreateNewEntity();
            firstGrandChild = CreateNewEntity();
            multiGrandChild = CreateNewEntity();
            using (var writer = GetHive().Create<IContentStore>())
            {
                domainRoot.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "homepage";
                firstChild.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "first-child";
                firstGrandChild.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "first-grandchild";
                multiGrandChild.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "multi-grandchild";
                secondChild.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "second-child";
                writer.Repositories.AddOrUpdate(domainRoot);
                writer.Repositories.AddOrUpdate(firstChild);
                writer.Repositories.AddOrUpdate(firstGrandChild);
                writer.Repositories.AddOrUpdate(multiGrandChild);
                writer.Repositories.AddOrUpdate(secondChild);
                writer.Repositories.AddRelation(
                    FixedHiveIds.ContentVirtualRoot, domainRoot.Id, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddRelation(domainRoot, firstChild, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddRelation(firstChild, firstGrandChild, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddRelation(firstChild, multiGrandChild, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddRelation(secondChild, multiGrandChild, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddRelation(domainRoot, secondChild, FixedRelationTypes.DefaultRelationType, 0);

                var hostname = new Hostname { Name = "hello.com" };
                writer.Repositories.AddOrUpdate(hostname);
                writer.Repositories.AddRelation(domainRoot.Id, hostname.Id, FixedRelationTypes.HostnameRelationType, 0);

                writer.Complete();
            }
            return domainRoot;
        }

        private void AssertFoundEntityByUrl(TypedEntity domainRoot, Uri fullUrlIncludingDomain)
        {
            var entityByUrl = GetEngine(new Uri("http://hello.com")).FindEntityByUrl(fullUrlIncludingDomain, null);
            Assert.NotNull(entityByUrl);
            Assert.That(entityByUrl.Status, Is.EqualTo(EntityRouteStatus.SuccessWithoutHostname));
            Assert.NotNull(entityByUrl.RoutableEntity);
            Assert.AreEqual(domainRoot.Id, entityByUrl.RoutableEntity.Id);
        }

        [Test]
        public void RouteEngine_Get_Domain_Url_With_Hostname_On_Other_Authority()
        {
            //NOTE: We know that the mock http context we've setup is using hello.com as it's Url Authority

            //creates an entity without a domain
            var nonDomain = CreateNewEntity();

            //these will have a domain attached
            var main = CreateNewEntity();
            var sub = CreateNewEntity();

            using (var writer = GetHive().Create<IContentStore>())
            {
                main.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "hello";
                sub.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "world";

                //2 root nodes, one with and one without a domain
                writer.Repositories.AddOrUpdate(nonDomain);
                writer.Repositories.AddRelation(FixedHiveIds.ContentVirtualRoot, nonDomain.Id, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddOrUpdate(main);
                writer.Repositories.AddRelation(FixedHiveIds.ContentVirtualRoot, main.Id, FixedRelationTypes.DefaultRelationType, 0);

                writer.Repositories.AddOrUpdate(sub);
                writer.Repositories.AddRelation(main.Id, sub.Id, FixedRelationTypes.DefaultRelationType, 0);

                var hostname = new Hostname { Name = "adifferentauthority.com" };
                writer.Repositories.AddOrUpdate(hostname);
                writer.Repositories.AddRelation(main.Id, hostname.Id, FixedRelationTypes.HostnameRelationType, 0);

                writer.Complete();
            }

            //Act

            var url1 = GetEngine(new Uri("http://hello.com/")).GetUrlForEntity(main);
            var url2 = GetEngine(new Uri("http://hello.com/")).GetUrlForEntity(sub);

            //Assert

            Assert.AreEqual(UrlResolutionStatus.SuccessWithHostname, url1.Status);
            Assert.AreEqual("adifferentauthority.com", url1.Url);
            Assert.AreEqual(UrlResolutionStatus.SuccessWithHostname, url2.Status);
            Assert.AreEqual("adifferentauthority.com/world", url2.Url);
        }

        [Test]
        public void RouteEngine_Get_Relative_Url_With_Hostname_On_Current_Authority()
        {
            //NOTE: We know that the mock http context we've setup is using hello.com as it's Url Authority

            //creates an entity without a domain
            var nonDomain = CreateNewEntity();

            using (var writer = GetHive().Create<IContentStore>())
            {               
                //Add a node under the root that doesn't have a domain
                writer.Repositories.AddOrUpdate(nonDomain);
                writer.Repositories.AddRelation(FixedHiveIds.ContentVirtualRoot, nonDomain.Id, FixedRelationTypes.DefaultRelationType, 0);
                writer.Complete();
            }
            
            //these will have a domain attached
            TypedEntity multiGrandChild;
            TypedEntity firstChild;
            TypedEntity firstGrandChild;
            TypedEntity secondChild;

            var domainRoot = SetupTestStructure(out multiGrandChild, out firstChild, out firstGrandChild, out secondChild);

            //Act

            var rootUrl = GetEngine(new Uri("http://hello.com/")).GetUrlForEntity(domainRoot);
            var firstChildUrl = GetEngine(new Uri("http://hello.com/")).GetUrlForEntity(firstChild);
            var firstGrandChildUrl = GetEngine(new Uri("http://hello.com/")).GetUrlForEntity(firstGrandChild);
            var secondChildUrl = GetEngine(new Uri("http://hello.com/")).GetUrlForEntity(secondChild);
            var multiGrandChildUrl = GetEngine(new Uri("http://hello.com/")).GetUrlForEntity(multiGrandChild);

            //Assert

            Assert.AreEqual(UrlResolutionStatus.SuccessWithoutHostname, rootUrl.Status);
            Assert.AreEqual("", rootUrl.Url);
            Assert.AreEqual(UrlResolutionStatus.SuccessWithoutHostname, firstChildUrl.Status);
            Assert.AreEqual("/first-child", firstChildUrl.Url);
            Assert.AreEqual(UrlResolutionStatus.SuccessWithoutHostname, firstGrandChildUrl.Status);
            Assert.AreEqual("/first-child/first-grandchild", firstGrandChildUrl.Url);
            Assert.AreEqual(UrlResolutionStatus.SuccessWithoutHostname, secondChildUrl.Status);
            Assert.AreEqual("/second-child", secondChildUrl.Url);
            Assert.AreEqual(UrlResolutionStatus.SuccessWithoutHostname, multiGrandChildUrl.Status);
            Assert.AreEqual("/first-child/multi-grandchild", multiGrandChildUrl.Url);
        }

        [Test]
        public void RouteEngine_Get_Url_WithoutHostname()
        {
            //Arrange

            var e = CreateNewEntity();
            var sub = CreateNewEntity();
            using (var writer = GetHive().Create<IContentStore>())
            {
                e.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "hello";
                sub.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "world";
                writer.Repositories.AddOrUpdate(e);
                writer.Repositories.AddRelation(FixedHiveIds.ContentVirtualRoot, e.Id, FixedRelationTypes.DefaultRelationType, 0);
                writer.Repositories.AddOrUpdate(sub);
                writer.Repositories.AddRelation(e.Id, sub.Id, FixedRelationTypes.DefaultRelationType, 0);
                writer.Complete();
            }

            //Act

            var url1 = GetEngine(new Uri("http://hello.com/")).GetUrlForEntity(e);
            var url2 = GetEngine(new Uri("http://hello.com/")).GetUrlForEntity(sub);

            //Assert

            Assert.AreEqual(UrlResolutionStatus.SuccessWithoutHostname, url1.Status);
            Assert.AreEqual("/", url1.Url);
            Assert.AreEqual(UrlResolutionStatus.SuccessWithoutHostname, url2.Status);
            Assert.AreEqual("/world", url2.Url);

        }

        [TestFixtureSetUp]
        public static void TestSetup()
        {
            DataHelper.SetupLog4NetForTests();
        }

        /// <summary>
        /// initialize all tests, this puts required data into Hive
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            Init();
            FrameworkContext.ApplicationCache.InvalidateItems(".*");
            AddRequiredDataToRepository();
        }

        /// <summary>
        /// Puts all of the data types into the repo from demo data and the inbuilt ones
        /// </summary>
        protected void AddRequiredDataToRepository()
        {
            // Create in-built attribute type definitions
            using (var writer = UmbracoApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                writer.Repositories.Schemas.AddOrUpdate(CoreCmsData.RequiredCoreSystemAttributeTypes());
                writer.Complete();
            }

            //create the core schemas
            using (var writer = UmbracoApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                writer.Repositories.Schemas.AddOrUpdate(CoreCmsData.RequiredCoreSchemas());
                writer.Complete();
            }

            //create the core root nodes
            using (var writer = UmbracoApplicationContext.Hive.OpenWriter<IContentStore>())
            {
                foreach (var e in CoreCmsData.RequiredCoreRootNodes())
                {
                    writer.Repositories.AddOrUpdate(e);                    
                }
                writer.Complete();
            }
        }
    }
}
