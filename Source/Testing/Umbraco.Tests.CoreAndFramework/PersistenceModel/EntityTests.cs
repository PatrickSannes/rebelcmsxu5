using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.PersistenceModel
{
    [TestFixture]
    public class EntityTests : AbstractPartialTrustFixture<EntityTests>
    {
        [Test]
        public void Multiple_NewEntities_WithEmptyIds_AreNotEqual()
        {
            // Arrange
            var entity1 = HiveModelCreationHelper.MockTypedEntity(false);
            var entity2 = HiveModelCreationHelper.MockTypedEntity(false);

            // Assert
            Assert.AreEqual(HiveId.Empty, entity1.Id);
            Assert.AreEqual(HiveId.Empty, entity2.Id);
            Assert.AreNotEqual(entity1, entity2);
        }

        public override void TestSetup()
        {
            return;
        }

        public override void TestTearDown()
        {
            return;
        }
    }
}