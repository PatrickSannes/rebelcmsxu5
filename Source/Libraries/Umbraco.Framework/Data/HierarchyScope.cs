namespace Umbraco.Framework.Data
{
    using System;

    [Flags]
    public enum HierarchyScope
    {
        AllOrNone = 0,
        Parent = 2, // One
        Ancestors = 4, // Many
        AncestorsOrSelf = 8, // Many including self
        Descendents = 16, // All
        DescendentsOrSelf = 32, // All including self
        Children = 64, // Just one level below
        Parents = 128 // Many immediate ancestors (one level 'up')
    }

    [Flags]
    public enum Direction
    {
        Parents = 0,
        Ancestors = 2,
        Descendents = 4,
        Children = 8,
        Siblings = 16
    }
}