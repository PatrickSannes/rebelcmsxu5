using System.Collections.Generic;

namespace Umbraco.Framework.Persistence.Abstractions
{
    /// <summary>
    /// Provides a simple 1-dimensional list of entities
    /// </summary>
    public interface IPersistenceEntityCollection<T> : IList<T> where T : IPersistenceEntity
    {
        void Add(HiveEntityUri identifier, T item);
        T this[HiveEntityUri index] { get; set; }
        T Get(HiveEntityUri id);
    }
}