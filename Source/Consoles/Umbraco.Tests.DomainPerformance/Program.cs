using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using global::Umbraco.Tests.DomainDesign.GraphedModel;

namespace Umbraco.Tests.DomainPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                var root = GraphBuilder.GenerateRoot2();
                GraphBuilder.WalkAssociates2(root.CentrallyStoredChildren, 0, true);
            }
        }
    }
}
