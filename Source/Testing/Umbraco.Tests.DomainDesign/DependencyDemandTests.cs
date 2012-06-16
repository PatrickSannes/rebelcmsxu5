using System;
using System.Linq;
using System.Linq.Expressions;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using Umbraco.Foundation.Extensions;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.DependencyManagement.Autofac;
using Umbraco.Framework.DependencyManagement.Ninject;
using Umbraco.Framework.Hive;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.DependencyManagement.DemandBuilders;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Tests.DomainDesign
{
	[TestClass]
	public class DependencyDemandTests
	{
		private static TestContext _testContext;
		private AutofacContainerBuilder _autofacContainer;
		private NinjectContainerBuilder _ninjectContainer;
		private ContainerBuilder _realAutofacBuilder;
		private StandardKernel _realNinjectKernel;

		[ClassInitialize]
		public static void MyClassInitialize(TestContext testContext)
		{
			_testContext = testContext;
		}

		[TestInitialize]
		public void TestInitialize()
		{
			_realAutofacBuilder = new ContainerBuilder();
			_realNinjectKernel = new StandardKernel();
			_autofacContainer = new AutofacContainerBuilder(_realAutofacBuilder);
			_ninjectContainer = new NinjectContainerBuilder(_realNinjectKernel);

			// Temp - fake registration of IUnitOfWork etc.
        // TODO: Re-enable once implementation has been rewritten
      //_realAutofacBuilder.RegisterType<AutoMapperTypeMapper>().As<ITypeMapper>().SingleInstance();
      //_realNinjectKernel.Bind<ITypeMapper>().To<AutoMapperTypeMapper>().InSingletonScope();
		}

		[TestMethod]
		public void AutofacCanLoadConfiguration()
		{
			//_autofacContainer.AddDependencyDemandBuilder(new LoadFromConfiguration());
            _autofacContainer.AddDemandsFromAssemblyOf<LoadFromPersistenceConfig>();

			IDependencyResolver resolver = _autofacContainer.Build();

            CheckResolvedProvider(resolver.Resolve<IPersistenceMappingGroup>("nhibernate-01"),
			                      typeof (Framework.Persistence.NHibernate.Reader),
			                      typeof (Framework.Persistence.NHibernate.ReadWriter));
		}

        [TestMethod]
        public void AutofacCanLoadConfigurationv2()
        {
            //_autofacContainer.AddDependencyDemandBuilder(new LoadFromConfiguration());
            _autofacContainer.AddDemandsFromAssemblyOf<LoadFromPersistenceConfig>();

            IDependencyResolver resolver = _autofacContainer.Build();

            CheckResolvedReader(resolver.Resolve<IPersistenceReadWriter>("r-nhibernate-01"),
                                typeof (Framework.Persistence.NHibernate.ReadWriter));
        }

        private static void CheckResolvedProvider(IPersistenceMappingGroup manager, Type readerType, Type readWriterType)
		{
			Assert.IsNotNull(manager, "Provider not resolved");
            Assert.IsFalse(string.IsNullOrWhiteSpace(manager.Key), "Alias is blank");

            manager.Readers.ForEach(x => CheckResolvedReader(x, readerType));
            manager.ReadWriters.ForEach(x => CheckResolvedWriter(x, readWriterType));
		}

        private static void CheckResolvedWriter(IPersistenceReadWriter manager, Type readWriterType)
	    {
            Assert.IsNotNull(manager, "ReadWriter not injected");
            Assert.IsFalse(string.IsNullOrWhiteSpace(manager.RepositoryKey), "Writer key is blank");
            Assert.IsInstanceOfType(manager, readWriterType, "Wrong ReadWriter type resolved");
	    }

        private static void CheckResolvedReader(IPersistenceReader reader, Type readerType)
	    {
	        Assert.IsNotNull(reader, "Reader not injected");
            Assert.IsFalse(string.IsNullOrWhiteSpace(reader.RepositoryKey), "Reader key is blank");
            Assert.IsInstanceOfType(reader, readerType, "Wrong Reader type resolved");
	    }

	    [TestMethod]
		public void NinjectCanLoadConfiguration()
		{
			_ninjectContainer.AddDependencyDemandBuilder(new LoadFromPersistenceConfig());

			IDependencyResolver resolver = _ninjectContainer.Build();

            var instance = resolver.Resolve<IPersistenceMappingGroup>("in-memory-01");
            Assert.IsInstanceOfType(instance, typeof(IPersistenceMappingGroup));
			Assert.IsNotNull(instance);
		}
	}
}