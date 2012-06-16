using System;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.DataManagement;

namespace Umbraco.Framework
{
	[Obsolete]
    public class GuidIdentifier : IMappedIdentifier
    {
        private DataSerializationTypes _serializationType = DataSerializationTypes.String;
        private Guid _value;

        #region Implementation of IMappedIdentifier

        public string ValueAsString
        {
            get { return string.Concat(MappingKey, ":", Value.ToString()); }
        }

        public dynamic Value
        {
            get { return _value; }
            set { _value = (Guid) value; }
        }

        public string MappingKey
        {
            get { return "UMB"; }
        }

        public IProviderManifest MappedProvider { get;  set; }

        public DataSerializationTypes DataSerializationType
        {
            get { return _serializationType; }
            set { _serializationType = value; }
        }

        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is string) return Equals((string)obj);
            if (obj is IMappedIdentifier) return Equals((IMappedIdentifier)obj);
            return Equals(obj.ToString());
        }

        #region Implementation of IEquatable<IMappedIdentifier>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public virtual bool Equals(IMappedIdentifier other)
        {
            if (other == null ) return false;
            if (other is string) return Equals(other.ToString());
            return Equals(other.ValueAsString);
        }

        #endregion

        #region Implementation of IEquatable<string>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public virtual bool Equals(string other)
        {
            if (other == null) return false;
            return string.Equals(other, ValueAsString);
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ValueAsString;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GuidIdentifier left, GuidIdentifier right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GuidIdentifier left, GuidIdentifier right)
        {
            return !Equals(left, right);
        }
    }
}