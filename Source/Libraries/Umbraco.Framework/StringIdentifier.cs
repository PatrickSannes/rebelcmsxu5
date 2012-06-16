using System;

using Umbraco.Foundation;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Resources;

namespace Umbraco.Framework
{
	[Obsolete]
    public struct StringIdentifier : IMappedIdentifier, IComparable<StringIdentifier>, IEquatable<StringIdentifier>, IComparable<string>
    {
        public StringIdentifier(object value, string providerKey)
        {
            _value = value;
            _entityProviderKey = providerKey;
            _mappedProvider = null;
        }

        #region Implementation of IMappedIdentifier

        private readonly string _entityProviderKey;
        private IProviderManifest _mappedProvider;
        private object _value;

        /// <summary>
        /// Gets or sets the value as string.
        /// </summary>
        /// <value>The value as string.</value>
        public string ValueAsString
        {
            get { return GetFormattedValue(Value, MappingKey); }
        }


        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Gets the entity provider key.
        /// </summary>
        /// <value>The entity provider key.</value>
        public string MappingKey
        {
            get { return _entityProviderKey; }
        }

        public IProviderManifest MappedProvider
        {
            get { return _mappedProvider; }
            set { _mappedProvider = value; }
        }

        /// <summary>
        /// Gets or sets the type of data serialization.
        /// </summary>
        /// <value>The type of the serialization.</value>
        public DataSerializationTypes DataSerializationType
        {
            get { return DataSerializationTypes.String; }
        }

	    /// <summary>
	    /// Created a new EntityIdenifier from a string instance
	    /// </summary>
	    /// <param name="identifier">The identifier.</param>
	    /// <example>
	    /// EntityIdentifier id = EntityIdentifier.FromString("{MyProviderKey}1234");
	    /// </example>
	    /// <returns></returns>
	    /// <exception cref="System.ArgumentException">Thrown when the identifier does not match the expected format</exception>
	    public static StringIdentifier FromString(string identifier)
        {
            Mandate.ParameterNotNullOrEmpty(identifier, "identifier");

            identifier = identifier.Trim('{');
            string[] keys = identifier.Split(new[] {'}'}, 2);
            bool isValid = Validate.Array(keys, 2, 2);
            if (!isValid)
            {
                string message =
                    Validation.UmbracoFramework_EntityGraph_EntityIdentifier_FromString__ParameterFormat;
                message = string.Format(message, identifier, GetFormattedValue(12345, "EntityProviderName"));
                throw new ArgumentException(message, "identifier");
            }

            return new StringIdentifier(keys[1], keys[0]);
        }

        /// <summary>
        /// Gets the formatted value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="entityProviderKey">The entity provider key.</param>
        /// <returns></returns>
        private static string GetFormattedValue(object value, string entityProviderKey)
        {
            return string.Format("{{{0}}}{1}", entityProviderKey, value);
        }

        #endregion

        #region IComparable<StringIdentifier> Members

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(StringIdentifier other)
        {
            //TODO: Consider the type of the actual ID not just a string compare
            //so that this can be used for sorting by ID
            return string.Compare(ValueAsString, other.ValueAsString, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

        #region IEquatable<StringIdentifier> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(StringIdentifier other)
        {
            return Equals(other._value, _value) && Equals(other._entityProviderKey, _entityProviderKey);
        }

        #endregion

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(IMappedIdentifier other)
        {
            if (other == null) return false;
            if (other is string) return Equals(other.ValueAsString);
            return Equals(other.ValueAsString);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(string other)
        {
            if (other == null) return false;
            return string.Equals(other, ValueAsString);
        }

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
        /// Performs an explicit conversion from <see cref="System.String"/> to <see cref="StringIdentifier"/>.
        /// </summary>
        /// <param name="identifier">The string implementation of the EntityIdentifier.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator StringIdentifier(string identifier)
        {
            Mandate.ParameterNotNullOrEmpty(identifier, "identifier");

            return FromString(identifier);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(StringIdentifier left, StringIdentifier right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(StringIdentifier left, StringIdentifier right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (StringIdentifier)) return false;
            return Equals((StringIdentifier) obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_value != null ? _value.GetHashCode() : 0)*397) ^
                       (_entityProviderKey != null ? _entityProviderKey.GetHashCode() : 0);
            }
        }

        #region IComparable<string> Members

        public int CompareTo(string other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}