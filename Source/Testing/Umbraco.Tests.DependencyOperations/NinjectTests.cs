using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.DependencyManagement.Ninject;
using Umbraco.Tests.DependencyOperations.IoCStubs;

namespace Umbraco.Tests.DependencyOperations
{
	[TestClass]
	public class NinjectTests
	{
		private IContainerBuilder _containerBuilder;

		[TestInitialize]
		public void TestInitialize()
		{
			_containerBuilder = new NinjectContainerBuilder();
		}

		[TestMethod]
		public void CanRegisterAndResolveService()
		{
			// Arrange
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register();
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result = container.Resolve<IMyInterface>();

			// Assert
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(IMyInterface));
		}

		[TestMethod]
		public void CanRegisterAndResolveServiceWithRuntimeType()
		{
			// Arrange
			_containerBuilder.ForType<IMyInterface>(typeof(MyClassImplementingInterface)).Register();
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result = container.Resolve<IMyInterface>();

			// Assert
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(IMyInterface));
		}

		[TestMethod]
		public void CanRegisterAndResolveInstance()
		{
			// Arrange
			var instance = new MyClassImplementingInterface();
			_containerBuilder.ForInstanceOfType<IMyInterface>().Register(instance);
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result = container.Resolve<IMyInterface>();

			// Assert
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(IMyInterface));
		}


		[TestMethod]
		public void CanRegisterAndResolveInstanceByName()
		{
			// Arrange
			var instance = new MyClassImplementingInterface();
			var instance2 = new MyClassImplementingInterface("this is a difference instance");
			_containerBuilder.ForInstanceOfType<IMyInterface>().Register(instance);
			_containerBuilder.ForInstanceOfType<IMyInterface>().Register(instance2, "named");
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result1 = container.Resolve<IMyInterface>();
			var result2 = container.Resolve<IMyInterface>("named");

			// Assert
			Assert.IsNotNull(result1);
			Assert.IsInstanceOfType(result1, typeof(IMyInterface));
			Assert.IsNotNull(result2);
			Assert.IsInstanceOfType(result2, typeof(IMyInterface));
			Assert.AreNotSame(result1, result2);
		}

		[TestMethod]
		public void CanRegisterAndResolveServiceWithNamedConstructorParam()
		{
			// Arrange
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register().WithNamedParam("myParam",
			                                                                                                  "this is my value");
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result = container.Resolve<IMyInterface>();
			var propertyResult = result.MyStringProperty;

			// Assert
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof (IMyInterface));
			Assert.IsTrue(propertyResult == "this is my value");
		}

		[TestMethod]
		public void CanRegisterAndResolveServiceWithTypedConstructorParam()
		{
			// Arrange
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register().WithTypedParam<string>(
				"this is my value");
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result = container.Resolve<IMyInterface>();
			var propertyResult = result.MyStringProperty;

			// Assert
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(IMyInterface));
			Assert.IsTrue(propertyResult == "this is my value");
		}

		[TestMethod]
		public void CanRegisterAndResolveServiceWithNamedConstructorParamResolvedByContainer()
		{
			// Arrange
			_containerBuilder.ForType<IMyParamTypeInterface, MyClassImplementingParamInterface>().Register().ScopedAsSingleton();
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register().WithResolvedParam<IMyParamTypeInterface>();
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result = container.Resolve<IMyInterface>();
			var paramResult = container.Resolve<IMyParamTypeInterface>();
			var propertyResult = result.MyService;

			// Assert
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.MyService);
			Assert.IsInstanceOfType(result, typeof(IMyInterface));
			Assert.AreSame(result.MyService, paramResult);
		}

		[TestMethod]
		public void CanRegisterAndResolveServiceWithCallbackConstructorParam()
		{
			// Arrange
			_containerBuilder.ForType<IMyParamTypeInterface, MyClassImplementingParamInterface>().Register().ScopedAsSingleton();
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register().WithResolvedParam(a => a.Resolve<IMyParamTypeInterface>());
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result = container.Resolve<IMyInterface>();
			var paramResult = container.Resolve<IMyParamTypeInterface>();
			var propertyResult = result.MyService;

			// Assert
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.MyService);
			Assert.IsInstanceOfType(result, typeof(IMyInterface));
			Assert.AreSame(result.MyService, paramResult);
		}

		[TestMethod]
		public void CanRegisterAndResolveServiceWithNamedCallbackConstructorParam()
		{
			// Arrange
			_containerBuilder.ForType<IMyParamTypeInterface, MyClassImplementingParamInterface>().Register().ScopedAsSingleton();
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register().WithNamedResolvedParam("myService", a => a.Resolve<IMyParamTypeInterface>());
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result = container.Resolve<IMyInterface>();
			var paramResult = container.Resolve<IMyParamTypeInterface>();
			var propertyResult = result.MyService;

			// Assert
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.MyService);
			Assert.IsInstanceOfType(result, typeof(IMyInterface));
			Assert.AreSame(result.MyService, paramResult);
		}

		[TestMethod]
		public void CanRegisterAndResolveNamedService()
		{
			// Arrange
			// - Register non-named service
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register();
			// - Register named service of same type
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register("myName");
			// - Build container
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var unnamedResult = container.Resolve<IMyInterface>();
			var namedResult = container.Resolve<IMyInterface>("myName");

			// Assert
			Assert.IsNotNull(unnamedResult);
			Assert.IsNotNull(namedResult);
			Assert.IsInstanceOfType(namedResult, typeof(IMyInterface));
			Assert.AreNotSame(unnamedResult, namedResult);
		}

		[TestMethod]
		public void CanRegisterAndResolveNamedServiceWithRuntimeType()
		{
			// Arrange
			// - Register non-named service
			_containerBuilder.ForType<IMyInterface>(typeof(MyClassImplementingInterface)).Register();
			// - Register named service of same type
			_containerBuilder.ForType<IMyInterface>(typeof(MyClassImplementingInterface)).RegisterNamed("myName");
			// - Build container
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var unnamedResult = container.Resolve<IMyInterface>();
			var namedResult = container.Resolve<IMyInterface>("myName");

			// Assert
			Assert.IsNotNull(unnamedResult);
			Assert.IsNotNull(namedResult);
			Assert.IsInstanceOfType(namedResult, typeof(IMyInterface));
			Assert.AreNotSame(unnamedResult, namedResult);
		}

		[TestMethod]
		public void CanRegisterAndResolveServiceNotSingletonScoped()
		{
			// Arrange
			// - Register service as singleton-scoped
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register();
			// - Build container
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result1 = container.Resolve<IMyInterface>();
			var result2 = container.Resolve<IMyInterface>();

			// Assert
			Assert.IsNotNull(result1);
			Assert.IsNotNull(result2);
			Assert.IsInstanceOfType(result2, typeof(IMyInterface));
			Assert.AreNotSame(result1, result2);
		}

		[TestMethod]
		public void CanRegisterAndResolveServiceSingletonScoped()
		{
			// Arrange
			// - Register service as singleton-scoped
			_containerBuilder.ForType<IMyInterface, MyClassImplementingInterface>().Register().ScopedAsSingleton();
			// - Build container
			IDependencyResolver container = _containerBuilder.Build();

			// Act
			var result1 = container.Resolve<IMyInterface>();
			var result2 = container.Resolve<IMyInterface>();

			// Assert
			Assert.IsNotNull(result1);
			Assert.IsNotNull(result2);
			Assert.IsInstanceOfType(result2, typeof(IMyInterface));
			Assert.AreSame(result1, result2);
		}
	}
}
