using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Foundation;
using Umbraco.Foundation.Configuration;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Hive.PersistenceGovernor;
using Umbraco.Framework.Persistence.Configuration;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Persistence.XmlStore.Config;
using Umbraco.Framework.Persistence.XmlStore.DataManagement;
using Umbraco.Framework.Persistence.XmlStore.DataManagement.ReadWrite;
using Umbraco.TestExtensions;
using Umbraco.Tests.DomainDesign.Helpers;
using Reader = Umbraco.Framework.Persistence.XmlStore.Reader;
using ReadWriter = Umbraco.Framework.Persistence.XmlStore.ReadWriter;
using EntityRepositoryReader = Umbraco.Framework.Persistence.XmlStore.EntityRepositoryReader;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders.XmlStore
{
    [TestClass()]
    public class XmlStore
    {
        private static DefaultPersistenceMappingGroup _mappingGroup;
        private static Reader _xmlReader;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            var persistenceConfig = ConfigurationManager.GetSection(HiveConfigurationSection.ConfigXmlKey) as HiveConfigurationSection;

            var localConfig = persistenceConfig.AvailableProviders.ReadWriters["rw-xmlstore-01"].GetLocalProviderConfig() as ProviderConfigurationSection;

            Assert.IsNotNull(localConfig);

            var dataPath = TestHelper.MapPathForTest(localConfig.Path);

            // Setup the local provider
            var dataContextFactory = new DataContextFactory(dataPath);
            var unitOfWorkFactory = new ReadWriteRepositoryUnitOfWorkFactory(dataContextFactory);
            _xmlReader = new Reader(unitOfWorkFactory);
            var readWriter = new ReadWriter(unitOfWorkFactory);

            var uriMatch = new DefaultUriMatch() { MatchType = UriMatchElement.MatchTypes.Wildcard, Uri = "content://*/" };

            // Setup hive's provider governor. Normally it takes two uow factories (read and read-write) but we can use the same for both here
            _mappingGroup = new DefaultPersistenceMappingGroup("rw-demodata-01", new[] { unitOfWorkFactory }, new[] { unitOfWorkFactory }, new[] { _xmlReader }, new[] { readWriter }, new[] { uriMatch });
        }

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
            using (var uow = _mappingGroup.CreateReadOnlyUnitOfWork())
            {
                Assert.IsNotNull(uow.ReadRepository.GetEntityByRoute("/runway-homepage/installing-runway-modules/"));
            }
        }

        [TestMethod]
        public void ReadWriterQueryById()
        {
            using (DisposableTimer.Start(x => Console.WriteLine("ReadWriterQueryById took {0}ms", x)))
            {
                // Test provider directly rather than going via Hive
                using (var uow = _xmlReader.CreateReadOnlyUnitOfWork())
                {
                    using (DisposableTimer.Start(x => Console.WriteLine("Query and resolution took {0}ms", x)))
                    {
                        var item =
                            ((EntityRepositoryReader) uow.ReadRepository).QueryContext.Query().Where(
                                x => x.Id == new HiveEntityUri(1048)).FirstOrDefault();
                        Assert.IsNotNull(item);
                        Assert.AreEqual(item.Id.AsInt, 1048);
                    }
                }
                using (var uow = _mappingGroup.CreateReadOnlyUnitOfWork())
                {
                    using (DisposableTimer.Start(x => Console.WriteLine("Query and resolution via Hive took {0}ms", x)))
                    {
                        var item =
                            uow.ReadRepository.QueryContext.Query().Where(
                                x => x.Id == new HiveEntityUri(1048)).FirstOrDefault();
                        Assert.IsNotNull(item);
                        Assert.AreEqual(item.Id.AsInt, 1048);
                    }
                }
            }
        }
    }
}
