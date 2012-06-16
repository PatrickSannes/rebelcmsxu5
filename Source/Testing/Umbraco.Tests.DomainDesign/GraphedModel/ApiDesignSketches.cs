using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Umbraco.Tests.DomainDesign.GraphedModel
{
    [TestClass]
    public class ApiDesignSketches
    {
        [TestMethod]
        public void IndexedCollectionHasEqualInnerListSizes()
        {
            Assert.Inconclusive("Ignored for now - May 2011 APN");

            var graph = GraphBuilder.GenerateRoot2();
            Assert.Inconclusive("No assert");
        }

        [TestMethod]
        public void WalkViaStandardAxis()
        {
            Assert.Inconclusive("Ignored for now - May 2011 APN");

            var timer = new Stopwatch();
            timer.Start();

            RootEntity root0 = null;
            RootEntity root1 = null;
            RootEntity root2 = null;

            for (var i = 0; i < 50; i++)
            {
                root0 = GraphBuilder.GenerateRoot0();
            }
            Console.WriteLine("Took {0}ms to generate via classic (recursive)", timer.ElapsedMilliseconds);
            timer.Restart();

            for (var i = 0; i < 50; i++)
            {
                root1 = GraphBuilder.GenerateRoot1();
            }
            Console.WriteLine("Took {0}ms to generate via stack reference (recursive)", timer.ElapsedMilliseconds);

            timer.Restart();
            for (int i = 0; i < 50; i++)
            {
                root2 = GraphBuilder.GenerateRoot2();
            }
            Console.WriteLine("Took {0}ms to generate via central list (non-recursive)", timer.ElapsedMilliseconds);

            timer.Restart();

            var graph0 = new List<Entity>(new[] { root0 });
            var graph1 = new List<Entity>(new[] { root1 });
            var graph2 = new List<Entity>(new[] { root2 });

            timer.Restart();
            for (var i = 0; i < 50; i++)
            {
                //GraphBuilder.WalkAssociates(graph1, 0, false);
                graph0.First().WalkAllChildren0(0, false);
            }
            Console.WriteLine("Took {0}ms to walk via classic (recursive) after 50 iterations",
                              timer.ElapsedMilliseconds);

            timer.Restart();
            for (var i = 0; i < 50; i++)
            {
                //GraphBuilder.WalkAssociates(graph1, 0, false);
                graph1.First().WalkAllChildren1(0, false);
            }
            Console.WriteLine("Took {0}ms to walk via stack reference (recursive) after 50 iterations",
                              timer.ElapsedMilliseconds);

            timer.Restart();
            for (var i = 0; i < 50; i++)
            {
                //GraphBuilder.WalkAssociates2(graph2, 0, false);
                graph2.First().WalkAllChildren2(0, false);
            }
            Console.WriteLine("Took {0}ms to walk via central list after 50 iterations", timer.ElapsedMilliseconds);

            timer.Restart();
            Console.WriteLine("{0} children counted using method 0 in {1}ms", graph0.First().GetAllChildren0().ToList().Count(), timer.ElapsedMilliseconds);
            
            timer.Restart();
            Console.WriteLine("{0} children counted using method 1 in {1}ms", graph1.First().GetAllChildren1().ToList().Count(), timer.ElapsedMilliseconds);
            timer.Restart();
            Console.WriteLine("{0} children counted using method 2 in {1}ms", graph2.First().GetAllChildren2().ToList().Count(), timer.ElapsedMilliseconds);
            timer.Restart();
            graph0.First().WalkAllChildren0(0, false);
            Console.WriteLine("Classically stored children walked using method 0 in {0}ms", timer.ElapsedMilliseconds);
            timer.Restart();
            graph1.First().WalkAllChildren1(0, false);
            Console.WriteLine("LocallyStoredChildrenViaDictionary1 walked using method 1 in {0}ms", timer.ElapsedMilliseconds);
            timer.Restart();
            graph2.First().WalkAllChildren2(0, false);
            Console.WriteLine("Children walked using method 2 in {0}ms", timer.ElapsedMilliseconds);

            //GraphBuilder.WalkAssociates(graph1, 0, true);
            graph1.First().WalkAllChildren1(0, true);
            //GraphBuilder.WalkAssociates2(graph2, 0, true);
            graph2.First().WalkAllChildren2(0, true);

            Assert.Inconclusive("No assert");
        }
    }
}