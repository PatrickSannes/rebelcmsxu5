using System;
using System.IO;
using System.Text;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Hive.PersistenceGovernor;
using Umbraco.Framework.Persistence.NHibernate;
using Umbraco.Framework.Persistence.NHibernate.DataManagement;
using Umbraco.Framework.Persistence.NHibernate.DependencyDemandBuilders;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Persistence.RdbmsModel.Mapping;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Framework.TypeMapping;
using Umbraco.Tests.Extensions.Stubs;
using HiveReadProvider = Umbraco.Framework.Persistence.NHibernate.HiveReadProvider;
using HiveReadWriteProvider = Umbraco.Framework.Persistence.NHibernate.HiveReadWriteProvider;

namespace Umbraco.Tests.Extensions
{
    /// <summary>
    /// A helper class to create an in memory sql lite persistence layer for use in unit tests
    /// </summary>
    public class NHibernateInMemoryRepository : DisposableObject
    {
        private static ISessionFactory _sessionFactory;
        
        private readonly DataContextFactory _dataContextFactory;
        
        private readonly HiveReadProvider _hiveReadProvider;        
        private readonly HiveReadWriteProvider _writeProvider;
        private readonly TextWriter _schemaWriter = new StringWriter(new StringBuilder());
        private readonly Framework.Hive.PersistenceGovernor.HiveReadProvider _hiveReadProviderViaGovernor;
        private readonly Framework.Hive.PersistenceGovernor.HiveReadWriteProvider _hiveReadWriteProviderViaGovernor;

        public ReadOnlyUnitOfWorkFactory ReadOnlyUnitOfWorkFactory { get; private set; }
        public ReadWriteUnitOfWorkFactory ReadWriteUnitOfWorkFactory { get; private set; }
        public ISession SessionForTest { get; private set; }
        public IHiveReadProvider HiveReadProvider { get { return _hiveReadProvider; } }
        public IHiveReadWriteProvider ReadWriteProvider { get { return _writeProvider; } }
        public IHiveReadProvider HiveReadProviderViaGovernor { get { return _hiveReadProviderViaGovernor; } }
        public IHiveReadWriteProvider HiveReadWriteProviderVieGovernor { get { return _hiveReadWriteProviderViaGovernor; } }

        public NHibernateInMemoryRepository(IFrameworkContext fakeFrameworkContext, ISessionFactory sessionFactory = null, ISession sessionForTest = null)
        {
            using (DisposableTimer.TraceDuration<NHibernateInMemoryRepository>("Start setup", "End setup"))
            {
                if (sessionFactory == null && sessionForTest == null)
                {
                    var builder = new NHibernateConfigBuilder("data source=:memory:", "unit-tester",
                                                              SupportedNHDrivers.SqlLite, "thread_static", false);
                    var config = builder.BuildConfiguration();
                    _sessionFactory = config.BuildSessionFactory();
                    SessionForTest = _sessionFactory.OpenSession();

                    // See http://stackoverflow.com/questions/4325800/testing-nhibernate-with-sqlite-no-such-table-schema-is-generated
                    // and also http://nhforge.org/doc/nh/en/index.html#architecture-current-session
                    // regarding contextual sessions and GetCurrentSession()

                    // We pass in our own TextWriter because a bug in VS's testing framework means directly passing in Console.Out causes an ObjectDisposedException
                    new SchemaExport(config).Execute(false, true, false, SessionForTest.Connection, _schemaWriter);
                }
                else
                {
                    _sessionFactory = sessionFactory;
                    SessionForTest = sessionForTest;
                }

                _dataContextFactory = new DataContextFactory(fakeFrameworkContext, SessionForTest, true);

                // Create reader
                ReadOnlyUnitOfWorkFactory = new ReadOnlyUnitOfWorkFactory();
                _hiveReadProvider = new HiveReadProvider(new HiveProviderSetup(fakeFrameworkContext, "r-unit-tester", new FakeHiveProviderBootstrapper(), ReadOnlyUnitOfWorkFactory, null, _dataContextFactory));

                // Create writer
                ReadWriteUnitOfWorkFactory = new ReadWriteUnitOfWorkFactory();
                _writeProvider = new HiveReadWriteProvider(new HiveProviderSetup(fakeFrameworkContext, "rw-unit-tester", new FakeHiveProviderBootstrapper(), ReadOnlyUnitOfWorkFactory, ReadWriteUnitOfWorkFactory, _dataContextFactory));

                //setup nhibernate mappers
                var manualMapper = new ManualMapper(new NhLookupHelper(_dataContextFactory), _writeProvider);
                fakeFrameworkContext.TypeMappers.Add(new Lazy<AbstractTypeMapper, TypeMapperMetadata>(() => manualMapper, new TypeMapperMetadata(true)));


                // Create hive wrappers for the readers and writers
                var governorRUowFactory = new ReadOnlyUnitOfWorkFactoryWrapper(new[] { _hiveReadProvider });

                var governorRWUowFactory = new ReadWriteUnitOfWorkFactoryWrapper(new[] { _writeProvider });

                _hiveReadProviderViaGovernor = new Framework.Hive.PersistenceGovernor.HiveReadProvider(new HiveProviderSetup(fakeFrameworkContext, "r-unit-wrapper", new FakeHiveProviderBootstrapper(), governorRUowFactory, null, null), new[] { _hiveReadProvider });
                _hiveReadWriteProviderViaGovernor = new Framework.Hive.PersistenceGovernor.HiveReadWriteProvider(new HiveProviderSetup(fakeFrameworkContext, "rw-unit-wrapper", new FakeHiveProviderBootstrapper(), governorRUowFactory, governorRWUowFactory, null), new[] { _writeProvider });
            }
        }

        public void ClearTest()
        {
            SessionForTest.Clear();
        }

        protected override void DisposeResources()
        {
            if (SessionForTest != null) SessionForTest.Dispose();
        }
    }
}