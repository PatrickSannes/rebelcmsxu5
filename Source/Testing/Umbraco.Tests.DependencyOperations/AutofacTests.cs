using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Foundation;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.DependencyManagement.Autofac;
using Umbraco.Tests.DependencyOperations.IoCStubs;

namespace Umbraco.Tests.DependencyOperations
{
    [TestClass]
    public class AutofacTests
    {
        private IContainerBuilder _containerBuilder;
        private UmbracoFactorySettings _factorySettings;

        [TestInitialize]
        public void TestInitialize()
        {
            //TODO: Switch over to using NUnit to avoid repeating test code for different providers (e.g. AutoFac and Ninject)
            _containerBuilder = new AutofacContainerBuilder();
            _factorySettings = new UmbracoFactorySettings();
        }

        [TestMethod]
        public void DefaultsAreRegistered()
        {
            // Arrange
            _factorySettings
                .SetContainerBuilder(new AutofacContainerBuilder())
                .SetupFrameworkForApps();

            IDependencyResolver container = _containerBuilder.Build();
            
            // Act
            var result = container.Resolve<IFrameworkContext>();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IFrameworkContext));
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
        public void CanRegisterAndResolveServiceWithRuntimeTypeAndMetadata()
        {
            // Arrange
            _containerBuilder.ForType<IMyInterface>(typeof (MyClassImplementingInterfaceWithMetadata))
                .Register()
                .WithMetadata<IMyMetadata, string>(a => a.StringValue, "sdasdasd")
                .WithMetadata<IMyMetadata, Guid>(a => a.GuidValue, Guid.NewGuid())
                .WithMetadata<IMyMetadata, int>(a => a.IntValue, 1237);

            _containerBuilder.ForType<IMyInterface>(typeof(MyClassImplementingInterfaceWithMetadata))
                .Register()
                .WithMetadata<IMyMetadata, string>(a => a.StringValue, "blahblah")
                .WithMetadata<IMyMetadata, Guid>(a => a.GuidValue, Guid.Empty)
                .WithMetadata<IMyMetadata, int>(a => a.IntValue, 7);

            DelegateTest delegateTest = new DelegateTest();
            delegateTest.GetGuid = Guid.NewGuid();
            var blahblah2 = "blahblah2";
            delegateTest.GetString = blahblah2;
            delegateTest.GetInt = 42;

            _containerBuilder.ForType<IMyInterface>(typeof(MyClassImplementingInterfaceWithMetadata))
                .Register()
                .WithMetadata<IMyMetadata, string>(a => a.StringValue, delegateTest.GetString)
                .WithMetadata<IMyMetadata, Guid>(a => a.GuidValue, delegateTest.GetGuid)
                .WithMetadata<IMyMetadata, int>(a => a.IntValue, delegateTest.GetInt);

            delegateTest.GetGuid = Guid.Empty;

            _containerBuilder.ForType<MyClassAcceptingResolvedMetadata, MyClassAcceptingResolvedMetadata>().Register();

            IDependencyResolver container = _containerBuilder.Build();

            // Act
            var result = container.Resolve<IMyInterface>();
            var autoResult = container.Resolve<MyClassAcceptingResolvedMetadata>();
            var propertyResult = result.MyStringProperty;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(autoResult);
            Assert.IsInstanceOfType(result, typeof(IMyInterface));
            Assert.IsTrue(propertyResult == blahblah2);
        }

        private class DelegateTest
        {
            public Guid GetGuid { get; set; }
            public int GetInt { get; set; }
            public string GetString { get; set; }
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
            //TODO: Refactor InstanceContainerBuilder to remove second type param
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
            //TODO: Refactor InstanceContainerBuilder to remove second type param
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
            Assert.IsInstanceOfType(result, typeof(IMyInterface));
            Assert.IsTrue(propertyResult == "this is my value");
        }

        [TestMethod]
        public void CanRegisterAndResolveServiceWithRuntimeTypeAndNamedConstructorParam()
        {
            // Arrange
            _containerBuilder.ForType<IMyInterface>(typeof(MyClassImplementingInterface)).Register().WithNamedParam("myParam",
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
        public void CanRegisterAndResolveServiceWithNamedRuntimeTypeAndNamedConstructorParam()
        {
            // Arrange
            var serviceName = "service";
            _containerBuilder.ForType<IMyInterface>(typeof(MyClassImplementingInterface)).RegisterNamed(serviceName).WithNamedParam("myParam",
                                                                                                                                                                                                                "this is my value");
            IDependencyResolver container = _containerBuilder.Build();

            // Act
            var result = container.Resolve<IMyInterface>(serviceName);
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
