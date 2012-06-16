using System;
using System.Collections.Generic;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    ///   Represents a graph of entities through a list of entity vertices
    /// </summary>
    public interface IEntityGraph<T> : IEnumerable<T> where T : IEntityVertex
    {
        void Add(IMappedIdentifier identifier, Func<T> item);
        void Add(IMappedIdentifier identifier, T Item);
        void Add(IMappedIdentifier identifier, Lazy<T> item);

        void Clear();

        int Count { get; }
    }
}