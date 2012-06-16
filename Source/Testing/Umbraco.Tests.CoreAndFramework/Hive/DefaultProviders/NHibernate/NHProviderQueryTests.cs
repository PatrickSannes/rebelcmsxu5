using System;
using NUnit.Framework;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.Extensions;
using Umbraco.Hive;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.NHibernate
{
    [TestFixture]
    public class NhProviderQueryTests : AbstractProviderQueryTests
    {
        private NhibernateTestSetupHelper _helper;
        private GroupUnitFactory _groupUnitFactory;


        

        [SetUp]
        protected void TestSetup()
        {
            _helper = new NhibernateTestSetupHelper();
            _groupUnitFactory = new GroupUnitFactory(_helper.ProviderSetup, new Uri("content://"), FakeHiveCmsManager.CreateFakeRepositoryContext(_helper.FakeFrameworkContext));
        }

        [TearDown]
        protected void TestTearDown()
        {
            _helper.Dispose();
        }

        protected override ProviderSetup ProviderSetup
        {
            get { return _helper.ProviderSetup; }
        }

        protected override GroupUnitFactory GroupUnitFactory
        {
            get { return _groupUnitFactory; }
        }
    }
}
