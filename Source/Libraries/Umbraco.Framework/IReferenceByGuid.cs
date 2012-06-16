using System;

namespace Umbraco.Framework
{
    /// <summary>
    /// Stipulates that implementing types must have an <see cref="Id"/> property of type <see cref="Guid"/>.
    /// </summary>
    public interface IReferenceByGuid
    {
        Guid Id { get; set; }
    }
}
