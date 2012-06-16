using System;

namespace Umbraco.Framework
{
    /// <summary>
    /// Permits an object to be referenced by its ordinal
    /// </summary>
    public interface IReferenceByOrdinal : IComparable<int>
    {
        /// <summary>
        /// Gets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        int Ordinal { get; set; }
    }
}