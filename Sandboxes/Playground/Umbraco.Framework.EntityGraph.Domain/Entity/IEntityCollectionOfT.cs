using System.Collections.Generic;

namespace Umbraco.Framework.EntityGraph.Domain.Entity
{
    /// <summary>
    /// Provides a simple 1-dimensional list of entities
    /// </summary>
    public interface IEntityCollection<T> : IList<T> where T : IEntity
    {
        //TODO: Why do we have this class? IMO we should get rid of it, nothing wrong with standard List<T> - Az
    }
}