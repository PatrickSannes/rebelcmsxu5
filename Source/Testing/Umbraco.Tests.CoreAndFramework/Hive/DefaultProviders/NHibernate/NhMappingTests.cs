using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.NHibernate;
using Umbraco.Framework.Persistence.RdbmsModel;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.NHibernate
{
    [TestFixture]
    public class NhMappingTests
    {
        private NhSessionHelper _nhSessionHelper;
        private RdbmsModelMapper _rdbmsTypeMapper;

        /// <summary>
        /// Run once before each test in derived test fixtures.
        /// </summary>
        [SetUp]
        public void TestSetup()
        {
            var testSetup = new NhibernateTestSetupHelper();
            _nhSessionHelper = new NhSessionHelper(testSetup.SessionForTest, testSetup.FakeFrameworkContext);
            _rdbmsTypeMapper = new RdbmsModelMapper(_nhSessionHelper, testSetup.FakeFrameworkContext);
            _rdbmsTypeMapper.ConfigureMappings();
        }

        /// <summary>
        /// Run once after each test in derived test fixtures.
        /// </summary>
        [TearDown]
        public void TestTearDown()
        {
            _nhSessionHelper.NhSession.Clear();
        }

        #region Rdbms to model

        [Test]
        public void FromRdbms_StatusType_ToModel()
        {
            // Arrange
            var rdbms = new NodeVersionStatusType()
            {
                Alias = "myalias",
                Id = Guid.NewGuid(),
                IsSystem = true,
                Name = "myname"
            };

            // Act
            var model = _rdbmsTypeMapper.Map<RevisionStatusType>(rdbms);

            // Assert
            Assert_CompareStatusType(rdbms, model);
        }

        [Test]
        public void FromRdbms_Locale_ToModel()
        {
            // Arrange
            var rdbms = new Locale()
            {
                Alias = "en-gb",
                Id = Guid.NewGuid(),
                Name = "myname",
                LanguageIso = "en-gb"
            };

            // Act
            var model = _rdbmsTypeMapper.Map<LanguageInfo>(rdbms);

            // Assert
            Assert_CompareLocale(rdbms, model);
        }

        [Test]
        public void FromRdbms_AttributeType_ToModel()
        {
            // Arrange
            var rdbms = new AttributeType()
                {
                    Alias = "myalias",
                    Description = "mydescription",
                    Id = Guid.NewGuid(),
                    Name = "myname",
                    PersistenceTypeProvider = typeof(StringSerializationType).AssemblyQualifiedName,
                    RenderTypeProvider = typeof(int).AssemblyQualifiedName,
                    XmlConfiguration = "myxml"
                };

            // Act
            var model = _rdbmsTypeMapper.Map<Framework.Persistence.Model.Attribution.MetaData.AttributeType>(rdbms);

            // Assert
            Assert_CompareAttributeType(rdbms, model);
        }
        #endregion
        #region Comparisons

        private static void Assert_CompareAttributeType(AttributeType rdbms, Framework.Persistence.Model.Attribution.MetaData.AttributeType model)
        {
            Assert.That(model.Alias, Is.EqualTo(rdbms.Alias));
            Assert.That((string) model.Description, Is.EqualTo(rdbms.Description));
            Assert.That(model.RenderTypeProvider, Is.EqualTo(rdbms.RenderTypeProvider));
            Assert.That(model.RenderTypeProviderConfig, Is.EqualTo(rdbms.XmlConfiguration));
            Assert.That(model.SerializationType.GetType().AssemblyQualifiedName, Is.EqualTo(rdbms.PersistenceTypeProvider));
            Assert.That((string) model.Name, Is.EqualTo(rdbms.Name));
            Assert.That((Guid) model.Id.Value, Is.EqualTo(rdbms.Id));
        }

        private static void Assert_CompareLocale(Locale rdbms, LanguageInfo model)
        {
            Assert.That(model.Alias, Is.EqualTo(rdbms.Alias));
            Assert.That(model.Key, Is.EqualTo(rdbms.LanguageIso));
            Assert.That((string)model.Name, Is.EqualTo(rdbms.Name));
            Assert.That(model.Culture, Is.EqualTo(LanguageInfo.GetCultureFromKey("en-gb")));
        }

        private static void Assert_CompareStatusType(NodeVersionStatusType rdbms, RevisionStatusType model)
        {
            Assert.That(model.Alias, Is.EqualTo(rdbms.Alias));
            Assert.That(model.IsSystem, Is.EqualTo(rdbms.IsSystem));
            Assert.That((string)model.Name, Is.EqualTo(rdbms.Name));
            Assert.That((Guid)model.Id.Value, Is.EqualTo(rdbms.Id));
        }

        #endregion

        #region Model to Rdbms

        [Test]
        public void FromModel_AttributeType_ToRdbms()
        {
            // Arrange
            var model = new Framework.Persistence.Model.Attribution.MetaData.AttributeType()
                {
                    Alias = "myalias",
                    Description = "mydescription",
                    Id = new HiveId(Guid.NewGuid()),
                    Name = "myname",
                    Ordinal = 0, // TODO: Remove ordinal
                    RenderTypeProvider = typeof (int).AssemblyQualifiedName,
                    RenderTypeProviderConfig = "myxml",
                    SerializationType = new StringSerializationType()
                };

            // Act
            var rdbms = _rdbmsTypeMapper.Map<AttributeType>(model);

            // Assert
            Assert_CompareAttributeType(rdbms, model);
        }



        [Test]
        public void FromModel_AttributeType_ToRdbms_WhereAlreadyExists()
        {
            using (var tran = _nhSessionHelper.NhSession.BeginTransaction())
            {
                // Arrange
                var rdbmsExists = new AttributeType()
                    {
                        Alias = "my-original-alias",
                        Name = "myname"
                    };
                _nhSessionHelper.NhSession.SaveOrUpdate(rdbmsExists);
                var generatedId = rdbmsExists.Id;
                tran.Commit();

                var model = new Framework.Persistence.Model.Attribution.MetaData.AttributeType()
                    {
                        Alias = "my-new-alias",
                        Description = "mydescription",
                        Id = new HiveId(generatedId),
                        // Need to save over the top
                        Name = "myname",
                        Ordinal = 0,
                        // TODO: Remove ordinal
                        RenderTypeProvider = typeof (int).AssemblyQualifiedName,
                        RenderTypeProviderConfig = "myxml",
                        SerializationType = new StringSerializationType()
                    };

                // Act
                var rdbms = _rdbmsTypeMapper.Map<AttributeType>(model);

                // Assert
                Assert_CompareAttributeType(rdbms, model);
                Assert.That(_nhSessionHelper.NhSession.Query<AttributeType>().Count(), Is.EqualTo(1));
            }
        }

        [Test]
        public void FromModel_StatusType_ToRdbms()
        {
            // Arrange
            var model = new RevisionStatusType("myalias", "myname")
                {
                    Id = new HiveId(Guid.NewGuid()),
                    IsSystem = true
                };

            // Act
            var rdbms = _rdbmsTypeMapper.Map<NodeVersionStatusType>(model);

            // Assert
            Assert_CompareStatusType(rdbms, model);
        }

        [Test]
        public void FromModel_LanguageInfo_ToRdbms()
        {
            // Arrange
            var model = new LanguageInfo()
                {
                    Key = "en-gb",
                    Name = "myname"
                };

            // Act
            var rdbms = _rdbmsTypeMapper.Map<Locale>(model);

            // Assert
            Assert_CompareLocale(rdbms, model);
        }

        #endregion
    }
}
