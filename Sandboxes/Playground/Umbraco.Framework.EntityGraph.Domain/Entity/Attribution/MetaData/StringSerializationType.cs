using System;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.Versioning;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData
{
    public class StringSerializationType : IAttributeSerializationDefinition
    {
        #region Implementation of IReferenceByAlias

        /// <summary>
        /// Gets or sets the alias of the object. The alias is a string to which this object
        /// can be referred programmatically, and is often a normalised version of the <see cref="IReferenceByAlias.Name"/> property.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the object. The name is a string intended to be human-readable, and
        /// is often a more descriptive version of the <see cref="IReferenceByAlias.Alias"/> property.
        /// </summary>
        /// <value>The name.</value>
        public LocalizedString Name { get; set; }

        #endregion

        #region Implementation of IAttributeSerializationDefinition<LocalizedString>

        /// <summary>
        ///   Gets or sets the type of the data serialization.
        /// </summary>
        /// <value>The type of the data serialization.</value>
        public DataSerializationTypes DataSerializationType { get; set; }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <remarks>
        /// The reason this has an <code>out</code> parameter rather than a returntype of TAllowedType is that TAllowedType would require a parameterless constructor,
        /// making it impossible to call this method having passed <see cref="System.String"/> and other primitive types as TAllowedType.
        /// </remarks>
        /// <param name="value">The value.</param>
        /// <param name="serializeTo">The object to serialize to.</param>
        public dynamic Serialize(ITypedAttribute value)
        {
            return (dynamic)value.Value;
        }

        #endregion

        #region Implementation of ITracksConcurrency

        /// <summary>
        ///   Gets or sets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        public IConcurrencyToken ConcurrencyToken { get; set; }

        #endregion

        #region Implementation of IEntity

        /// <summary>
        ///   Gets or sets the status of the entity.
        /// </summary>
        /// <value>The status.</value>
        public IEntityStatus Status { get; set; }

        /// <summary>
        ///   Gets or sets the creation date of the entity (UTC).
        /// </summary>
        /// <value>The UTC created.</value>
        public DateTime UtcCreated { get; set; }

        /// <summary>
        ///   Gets or sets the modification date of the entity (UTC).
        /// </summary>
        /// <value>The UTC modified.</value>
        public DateTime UtcModified { get; set; }

        /// <summary>
        ///   Gets or sets the timestamp when the status of this entity was last changed (in UTC).
        /// </summary>
        /// <value>The UTC status changed.</value>
        public DateTime UtcStatusChanged { get; set; }

        /// <summary>
        ///   Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public IMappedIdentifier Id { get; set; }

        /// <summary>
        ///   Gets or sets the revision data.
        /// </summary>
        /// <value>The revision.</value>
        public IRevisionData Revision { get; set; }

        #endregion
    }
}