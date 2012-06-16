using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.Examine
{
    [TestFixture]
    public class ExamineProviderQueryTests : AbstractProviderQueryTests
    {
        private ExamineTestSetupHelper _helper;
        private GroupUnitFactory _groupUnitFactory;


        [SetUp]
        protected void TestSetup()
        {
            _helper = new ExamineTestSetupHelper();
            _groupUnitFactory = new GroupUnitFactory(_helper.ProviderSetup, new Uri("content://"), FakeHiveCmsManager.CreateFakeRepositoryContext(_helper.FrameworkContext));
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
