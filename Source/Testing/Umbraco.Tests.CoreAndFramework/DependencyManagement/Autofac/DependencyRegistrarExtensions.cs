using NUnit.Framework;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.DependencyManagement.Autofac;
using Umbraco.Framework.Testing.PartialTrust;

namespace Umbraco.Tests.CoreAndFramework.DependencyManagement.Autofac
{
    [TestFixture]
    public class DependencyRegistrarExtensions : AbstractPartialTrustFixture<DependencyRegistrarExtensions>
    {
        public class Foo
        {
        }

        [Test]
        public void DependencyRegistrarExtensions_KnwonAsSelf_RegistersTypeAsItself()
        {
            //Arrange
            var builder = new AutofacContainerBuilder();

            //Act
            builder.For(typeof(Foo))
                .KnownAsSelf();
            var resolver = builder.Build();

            //Assert
            Assert.IsNotNull(resolver.Resolve<Foo>());
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
