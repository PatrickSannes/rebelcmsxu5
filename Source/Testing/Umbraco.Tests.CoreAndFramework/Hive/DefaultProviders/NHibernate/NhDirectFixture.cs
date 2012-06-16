using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.NHibernate
{
    using NUnit.Framework;

    using Umbraco.Framework.Persistence.RdbmsModel;

    using Umbraco.Tests.Extensions;

    using global::NHibernate;

    using global::NHibernate.Event;

    [TestFixture]
    public class NhDirectFixture
    {
        [Test]
        public void WhenAddingNodeVersionToSession_ThenClearingAndResettingAttributesCollection_BeforeCommitting_AttributesCollectionIsCorrect()
        {
            var setup = new NhibernateTestSetupHelper();
            var node = new Node() { IsDisabled = false };

            using (var tran = setup.SessionForTest.BeginTransaction())
            {
                node = setup.SessionForTest.Merge(node) as Node;
                tran.Commit();
            }
            setup.SessionForTest.Clear();

            NodeVersion nodeVersion = null;

            using (var tran = setup.SessionForTest.BeginTransaction())
            {
                // Get the node from the cleared session to mimic what the NH revision repository is doing
                node = setup.SessionForTest.Get<Node>(node.Id);

                var currentNodeId = node.Id;
                Assert.That(currentNodeId, Is.Not.EqualTo(Guid.Empty));

                var schema = new AttributeSchemaDefinition { Alias = "tes1", SchemaType = "content" };
                var type = new AttributeType { Alias = "type1" };
                var def = new AttributeDefinition
                    { Alias = "def1", AttributeDefinitionGroup = new AttributeDefinitionGroup { Alias = "group1", AttributeSchemaDefinition = schema}, AttributeSchemaDefinition = schema, AttributeType = type};
                schema.AttributeDefinitions.Add(def);
                nodeVersion = new NodeVersion { DefaultName = "test", Node = node, AttributeSchemaDefinition = schema };
                nodeVersion.Attributes.Add(new Attribute { AttributeDefinition = def, NodeVersion = nodeVersion });
                nodeVersion.Node = node;

                // Merge the version into the session, which will add it to 1st-level cache but we haven't committed to db yet
                nodeVersion = setup.SessionForTest.Merge(nodeVersion) as NodeVersion;

                var persister = setup.SessionForTest.GetSessionImplementation().PersistenceContext;

                nodeVersion.Attributes.Clear();
                nodeVersion.Attributes.Add(new Attribute { AttributeDefinition = def, NodeVersion = nodeVersion });

                nodeVersion = setup.SessionForTest.Merge(nodeVersion) as NodeVersion;

                node.NodeVersions.Add(nodeVersion);

                setup.SessionForTest.Merge(node);

                tran.Commit();
            }
            setup.SessionForTest.Clear();

            using (var tran = setup.SessionForTest.BeginTransaction())
            {
                var reloadVersion = setup.SessionForTest.Get<NodeVersion>(nodeVersion.Id);
                reloadVersion.Attributes.Clear();
                setup.SessionForTest.Merge(reloadVersion);
                tran.Commit();
            }

            using (var tran = setup.SessionForTest.BeginTransaction())
            {
                var reloadVersion = setup.SessionForTest.Get<NodeVersion>(nodeVersion.Id);
                Assert.That(reloadVersion.Attributes.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void SavingNodeVersion_ViaAddingToNodeCollection_UpdatesExistingInstance_WhenMerging()
        {
            var setup = new NhibernateTestSetupHelper();
            var node = new Node() { IsDisabled = false };

            using (var tran = setup.SessionForTest.BeginTransaction())
            {
                node = setup.SessionForTest.Merge(node) as Node;
                //setup.SessionForTest.SaveOrUpdate(node);
                tran.Commit();
            }

            Assert.That(node.Id, Is.Not.EqualTo(Guid.Empty));
            setup.SessionForTest.Clear();

            Assert.That(setup.SessionForTest.QueryOver<Node>().List().Count, Is.EqualTo(1));
            Assert.That(node.Id, Is.Not.EqualTo(Guid.Empty));

            NodeVersion nodeVersion = null;

            using (var tran = setup.SessionForTest.BeginTransaction())
            {
                // Get the node from the cleared session to mimic what the NH revision repository is doing
                node = setup.SessionForTest.Get<Node>(node.Id);

                var currentNodeId = node.Id;
                Assert.That(currentNodeId, Is.Not.EqualTo(Guid.Empty));

                var schema = new AttributeSchemaDefinition { Alias = "tes1", SchemaType = "content" };
                nodeVersion = new NodeVersion { DefaultName = "test", Node = node, AttributeSchemaDefinition = schema };

                nodeVersion = setup.SessionForTest.Merge(nodeVersion) as NodeVersion;


                node.NodeVersions.Add(nodeVersion);
                
                node = setup.SessionForTest.Merge(node) as Node;

                tran.Commit();
                Assert.That(currentNodeId, Is.EqualTo(node.Id));
                Assert.That(node.NodeVersions.Count, Is.EqualTo(1));
                Assert.That(node.NodeVersions.First().Id, Is.EqualTo(nodeVersion.Id));
                Assert.That(nodeVersion.Id, Is.Not.EqualTo(Guid.Empty));
            }

            setup.SessionForTest.Clear();

            Assert.That(setup.SessionForTest.QueryOver<Node>().List().Count, Is.EqualTo(2)); // One for the Node, one for the Schema
            Assert.That(node.Id, Is.Not.EqualTo(Guid.Empty));

            node = setup.SessionForTest.Get<Node>(node.Id);

            Assert.That(node.NodeVersions, Is.Not.Empty);
            Assert.That(node.NodeVersions.Count, Is.EqualTo(1));
            Assert.That(nodeVersion.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(node.NodeVersions.First().Id, Is.EqualTo(nodeVersion.Id));
        }
    }
}
