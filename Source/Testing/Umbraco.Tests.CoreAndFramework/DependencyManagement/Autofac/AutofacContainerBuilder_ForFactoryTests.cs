using NUnit.Framework;
using Umbraco.Framework.DependencyManagement.Autofac;
using Umbraco.Framework.Testing.PartialTrust;

namespace Umbraco.Tests.CoreAndFramework.DependencyManagement.Autofac
{
    [TestFixture]
    public class AutofacContainerBuilder_ForFactoryTests : AbstractPartialTrustFixture<AutofacContainerBuilder_ForFactoryTests>
    {
        public class Bar
        {
            
        }

        public class Foo
        {
            public Bar Bar { get; set; }
        }

        [Test]
        public void AutofacContainerBuilder_ForFactoryTests_ForFactory_RegisteredTypeComesFromDelegate()
        {
            //Arrange
            var autofacContainerBuilder = new AutofacContainerBuilder();

            //Act
            autofacContainerBuilder.ForFactory(x => new Foo());
            var resolver = autofacContainerBuilder.Build();

            //Assert
            Assert.IsNotNull(resolver.Resolve<Foo>());
        }

        [Test]
        public void AutofacContainerBuilder_ForFactoryTests_ForFactory_DelegateCanSetProperties()
        {
            //Arrange
            var autofacContainerBuilder = new AutofacContainerBuilder();

            //Act
            autofacContainerBuilder.ForFactory(x => new Foo() { Bar = new Bar()});
            var resolver = autofacContainerBuilder.Build();

            //Assert
            Assert.IsNotNull(resolver.Resolve<Foo>().Bar);
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
