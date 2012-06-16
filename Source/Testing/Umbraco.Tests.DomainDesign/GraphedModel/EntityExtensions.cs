using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Umbraco.Tests.DomainDesign.GraphedModel
{
    public static class EntityExtensions
    {
        public static IEnumerable<Entity> GetAllChildren0(this Entity entity)
        {
            foreach (Entity child in entity.ClassicLocallyStoredChildren)
            {
                yield return child;

                foreach (Entity allChild in child.GetAllChildren0())
                {
                    yield return allChild;
                }
            }
        }

        public static IEnumerable<Entity> GetAllChildren1(this Entity entity)
        {
            foreach (Entity child in entity.LocallyStoredChildrenViaDictionary1)
            {
                yield return child;

                foreach (Entity allChild in child.GetAllChildren1())
                {
                    yield return allChild;
                }
            }
        }

        public static IEnumerable<Entity> GetAllChildren2(this Entity entity)
        {
            foreach (Entity child in entity.CentrallyStoredChildren)
            {
                yield return child;

                foreach (Entity allChild in child.GetAllChildren2())
                {
                    yield return allChild;
                }
            }
        }

        public static void WalkAllChildren0(this Entity entity, int indentDepth, bool output)
        {
            string indent = String.Empty.PadLeft(indentDepth, '\t');
            foreach (var child in entity.ClassicLocallyStoredChildren)
            {
                if (output) Console.WriteLine("EVersion1 - {0}{1}", indent, child.Id);
                child.WalkAllChildren0(indentDepth + 1, output);
            }
        }

        public static void WalkAllChildren1(this Entity entity, int indentDepth, bool output)
        {
            string indent = String.Empty.PadLeft(indentDepth, '\t');
            foreach (var child in entity.LocallyStoredChildrenViaDictionary1)
            {
                if (output) Console.WriteLine("EVersion1 - {0}{1}", indent, child.Id);
                child.WalkAllChildren1(indentDepth + 1, output);
            }
        }

        public static void WalkAllChildren2(this Entity entity, int indentDepth, bool output)
        {
            string indent = String.Empty.PadLeft(indentDepth, '\t');
            foreach (var child in entity.CentrallyStoredChildren)
            {
                if (output) Console.WriteLine("EVersion2 - {0}{1}", indent, child.Id);
                child.WalkAllChildren2(indentDepth + 1, output);
            }
        }

        public static int CountAllChildren0(this Entity entity)
        {
            int count = 0;
            foreach (Entity child in entity.ClassicLocallyStoredChildren)
            {
                count += child.CountAllChildren0();
            }
            return entity.ClassicLocallyStoredChildren.Count() + count;
        }

        public static int CountAllChildren1(this Entity entity)
        {
            int count = 0;
            foreach (Entity child in entity.LocallyStoredChildrenViaDictionary1)
            {
                count += child.CountAllChildren1();
            }
            return entity.LocallyStoredChildrenViaDictionary1.Count() + count;
        }

        public static int CountAllChildren2(this Entity entity)
        {
            int count = 0;
            foreach (Entity child in entity.CentrallyStoredChildren)
            {
                count += child.CountAllChildren1();
            }
            return entity.CentrallyStoredChildren.Count() + count;
        }
    }
}