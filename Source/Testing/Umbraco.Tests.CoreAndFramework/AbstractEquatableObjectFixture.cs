namespace Umbraco.Tests.CoreAndFramework
{
    using System;

    using NUnit.Framework;

    using Umbraco.Framework;

    [TestFixture]
    public class AbstractEquatableObjectFixture
    {
        class AnObjectWithNaturalId : AbstractEquatableObject<AnObjectWithNaturalId>
        {
            public Guid Id { get; set; }

            protected override System.Collections.Generic.IEnumerable<System.Reflection.PropertyInfo> GetMembersForEqualityComparison()
            {
                return new[] { this.GetPropertyInfo(x => x.Id) };
            }
        }

        [Test]
        public void WhenIdChanges_HashcodeIsSame()
        {
            var obj = new AnObjectWithNaturalId() { Id = Guid.Empty };
            var hashcode = obj.GetHashCode();
            obj.Id = Guid.NewGuid();
            var newHashcode = obj.GetHashCode();

            Assert.That(newHashcode, Is.EqualTo(hashcode));
        }
    }
}
