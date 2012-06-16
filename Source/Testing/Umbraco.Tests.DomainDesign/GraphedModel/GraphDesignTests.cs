using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Framework.Hive;

namespace Umbraco.Tests.DomainDesign.GraphedModel
{
    [TestClass]
    public class GraphDesignTests
    {
        private static RelationalModelRepository _modelRepository;
        private static EntityPivot _pivotRoot;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _modelRepository = new RelationalModelRepository();

            _pivotRoot = new EntityPivot(_modelRepository) { Id = Guid.Empty };
            var timer = new Stopwatch();
            timer.Start();
            var generateGraph = GraphBuilder.HiveGraphBuilder.GenerateGraph(_modelRepository, _pivotRoot, 0, 10, 15);
            Console.WriteLine("{0} EntityPivots created in {1}ms",
                              generateGraph, timer.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Create()
        {
            Assert.IsNotNull(_pivotRoot);
        }

        [TestMethod]
        public void CountEntities()
        {
            var timer = new Stopwatch();
            timer.Start();
            var countAllChildren = _pivotRoot.CountAllChildren();
            Console.WriteLine("{0} entities walked and counted in {1}ms", countAllChildren, timer.ElapsedMilliseconds);
            Assert.Inconclusive("No assert defined");
        }

        [TestMethod]
        public void EntitiesHavePermissions()
        {
            var entities = _pivotRoot.GetDescendants();
            foreach (var entityPivot in entities)
            {
                Assert.IsTrue(entityPivot.GetPermissions().Count() > 0);
            }
        }

        [TestMethod]
        public void GettingAllChildrenCreatesDistinctList()
        {
            var allChildren = _pivotRoot.GetAllChildren();
            Assert.AreEqual(allChildren.Count(), allChildren.Distinct().Count());
            Assert.AreEqual(allChildren.Select(x => x.Id), allChildren.Distinct().Select(x => x.Id));
            Assert.AreNotEqual(allChildren.Count(), allChildren.First().GetAllChildren().Count());
        }

        [TestMethod]
        public void CanGetEntityAssociates()
        {
            var entities = _pivotRoot.GetDescendants();
            Assert.IsTrue(entities.Count() > 0);
        }

        [TestMethod]
        public void CanGetEntityAssociatesLevel2()
        {
            var entities = _pivotRoot.GetDescendants().First().GetDescendants();
            Assert.IsTrue(entities.Count() > 0);
        }
    }
}