using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Foundation.Configuration;
using Umbraco.Framework.Context;
using Umbraco.Framework.Hive.PersistenceGovernor;
using Umbraco.Framework.Persistence.Configuration;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.NHibernate.Config;
using Umbraco.Framework.Persistence.NHibernate.DataManagement;
using Umbraco.Framework.Persistence.NHibernate.DataManagement.ReadWrite;
using Umbraco.Framework.Persistence.NHibernate.DependencyDemandBuilders;
using Umbraco.Framework.Persistence.ProviderSupport;
using Reader = Umbraco.Framework.Persistence.NHibernate.Reader;
using ReadWriter = Umbraco.Framework.Persistence.NHibernate.ReadWriter;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders.NHibernate
{


    /// <summary>
    ///This is a test class for MockedInMemoryProviderTest and is intended
    ///to contain all MockedInMemoryProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NHibernateProviderTest
    {
        private static DefaultPersistenceMappingGroup _mappingGroup;

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            var frameworkContext =
                new FrameworkContext(
                    ConfigurationManager.GetSection("umbraco.foundation") as IFoundationConfigurationSection);

            var persistenceConfig = ConfigurationManager.GetSection("hive.persistence") as HiveConfigurationSection;

            var localConfig = persistenceConfig.AvailableProviders.ReadWriters["rw-nhibernate-01"].GetLocalProviderConfig() as ProviderConfigurationSection;

            var nhSetup = new NHibernateConfigBuilder("rw-nhibernate-01", localConfig);
            var config = nhSetup.BuildConfiguration();

            // Setup the local provider
            var dataContextFactory = new DataContextFactory(config.BuildSessionFactory());
            var unitOfWorkFactory = new ReadWriteRepositoryUnitOfWorkFactory(dataContextFactory);
            var reader = new Reader(unitOfWorkFactory);
            var readWriter = new ReadWriter(unitOfWorkFactory);

            var uriMatch = new DefaultUriMatch() {MatchType = UriMatchElement.MatchTypes.Wildcard, Uri = "content://*/"};

            // Setup hive's provider governor. Normally it takes two uow factories (read and read-write) but we can use the same for both here
            _mappingGroup = new DefaultPersistenceMappingGroup("rw-nhibernate-01", new[] { unitOfWorkFactory }, new[] { unitOfWorkFactory }, new[] { reader }, new[] { readWriter }, new[]{uriMatch});
        }

        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion



        [TestMethod()]
        public void ReadersPopulatedTest()
        {
            Assert.IsNotNull(_mappingGroup.Readers);
        }

        [TestMethod()]
        public void ReadWritersPopulatedTest()
        {
            Assert.IsNotNull(_mappingGroup.ReadWriters);
        }

        [TestMethod]
        public void ReadWriterGetTypedPersistenceEntity()
        {
            var timer = new Stopwatch();
            timer.Start();
            long lastCheck = 0;

            using (var unit = _mappingGroup.CreateReadWriteUnitOfWork())
            {
                lastCheck = timer.ElapsedMilliseconds;
                Console.WriteLine("Created UoW in {0}ms", lastCheck);

                var entity1 = unit.ReadWriteRepository.GetTypedEntity(Guid.Parse("54700900-DA84-4A17-B681-B8A41C877F2E"));
                lastCheck = timer.ElapsedMilliseconds - lastCheck;
                Console.WriteLine("Got entity {0} in {1}ms", entity1.Id, lastCheck);

                var entity2 = unit.ReadWriteRepository.GetTypedEntity(Guid.Parse("B57F9271-BD63-4D8D-B97A-5FC3CF70D9B9"));
                lastCheck = timer.ElapsedMilliseconds - lastCheck;
                Console.WriteLine("Got entity {0} in {1}ms", entity2.Id, lastCheck);

                Assert.IsTrue(entity1.Id.AsGuid == Guid.Parse("54700900-DA84-4A17-B681-B8A41C877F2E"));
                Assert.IsTrue(entity2.Id.AsGuid == Guid.Parse("B57F9271-BD63-4D8D-B97A-5FC3CF70D9B9"));

            }
        }

        [TestMethod]
        public void ReadWriterGetPersistenceEntityTest()
        {
            PersistenceEntity persistedEntity = TestHelper.CreatePersistedEntity();

            using (var unit = _mappingGroup.CreateReadWriteUnitOfWork())
            {
                unit.ReadWriteRepository.Add(persistedEntity);
                unit.Commit();
            }

            Thread.Sleep(2000); // Pause to check if dates are persisted / reloaded

            using (var unit = _mappingGroup.CreateReadWriteUnitOfWork())
            {
                PersistenceEntity result = unit.ReadWriteRepository.GetEntity(persistedEntity.Id);
                Assert.IsTrue(persistedEntity.Id == result.Id);
            }
        }
    }
}
