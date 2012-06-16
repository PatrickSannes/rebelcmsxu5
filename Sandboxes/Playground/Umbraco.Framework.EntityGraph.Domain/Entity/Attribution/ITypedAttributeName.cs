using System;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution
{
    /// <summary>
    ///   Represents the name of an <see cref = "ITypedAttribute" />
    /// </summary>
    public interface ITypedAttributeName : IReferenceByAlias, IComparable<ITypedAttributeName>
    {
    }
}