using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Framework.DependencyManagement.Autofac;
using Umbraco.Framework.Testing.PartialTrust;

namespace Umbraco.Tests.CoreAndFramework.DependencyManagement.Autofac
{
    [TestFixture]
    public class AutofacContainerBuilder_WithTests : AbstractPartialTrustFixture<AutofacContainerBuilder_WithTests>
    {
        public class Foo
        {
            
        }

        public class Metadata
        {
            public Metadata(IDictionary<string, object> obj)
            {
                Value = (string) obj["Value"];
            }
            public string Value { get; set; }
        }

        [Test]
        public void AutofacContainerBuilder_WithTests_ProvidedMetaData_MappedToInstance()
        {
            //Arrange
            var builder = new AutofacContainerBuilder();

            //Act
            builder.For<Foo>()
                .WithMetadata<Metadata, string>(am => am.Value, "test");

            var resolver = builder.Build();

            //Assert
            var lazyFoo = resolver.Resolve<Lazy<Foo, Metadata>>();
            Assert.IsNotNull(lazyFoo);
            Assert.AreEqual("test", lazyFoo.Metadata.Value);
        }

        [Test]
        public void AutofacContainerBuilder_WithTests_ForFactory_ThrowsException()
        {
            //Arrange
            var builder = new AutofacContainerBuilder();

            //Act
            Assert.Throws<TypeAccessException>(
                () => builder.ForFactory(x => new Foo())
                          .WithResolvedParam(x => (string) null));
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
