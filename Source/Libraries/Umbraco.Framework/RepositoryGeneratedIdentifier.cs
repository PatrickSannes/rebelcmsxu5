using System;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.DataManagement;

namespace Umbraco.Framework
{
    /// <summary>
    /// An identifier for use by display-tier DTOs for transporting a request back to the repository to generate an Id at source
    /// </summary>
    [Obsolete]
    public class RepositoryGeneratedIdentifier : Framework.GuidIdentifier, IMappedIdentifier, IEquatable<Guid>
    {
        protected internal RepositoryGeneratedIdentifier()
        {}

        /// <summary>
        /// Creates a standardised instance recognised by repositories as an instruction to create an Id at the datasource.
        /// </summary>
        /// <returns></returns>
        public static RepositoryGeneratedIdentifier Create()
        {
            return new RepositoryGeneratedIdentifier
                       {
                           DataSerializationType = DataSerializationTypes.Guid,
                           Value = Guid.Empty
                       };
        }

        /// <summary>
        /// Gets the value as a GUID.
        /// </summary>
        /// <value>The value as GUID.</value>
        protected Guid ValueAsGuid
        {
            get
            {
                if (Value != null) return (Guid)Value;
                return Guid.Empty;
            }
        }

        #region Implementation of IEquatable<Guid>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Guid other)
        {
            return ValueAsGuid.Equals(other);
        }

        #endregion
    }
}