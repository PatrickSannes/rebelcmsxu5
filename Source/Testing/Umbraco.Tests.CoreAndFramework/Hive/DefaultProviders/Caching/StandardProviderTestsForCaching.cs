using System;
using System.Runtime.Caching;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.Caching
{
    using Umbraco.Hive.InMemoryProvider;

    [TestFixture]
    [Ignore("This provider is not finished")]
    public class StandardProviderTestsForCaching : AbstractProviderTests
    {
        [SetUp]
        public void BeforeTest()
        {
            SetupHelper = new CacheTestSetupHelper();
        }

        [TearDown]
        public void AfterTest()
        {
            SetupHelper.Dispose();
        }

        private CacheTestSetupHelper SetupHelper { get; set; }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            return;
        }

        protected override Action PostWriteCallback { get { return () => SetupHelper.PostWrite(); } }

        protected override ProviderSetup ProviderSetup { get { return SetupHelper.ProviderSetup; } }

        protected override ReadonlyProviderSetup ReadonlyProviderSetup { get { return SetupHelper.ReadonlyProviderSetup; } }
    }

    class CacheTestSetupHelper : DisposableObject
    {
        public CacheTestSetupHelper()
        {
            _cacheReference = new MemoryCache("unit-tester");

            var metadata = new ProviderMetadata("cache", new Uri("cache://"), false, true);
            var frameworkContext = new FakeFrameworkContext();

            var schemaRepositoryFactory = new SchemaRepositoryFactory(
                metadata,
                new NullProviderRevisionRepositoryFactory<EntitySchema>(metadata, frameworkContext),
                frameworkContext,
                new DependencyHelper(metadata, new CacheHelper(CacheReference)));

            var revisionRepositoryFactory = new EntityRevisionRepositoryFactory(
                metadata,
                frameworkContext,
                new DependencyHelper(metadata, new CacheHelper(CacheReference)));

            var entityRepositoryFactory = new EntityRepositoryFactory(
                metadata,
                revisionRepositoryFactory,
                schemaRepositoryFactory,
                frameworkContext,
                new DependencyHelper(metadata, new CacheHelper(CacheReference)));
            var unitFactory = new ProviderUnitFactory(entityRepositoryFactory);
            var readonlyUnitFactory = new ReadonlyProviderUnitFactory(entityRepositoryFactory);
            ProviderSetup = new ProviderSetup(unitFactory, metadata, frameworkContext, new NoopProviderBootstrapper(), 0);
            ReadonlyProviderSetup = new ReadonlyProviderSetup(readonlyUnitFactory, metadata, frameworkContext, new NoopProviderBootstrapper(), 0);
        }

        private MemoryCache _cacheReference;

        public void PostWrite()
        {
            // We don't clear the cache here otherwise that would kind of defeat the point of caching...
            return;
        }

        public ProviderSetup ProviderSetup { get; protected set; }
        public ReadonlyProviderSetup ReadonlyProviderSetup { get; protected set; }
        public MemoryCache CacheReference { get { return _cacheReference; } }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _cacheReference.Dispose();
        }
    }
}
