using System;
using Umbraco.Foundation;

namespace Umbraco.Framework.Persistence.Abstractions.Attribution
{
    /// <summary>
    ///   Represents the name of an <see cref = "ITypedAttribute" />
    /// </summary>
    public interface ITypedAttributeName : IReferenceByAlias, IComparable<ITypedAttributeName>
    {
    }
}