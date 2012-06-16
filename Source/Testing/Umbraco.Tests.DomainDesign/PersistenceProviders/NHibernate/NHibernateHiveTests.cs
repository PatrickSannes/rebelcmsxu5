using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Framework.Context;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders.NHibernate
{
    /// <summary>
    /// Summary description for NHibernate
    /// </summary>
    [TestClass]
    public class NHibernateHiveTests : AbstractHivePersistenceTest
    {
        [ClassInitialize]
        public static void TestSetup(TestContext testContext)
        {
            DataHelper.SetupLog4NetForTests();
        }

        private readonly NHibernateInMemoryRepository _repository;
        private readonly FakeFrameworkContext _fakeFrameworkContext;

       protected override Action PostWriteCallback
        {
            get
            {
                return () =>
                {
                    LogHelper.TraceIfEnabled<NHibernateHiveTests>("Clearing SessionForTest ({0} entities)", () => _repository.SessionForTest.Statistics.EntityCount);
                    _repository.SessionForTest.Clear();
                    LogHelper.TraceIfEnabled<NHibernateHiveTests>("Cleared SessionForTest ({0} entities)", () => _repository.SessionForTest.Statistics.EntityCount);
                };
            }
        }

        protected override IFrameworkContext FrameworkContext { get { return _fakeFrameworkContext; } }

        protected override IHiveReadProvider DirectReaderProvider { get { return _repository.HiveReadProvider; } }

        protected override IHiveReadWriteProvider DirectReadWriteProvider { get { return _repository.ReadWriteProvider; } }

        protected override IHiveReadProvider ReaderProviderViaHiveGovernor
        {
            get { return _repository.HiveReadProviderViaGovernor; }
        }

        protected override IHiveReadWriteProvider ReadWriteProviderViaHiveGovernor
        {
            get { return _repository.HiveReadWriteProviderVieGovernor; }
        }


        public NHibernateHiveTests()
        {
            _fakeFrameworkContext = new FakeFrameworkContext();
            _repository = new NHibernateInMemoryRepository(_fakeFrameworkContext);
        }

        protected override void DisposeResources()
        {
            _repository.Dispose();
        }
    }
}
