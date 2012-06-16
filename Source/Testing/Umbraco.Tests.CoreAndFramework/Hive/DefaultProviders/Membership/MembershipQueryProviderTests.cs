using System;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.Membership
{
    using Umbraco.Tests.Extensions;

    [TestFixture]
    public class MembershipQueryProviderTests : AbstractProviderQueryTests
    {
        [SetUp]
        public void BeforeTest()
        {
            _setup = new MembershipWrapperTestSetupHelper();
            _groupUnitFactory = new GroupUnitFactory(_setup.ProviderSetup, new Uri("content://"), FakeHiveCmsManager.CreateFakeRepositoryContext(_setup.FrameworkContext));
        }

        private MembershipWrapperTestSetupHelper _setup;
        private GroupUnitFactory _groupUnitFactory;
        
        protected override TypedEntity CreateEntityForTest(Guid newGuid, Guid newGuidRedHerring, ProviderSetup providerSetup)
        {
            var entity = new Member()
                {
                    Id = new HiveId(newGuid),
                    Email = Guid.NewGuid().ToString("N") + "test@test.com",
                    IsApproved = true,
                    Name = "Test",
                    Password = "hello",
                    Username = "test" + Guid.NewGuid().ToString("N") //must be unique
                };

            entity.Username = "my-new-value";
            entity.Name = "not-on-red-herring";
            entity.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "my-test-route";

            //var redHerringEntity = HiveModelCreationHelper.MockTypedEntity();
            //redHerringEntity.Id = new HiveId(newGuidRedHerring);
            //redHerringEntity.EntitySchema.Alias = "redherring-schema";

            using (var uow = providerSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(entity);
                //uow.EntityRepository.AddOrUpdate(redHerringEntity);
                uow.Complete();
            }

            return entity;
        }

        protected override ProviderSetup ProviderSetup
        {
            get { return _setup.ProviderSetup; }
        }

        protected override GroupUnitFactory GroupUnitFactory
        {
            get { return _groupUnitFactory; }
        }
    }
}