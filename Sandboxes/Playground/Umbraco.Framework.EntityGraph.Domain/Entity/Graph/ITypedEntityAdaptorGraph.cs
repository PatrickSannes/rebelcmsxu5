using System.Collections.Generic;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    ///   Represents a collection of entity synonyms, specifically a list of entity vertices which can lead to other parts of the graph
    /// </summary>
    /// <typeparam name = "TEntityAdaptor">The type of the entity synonym.</typeparam>
    public interface ITypedEntityAdaptorGraph<TEntityAdaptor> : IList<ITypedEntityAdaptorGraph<TEntityAdaptor>>
        where TEntityAdaptor : ITypedEntity
    {
    }
}