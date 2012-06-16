using System;
using NUnit.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Hive;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.NHibernate
{
    [TestFixture]
    public class NhStandardProviderTests : AbstractProviderTests
    {
        [SetUp]
        public void BeforeTest()
        {
            SetupHelper = new NhibernateTestSetupHelper();            
        }

        [TearDown]
        public void AfterTest()
        {
            SetupHelper.Dispose();
        }

        private NhibernateTestSetupHelper SetupHelper { get; set; }

        protected override Action PostWriteCallback
        {
            get
            {
                return () =>
                           {
                               LogHelper.TraceIfEnabled<NhStandardProviderTests>("Clearing SessionForTest ({0} entities)", () => SetupHelper.SessionForTest.Statistics.EntityCount);
                               SetupHelper.SessionForTest.Clear();
                               LogHelper.TraceIfEnabled<NhStandardProviderTests>("Cleared SessionForTest ({0} entities)", () => SetupHelper.SessionForTest.Statistics.EntityCount);
                           };
            }
        }

        protected override ProviderSetup ProviderSetup
        {
            get { return SetupHelper.ProviderSetup; }
        }

        protected override ReadonlyProviderSetup ReadonlyProviderSetup
        {
            get { return SetupHelper.ReadonlyProviderSetup; }
        }

        protected override void DisposeResources()
        {
            if (SetupHelper != null)
                SetupHelper.Dispose();
        }
    }
}