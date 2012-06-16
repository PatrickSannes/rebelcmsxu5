using System;
using System.ComponentModel;

namespace Umbraco.Framework.Persistence.Model
{
    using Umbraco.Framework.Data;

    public interface IEntity : ITracksConcurrency, IReferenceByHiveId
    {
        /// <summary>
        ///   Gets or sets the creation date of the entity (UTC).
        /// </summary>
        /// <value>The UTC created.</value>
        DateTimeOffset UtcCreated { get; set; }

        /// <summary>
        ///   Gets or sets the modification date of the entity (UTC).
        /// </summary>
        /// <value>The UTC modified.</value>
        DateTimeOffset UtcModified { get; set; }

        /// <summary>
        ///   Gets or sets the timestamp when the status of this entity was last changed (in UTC).
        /// </summary>
        /// <value>The UTC status changed.</value>
        DateTimeOffset UtcStatusChanged { get; set; }
    }
}