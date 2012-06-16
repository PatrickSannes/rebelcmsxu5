using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    ///   Represents a graph of entities through a list of entity vertices
    /// </summary>
    public interface IEntityGraph : IDictionary<IMappedIdentifier, IEntityVertex>, IEnumerable<IEntityVertex>
    {
    }
}