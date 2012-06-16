using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Hive.CachingProvider;
using Umbraco.Framework.Hive.PersistenceGovernor;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;
using HiveReadWriteProvider = Umbraco.Framework.Hive.CachingProvider.HiveReadWriteProvider;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders.CachingProvider
{
    [TestClass]
    public class CachingProviderTests : AbstractHivePersistenceTest
    {
        [ClassInitialize]
        public static void TestSetup(TestContext testContext)
        {
            DataHelper.SetupLog4NetForTests();
        }

        #region Overrides of DisposableObject

        private readonly IHiveReadWriteProvider _directReadWriteProvider;
        private readonly IHiveReadProvider _directReaderProvider;

        private readonly IHiveReadWriteProvider _readWriteProviderViaHiveGovernor;
        private readonly IHiveReadProvider _readerProviderViaHiveGovernor;
        private FakeFrameworkContext _fakeFrameworkContext;

        protected override IFrameworkContext FrameworkContext { get { return _fakeFrameworkContext; } }


        public CachingProviderTests()
        {
            _fakeFrameworkContext = new FakeFrameworkContext();

            var inMemory = new NHibernateInMemoryRepository(_fakeFrameworkContext);

            var dataContextFactory = new DataContextFactory();
            var readWriteUnitOfWorkFactory = new ReadWriteUnitOfWorkFactory();
            var directReaderProvider = new HiveReadWriteProvider(new HiveProviderSetup(_fakeFrameworkContext, "r-unit-tester",
                                                                  new FakeHiveProviderBootstrapper(),
                                                                  readWriteUnitOfWorkFactory, readWriteUnitOfWorkFactory,
                                                                  dataContextFactory));
            var directReadWriteProvider = directReaderProvider;

            // Create hive wrappers for the readers and writers
            var governorRUowFactory = new ReadOnlyUnitOfWorkFactoryWrapper(new[] { directReaderProvider, inMemory.HiveReadProvider });
            var governorRWUowFactory = new ReadWriteUnitOfWorkFactoryWrapper(new[] { directReadWriteProvider, inMemory.ReadWriteProvider });

            _readerProviderViaHiveGovernor = _directReaderProvider =
                new Framework.Hive.PersistenceGovernor.HiveReadProvider(new HiveProviderSetup(_fakeFrameworkContext, "r-unit-wrapper", new FakeHiveProviderBootstrapper(), governorRUowFactory, null, null), new[] { _directReaderProvider });
            _readWriteProviderViaHiveGovernor = _directReadWriteProvider =
                new Framework.Hive.PersistenceGovernor.HiveReadWriteProvider(new HiveProviderSetup(_fakeFrameworkContext, "rw-unit-wrapper", new FakeHiveProviderBootstrapper(), governorRUowFactory, governorRWUowFactory, null), new[] { _directReadWriteProvider });
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _directReaderProvider.Dispose();
            _directReadWriteProvider.Dispose();
        }

        #endregion

        #region Overrides of AbstractPersistenceTest

        protected override Action PostWriteCallback
        {
            get { return () => { return; }; }
        }

        protected override IHiveReadProvider DirectReaderProvider
        {
            get { return _directReaderProvider; }
        }

        protected override IHiveReadWriteProvider DirectReadWriteProvider
        {
            get { return _directReadWriteProvider; }
        }

        protected override IHiveReadProvider ReaderProviderViaHiveGovernor
        {
            get { return _readerProviderViaHiveGovernor; }
        }

        protected override IHiveReadWriteProvider ReadWriteProviderViaHiveGovernor
        {
            get { return _readWriteProviderViaHiveGovernor; }
        }

        #endregion
    }
}