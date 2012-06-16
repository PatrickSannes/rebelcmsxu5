using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Testing.PartialTrust;

namespace Umbraco.Tests.CoreAndFramework.Hive.Model.Associations
{
    [TestFixture]
    public class RelationByIdTests : AbstractPartialTrustFixture<RelationByIdTests>
    {
        [Test]
        public void RelationById_Setting_Ids_In_Constructor_Works()
        {
            //Arrange
            var sourceId = new HiveId(1);
            var destinationId = new HiveId(2);
            var relationType = new RelationType("TestRelationType");

            //Act
            var relation = new RelationById(sourceId, destinationId, relationType, 0);

            //Assert
            Assert.IsTrue(relation.SourceId.ToString(HiveIdFormatStyle.AsUri) == sourceId.ToString(HiveIdFormatStyle.AsUri));
            Assert.IsTrue(relation.DestinationId.ToString(HiveIdFormatStyle.AsUri) == destinationId.ToString(HiveIdFormatStyle.AsUri));
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
