using System;
using AutoMapper;
using Umbraco.Foundation;
using Umbraco.Framework.Bootstrappers.Autofac;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Persistence.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Framework.Persistence.DtoModel;
using Umbraco.Framework.Persistence.DtoModel.AutoMapped;
using Umbraco.Framework.Persistence.DtoModel.AutoMapped.Versioning;
using PersistenceEntityStatus = Umbraco.Framework.Persistence.DtoModel.AutoMapped.PersistenceEntityStatus;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders.NHibernate
{


    /// <summary>
    ///This is a test class for PocoMapperTest and is intended
    ///to contain all PocoMapperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PocoMapperTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            AutoFacResolver.InitialiseFoundation();

            DtoMappingHelper.RegisterAllFromAssemblyOf<PersistenceEntityDto>();
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

        [TestMethod]
        public void CanResolveTypeMapperViaIoc()
        {
            var mapper = DependencyResolver.Current.Resolve<ITypeMapper>();
            Assert.IsNotNull(mapper);
        }

        [TestMethod()]
        public void PersistenceEntityDtoViaPocoMapperTest()
        {
            var mapper = DependencyResolver.Current.Resolve<ITypeMapper>();

            PersistenceEntityDto entityDto = new PersistenceEntityDto();
            entityDto.ConcurrencyToken = new TickConcurrencyToken();
            entityDto.Id = Guid.NewGuid();
            entityDto.Revision = new RevisionData();
            entityDto.Revision.Changeset = new Changeset();
            entityDto.Revision.Changeset.Id = Guid.NewGuid();
            entityDto.Revision.Changeset.Branch = new Branch();
            entityDto.Revision.RevisionId = Guid.NewGuid();
            entityDto.UtcCreated = entityDto.UtcModified = entityDto.UtcStatusChanged = DateTime.UtcNow;
            entityDto.Status = new PersistenceEntityStatus();
            

            //Mapper.CreateMap<IPersistenceEntityStatus, PersistenceEntityStatus>();
            //Mapper.CreateMap<RevisionData, Persistence.Model.Versioning.RevisionData>();

            //Mapper.CreateMap<PersistenceEntityDto, Persistence.Model.PersistenceEntityDto>()
            //    //.ForMember(dest => dest.Id, opt => opt.ResolveUsing(new DtoGuidToEntityUriResolver()))
            //    .ForMember(dest => dest.Revision, opt => opt.ResolveUsing(new ProxyResolver<PersistenceEntityDto>(a=>a.Revision)));

            Mapper.AssertConfigurationIsValid();

            //Mapper.CreateMap<IRevisionData, Persistence.Model.Versioning.RevisionData>();

            var mappedEntity = mapper.Map<PersistenceEntityDto, IPersistenceEntity>(entityDto);

            //var mappedEntity2 = Mapper.Map<IPersistenceEntity, Persistence.Model.PersistenceEntityDto>(mappedEntity);

            var hydration = new PersistenceEntity();
            //entityDto.Hydrate(hydration);
            mapper.Map(entityDto, hydration);

            Assert.IsNotNull(mappedEntity);
            Assert.IsInstanceOfType(mappedEntity, typeof(IPersistenceEntity));
            Assert.AreEqual(entityDto.Id, hydration.Id.AsGuid);
            Assert.AreEqual(entityDto.Id, mappedEntity.Id.AsGuid);
        }

        //[TestMethod]
        //public void TestAttributeBasedMapRegistration()
        //{
        //    DtoMappingBuddyHelper.RegisterAllFromAssemblyOf<TestStatusDto>();

        //    TestStatusDto dto = new TestStatusDto();
        //}
    }
}
