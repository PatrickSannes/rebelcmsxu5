using NUnit.Framework;
using Umbraco.Framework.DependencyManagement.Autofac;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Framework.Testing.PartialTrust;

namespace Umbraco.Tests.CoreAndFramework.DependencyManagement.Autofac
{
    [TestFixture]
    public class AutofacContainerBuilderTests : AbstractPartialTrustFixture<AutofacContainerBuilderTests>
    {
        public interface IFoo
        {
        }

        public class Foo : IFoo
        {
        }

        [Test]
        public void AutofacContainerBuilderTests_TypesRegistrableAsInterfaces()
        {
            //Arrange
            var autofacContainerBuilder = new AutofacContainerBuilder();

            //Act
            autofacContainerBuilder.For<Foo>()
                .KnownAs<IFoo>();
            var dependencyResolver = autofacContainerBuilder.Build();

            //Assert
            Assert.IsNotNull(dependencyResolver.Resolve<IFoo>());
        }

        [Test]
        public void AutofacContainerBuilderTests_TypesRegistrableUsingImplementation()
        {
            //Arrange
            var autofacContainerBuilder = new AutofacContainerBuilder();

            //Act
            autofacContainerBuilder.ForInstanceOfType(new Foo())
                .KnownAs(typeof(IFoo));
            var dependencyResolver = autofacContainerBuilder.Build();

            //Assert
            Assert.IsNotNull(dependencyResolver.Resolve<IFoo>());
        }

        [Test]
        public void AutofacContainerBuilderTests_TypesRegistrableAsInterfaces_NewAPI()
        {
            //Arrange
            var autofacContainerBuilder = new AutofacContainerBuilder();

            //Act
            autofacContainerBuilder.For(typeof (Foo))
                .KnownAs(typeof(IFoo));
            var dependencyResolver = autofacContainerBuilder.Build();

            //Assert
            Assert.IsNotNull(dependencyResolver.Resolve<IFoo>());
        }

        [Test]
        public void AutofacContainerBuilderTests_TypesRegistrableAsSelf_NewAPI()
        {
            //Arrange
            var autofacContainerBuilder = new AutofacContainerBuilder();

            //Act
            autofacContainerBuilder.For(typeof(Foo));
            var dependencyResolver = autofacContainerBuilder.Build();

            //Assert
            Assert.IsNotNull(dependencyResolver.Resolve<Foo>());
        }

        [Test]
        public void AutofacContainerBuilderTests_ForInstanceOfType_RegisteredTypeResoslvable()
        {
            //Arrange
            var autofacContainerBuilder = new AutofacContainerBuilder();

            //Act
            autofacContainerBuilder.ForInstanceOfType(new Foo());
            var resolver = autofacContainerBuilder.Build();

            //Assert
            Assert.IsNotNull(resolver.Resolve<Foo>());
        }

        [Test]
        public void AutofacContainerBuilderTests_NamedRegistration_ResolvableByName()
        {
            //Arrange
            var builder = new AutofacContainerBuilder();

            //Act
            builder.ForInstanceOfType(new Foo())
                .Named<Foo>("name")
                ;
            var resolver = builder.Build();

            //Assert
            Assert.IsNotNull(resolver.Resolve<Foo>("name"));
        }

        [Test]
        public void AutofacContainerBuilderTests_NamedRegistration_InvalidNameFailsResolution()
        {
            //Arrange
            var builder = new AutofacContainerBuilder();

            //Act
            builder.ForInstanceOfType(new Foo())
                .Named<Foo>("name");
            var resolver = builder.Build();

            //Assert
            var foo = resolver.TryResolve<Foo>("invalid");
            Assert.IsNotNull(foo);
            Assert.IsFalse(foo.Success);
        }

        /// <summary>
        /// Run once before each test in derived test fixtures.
        /// </summary>
        public override void TestSetup()
        {
            return;
        }

        /// <summary>
        /// Run once after each test in derived test fixtures.
        /// </summary>
        public override void TestTearDown()
        {
            return;
        }
    }
}
