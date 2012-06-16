using System.Collections.Generic;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Graph
{
    /// <summary>
    ///   Represents a collection of entity synonyms, specifically a list of entity vertices which can lead to other parts of the graph
    /// </summary>
    /// <typeparam name = "TEntitySynonym">The type of the entity synonym.</typeparam>
    public interface IEntityAdaptorGraph<TEntitySynonym> : IList<IEntityAdaptorVertex<TEntitySynonym>>
        where TEntitySynonym : ITypedEntity
    {
    }
}