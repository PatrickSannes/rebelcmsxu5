using System;
using NUnit.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Hive;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.Examine
{
    [TestFixture]
    public class ExamineStandardProviderTests : AbstractProviderTests
    {
        [SetUp]
        public void BeforeTest()
        {
            TestSetupHelperHelper = new ExamineTestSetupHelper();
            TestSetupHelperHelper.ExamineHelper.ClearCache(true, true);
        }

        [TearDown]
        public void AfterTest()
        {
            TestSetupHelperHelper.Dispose();
        }

        private ExamineTestSetupHelper TestSetupHelperHelper { get; set; }

        protected override Action PostWriteCallback
        {
            get
            {
                return () => TestSetupHelperHelper.ExamineHelper.ClearCache(true, true);
            }
        }

        protected override ProviderSetup ProviderSetup
        {
            get { return TestSetupHelperHelper.ProviderSetup; }
        }

        protected override ReadonlyProviderSetup ReadonlyProviderSetup
        {
            get { return TestSetupHelperHelper.ReadonlyProviderSetup; }
        }

        protected override void DisposeResources()
        {
            TestSetupHelperHelper.Dispose();
        }
    }
}