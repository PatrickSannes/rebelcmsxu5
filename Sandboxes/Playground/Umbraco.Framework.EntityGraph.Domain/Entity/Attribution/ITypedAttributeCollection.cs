using System.Collections.Generic;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution
{
    /// <summary>
    /// Represents a collection of <see cref="ITypedAttribute"/>
    /// </summary>
    public interface ITypedAttributeCollection : IEntityGraph<ITypedAttribute>
    {
        ITypedAttribute this[string attributeName] { get; }
        ITypedAttribute Get(ITypedAttributeName attributeName);
        ITypedAttribute Get(string attributeName);
        bool Contains(string attributeName);
    }
}