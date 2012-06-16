using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Testing.PartialTrust;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    [TestFixture]
    public class RelationSerializerTests : AbstractPartialTrustFixture<RelationSerializerTests>
    {
        class StubRelatableEntity : IRelatableEntity
        {
            public StubRelatableEntity(HiveId id)
            {
                Id = id;
                RelationProxies = new RelationProxyCollection(this);
            }

            public HiveId Id { get; set; }

            /// <summary>
            /// A store of relation proxies for this entity, to support enlisting relations to this entity.
            /// The relations will not be persisted until the entity is passed to a repository for saving.
            /// If <see cref="RelationProxyCollection.IsConnected"/> is <code>true</code>, this sequence may have
            /// <see cref="RelationProxy"/> objects lazily loaded by enumerating the results of calling <see cref="RelationProxyCollection.LazyLoadDelegate"/>.
            /// </summary>
            /// <value>The relation proxies.</value>
            public RelationProxyCollection RelationProxies { get; private set; }

            public DateTimeOffset UtcCreated { get; set; }

            public DateTimeOffset UtcModified { get; set; }

            public DateTimeOffset UtcStatusChanged { get; set; }

            public Umbraco.Framework.Data.Common.IConcurrencyToken ConcurrencyToken { get; set; }
        }

        [Test]
        public void RelationSerializerTests_ToXml_Works()
        {
            //Arrange
            var source = new StubRelatableEntity(new HiveId(Guid.NewGuid()));
            var destination = new StubRelatableEntity(new HiveId(Guid.NewGuid()));
            var relationType = new RelationType("TestRelationType");
            var relation = new Relation(relationType, source, destination, new [] {
                new RelationMetaDatum("test1", "value1"),
                new RelationMetaDatum("test2", "value2")
            });

            //Act
            var xml = RelationSerializer.ToXml(relation);

            //Assert
            Assert.IsTrue(xml.Root.Name == "relation");
            Assert.IsTrue(xml.Root.Attribute("type").Value == relationType.RelationName);
            Assert.IsTrue(xml.Root.Attribute("sourceId").Value == source.Id.ToString(HiveIdFormatStyle.AsUri));
            Assert.IsTrue(xml.Root.Attribute("destinationId").Value == destination.Id.ToString(HiveIdFormatStyle.AsUri));
            Assert.IsTrue(xml.Root.Attribute("ordinal").Value == "0");
            Assert.IsTrue(xml.Root.HasElements);
            Assert.IsTrue(xml.Root.Elements("metaDatum").Count() == 2);
            Assert.IsTrue(xml.Root.Elements("metaDatum").Single(x => x.Attribute("key").Value == "test1").Attribute("value").Value == "value1");
            Assert.IsTrue(xml.Root.Elements("metaDatum").Single(x => x.Attribute("key").Value == "test2").Attribute("value").Value == "value2");
        }

        [Test]
        public void RelationSerializerTests_FromXml_Works()
        {
            //Arrange
            var sourceId = new HiveId(Guid.NewGuid());
            var destinationId = new HiveId(Guid.NewGuid());
            var relationType = new RelationType("TestRelationType");
            var xml = string.Format(
                @"<relation type=""{0}"" sourceId=""{1}"" destinationId=""{2}"" ordinal=""0"">
                    <metaDatum key=""test1"" value=""value1"" />
                    <metaDatum key=""test2"" value=""value2"" />
                 </relation>",
                 relationType.RelationName,
                 sourceId.ToString(HiveIdFormatStyle.AsUri),
                 destinationId.ToString(HiveIdFormatStyle.AsUri));

            //Act
            var relation = RelationSerializer.FromXml(xml);

            //Assert
            Assert.IsTrue(relation != null);
            Assert.IsTrue(relation.Type.RelationName == relationType.RelationName);
            Assert.IsTrue(relation.SourceId.ToString(HiveIdFormatStyle.AsUri) == sourceId.ToString(HiveIdFormatStyle.AsUri));
            Assert.IsTrue(relation.DestinationId.ToString(HiveIdFormatStyle.AsUri) == destinationId.ToString(HiveIdFormatStyle.AsUri));
            Assert.IsTrue(relation.Ordinal == 0);
            Assert.IsTrue(relation.MetaData.Count == 2);
            Assert.IsTrue(relation.MetaData.Single(x => x.Key == "test1").Value == "value1");
            Assert.IsTrue(relation.MetaData.Single(x => x.Key == "test2").Value == "value2");
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
