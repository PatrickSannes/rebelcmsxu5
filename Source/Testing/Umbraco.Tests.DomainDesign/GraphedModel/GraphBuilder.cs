using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework.Hive;
using Umbraco.Framework.Hive.Relations;

namespace Umbraco.Tests.DomainDesign.GraphedModel
{
    /// <summary>
    /// Summary description for GraphBuilder
    /// </summary>
    public static class GraphBuilder
    {
        public static class HiveGraphBuilder
        {
            public static IList<EntityPivot> PermissionEntities = new List<EntityPivot>();

            public static IEnumerable<EntityPivot> GetPermissionEntities(EntityPivot parent)
            {
                if (PermissionEntities.Count == 0)
                {
                    PermissionEntities.Add(new EntityPivot(parent));
                    PermissionEntities.Add(new EntityPivot(parent));
                    PermissionEntities.Add(new EntityPivot(parent));
                }
                return PermissionEntities;
            }

            public static int GenerateGraph(RelationalModelRepository repo, EntityPivot parent, int depth, int maxDepth, int branchSize)
            {
                if (depth == maxDepth)
                    return 0;

                var runningTotalCreated = 0;

                var children = new List<Relation>();
                for (var i = 0; i < branchSize; i++)
                {
                    var child = new EntityPivot(parent) { Id = Guid.NewGuid() };
                    var relation = new ContentOneToManyRelation(parent, child);
                    children.Add(relation);

                    children.AddRange(from permissionEntity in GetPermissionEntities(child)
                                      let metadata = new RelationMetadata(PermissionRelationType.Default)
                                                         {
                                                             {"allow", "read"},
                                                             {"deny", "write"}
                                                         }
                                      select new PermissionRelation(metadata, child, permissionEntity));

                    runningTotalCreated++;
                    runningTotalCreated += GenerateGraph(repo, child, depth + 1, maxDepth, branchSize);
                }
                parent.AddAssociates(children);
                return runningTotalCreated;
            }
        }

        public static RootEntity GenerateRoot0()
        {
            var rootEntity = new RootEntity { Id = "Root", IsRoot = true };
            GenerateChildren0(rootEntity, 0, 5, 10);
            return rootEntity;
        }

        public static RootEntity GenerateRoot1()
        {
            var rootEntity = new RootEntity {Id = "Root", IsRoot = true};
            GenerateChildren1(rootEntity, 0, 5, 10);
            return rootEntity;
        }

        public static RootEntity GenerateRoot2()
        {
            var rootEntity = new RootEntity {Id = "Root", IsRoot = true};
            GenerateChildren2(rootEntity, 0, 5, 10);
            return rootEntity;
        }

        public static void GenerateChildren0(Entity parent, int depth, int maxDepth, int branchSize)
        {
            if (depth == maxDepth)
                return;

            var children = new HashSet<Entity>();
            for (int i = 0; i < branchSize; i++)
            {
                var child = new Entity(parent.RootEntity) { Id = String.Format("Level{0}-{1}", depth, i), Parent = parent };
                GenerateChildren0(child, depth + 1, maxDepth, branchSize);
                children.Add(child);
            }
            parent.ClassicLocallyStoredChildren = children;
        }

        public static void GenerateChildren1(Entity parent, int depth, int maxDepth, int branchSize)
        {
            if (depth == maxDepth)
                return;

            var children = new List<Entity>();
            for (var i = 0; i < branchSize; i++)
            {
                var child = new Entity(parent.RootEntity)
                                {Id = String.Format("Level{0}-{1}", depth, i), Parent = parent};
                GenerateChildren1(child, depth + 1, maxDepth, branchSize);
                children.Add(child);
            }
            parent.LocallyStoredChildrenViaDictionary1 = children;
        }

        public static void GenerateChildren2(Entity parent, int depth, int maxDepth, int branchSize)
        {
            if (depth == maxDepth)
                return;

            var children = new List<Entity>();
            for (var i = 0; i < branchSize; i++)
            {
                var child = new Entity(parent.RootEntity)
                                {Id = String.Format("Level{0}-{1}", depth, i), Parent = parent};
                    // Parent to be re-wired if master list idea works
                GenerateChildren2(child, depth + 1, maxDepth, branchSize);
                children.Add(child);
            }
            parent.SetChildren2(children);
        }


        public static void WalkAssociates(List<Entity> children, int indentDepth, bool output)
        {
            var indent = String.Empty.PadLeft(indentDepth, '\t');
            children.ForEach(a =>
                                 {
                                     if (output) Console.WriteLine("Version 1 - {0}{1}", indent, a.Id);
                                     WalkAssociates(new List<Entity>(a.LocallyStoredChildrenViaDictionary1), indentDepth + 1, output);
                                 });
        }

        public static void WalkAssociates2(IEnumerable<Entity> children, int indentDepth, bool output)
        {
            var indent = String.Empty.PadLeft(indentDepth, '\t');
            foreach (var entity in children)
            {
                var nextChildren = entity.FakeGetChildren();
                if (output) Console.WriteLine("Version2 - {0}{1}", indent, entity.Id);
                WalkAssociates2(nextChildren, indentDepth + 1, output);
            }
        }
    }
}