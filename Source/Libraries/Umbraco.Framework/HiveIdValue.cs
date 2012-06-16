using System;
using System.ComponentModel;
using System.Linq;

namespace Umbraco.Framework
{
    public enum HiveIdValueTypes
    {
        String,
        Int32,
        Guid,
        Uri
    }

    /// <summary>
    /// A structure representing a value that can be a string, an integer or a <see cref="Guid"/>.
    /// </summary>
    /// <remarks></remarks>
    [TypeConverter(typeof(HiveIdValueTypeConverter))]
    public struct HiveIdValue : IEquatable<HiveIdValue>
    {
        private HiveIdValueTypes _type;
        private object _value;
        private bool? _isSystem;

        /// <summary>
        /// Gets or sets the type of the id value.
        /// </summary>
        /// <value>The type.</value>
        /// <remarks></remarks>
        public HiveIdValueTypes Type
        {
            get { return _type; }
            private set { _type = value; }
        }

        /// <summary>
        /// Gets or sets the original id value.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks></remarks>
        public object Value
        {
            get { return _value; }
            private set { _value = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a system value.
        /// </summary>
        /// <value><c>true</c> if this instance is system; otherwise, <c>false</c>.</value>
        /// <remarks></remarks>
        public bool IsSystem
        {
            get
            {
                if (_isSystem.HasValue) return _isSystem.Value;

                //now we need to check if it is a system one since its been created based on the full string
                if (Type != HiveIdValueTypes.Guid)
                    _isSystem = false;
                else
                {
                    var guidAsString = ToString();
                    if (!guidAsString.StartsWith("1") || guidAsString.ToCharArray().Any(char.IsLetter))
                        _isSystem = false;
                    else
                    {
                        //if there's no letters and it started with a '1' then its a system guid
                        //TODO: This isn't quite a 'perfect' check!
                        _isSystem = true;
                    }
                }

                return _isSystem.Value;
            }
            private set { _isSystem = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiveIdValue"/> struct from a string value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
        public HiveIdValue(string value)
        {
            _type = HiveIdValueTypes.String;
            _value = value;
            _isSystem = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiveIdValue"/> struct from a string value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
        public HiveIdValue(Uri value)
        {
            _type = HiveIdValueTypes.Uri;
            _value = value;
            _isSystem = false;
        }

        /// <summary>
        /// Tries to create a <see cref="HiveIdValue"/> given the provided value and type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static AttemptTuple<HiveIdValue> TryCreate(object value, HiveIdValueTypes type)
        {
            var potentialValue = CheckValueMatchesType(value, type);
            Mandate.ParameterCondition(potentialValue.Success, "type");

            switch (type)
            {
                case HiveIdValueTypes.Uri:
                    return new AttemptTuple<HiveIdValue>(true, new HiveIdValue((Uri) potentialValue.Result));
                case HiveIdValueTypes.Guid:
                    return new AttemptTuple<HiveIdValue>(true, new HiveIdValue((Guid) potentialValue.Result));
                case HiveIdValueTypes.Int32:
                    return new AttemptTuple<HiveIdValue>(true, new HiveIdValue((int) potentialValue.Result));
                case HiveIdValueTypes.String:
                    return new AttemptTuple<HiveIdValue>(true, new HiveIdValue((string)potentialValue.Result));
            }

            return AttemptTuple<HiveIdValue>.False;
        }

        private static AttemptTuple<object> CheckValueMatchesType(object value, HiveIdValueTypes type)
        {
            Mandate.ParameterNotNull(value, "value");

            switch (type)
            {
                case HiveIdValueTypes.Uri:
                    Uri uri;
                    return new AttemptTuple<object>(Uri.TryCreate(HiveId.HiveEntityUriDecode(value.ToString()), UriKind.RelativeOrAbsolute, out uri), uri);
                case HiveIdValueTypes.Guid:
                    Guid ignoredResult;
                    return new AttemptTuple<object>(Guid.TryParse(value.ToString(), out ignoredResult), ignoredResult);
                case HiveIdValueTypes.Int32:
                    Int32 ignoredInt;
                    return new AttemptTuple<object>(Int32.TryParse(value.ToString(), out ignoredInt), ignoredInt);
                default:
                    return new AttemptTuple<object>(true, value.ToString());
            }
        }

        public static HiveIdValue ConvertIntToGuid(int value)
        {
            Mandate.ParameterCondition(value != 0, "id");

            string template = Guid.Empty.ToString("N");
            bool isSystem = false;
            if (value < 0)
            {
                // Reset it 
                value = value * -1;
                // Add a prefix to the generated Guid
                template = 1 + template.Substring(1);

                //because it is negative we will deem it a system Uri
                isSystem = true;
            }
            string number = value.ToString();
            string guid = template.Substring(0, template.Length - number.Length) + number;

            return new HiveIdValue(Guid.Parse(guid)) {IsSystem = isSystem};
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiveIdValue"/> struct from an integer value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
        public HiveIdValue(int value)
        {
            _type = HiveIdValueTypes.Int32;
            _value = value;
            _isSystem = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiveIdValue"/> struct from a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks></remarks>
        public HiveIdValue(Guid value)
        {
            _type = HiveIdValueTypes.Guid;
            _value = value;
            _isSystem = null;
        }

        /// <summary>
        /// An empty <see cref="HiveIdValue"/>
        /// </summary>
        public static readonly HiveIdValue Empty = new HiveIdValue();

        /// <summary>
        /// Performs an explicit conversion from <see cref="Umbraco.Framework.HiveIdValue"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks></remarks>
        public static explicit operator string(HiveIdValue other)
        {
            Mandate.ParameterCondition(other != HiveIdValue.Empty, "other");
            Mandate.ParameterCondition(other.Type == HiveIdValueTypes.String, "other");

            return (string) other.Value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Umbraco.Framework.HiveIdValue"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks></remarks>
        public static explicit operator int(HiveIdValue other)
        {
            Mandate.ParameterCondition(other != HiveIdValue.Empty, "other");
            Mandate.ParameterCondition(other.Type == HiveIdValueTypes.Int32, "other");

            return (int)other.Value;
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Umbraco.Framework.HiveIdValue"/> to <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks></remarks>
        public static explicit operator Guid(HiveIdValue other)
        {
            if (other == HiveIdValue.Empty) return Guid.Empty;
            Mandate.That(other.Type == HiveIdValueTypes.Guid, x => new ArgumentException("Parameter 'other' must be of type Guid to convert to a Guid CLR type, but it is '" + other.Type.ToString() + "', with value: " + other.Value));
            return (Guid)other.Value;
        }

        /// <summary>
        /// Implements the operator == between <see cref="Guid"/> and <see cref="HiveIdValue"/>.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks></remarks>
        public static bool operator ==(Guid left, HiveIdValue right)
        {
            if (right.Type != HiveIdValueTypes.Guid)
                return false;
            var guid = (Guid) right.Value;
            return left == guid;
        }

        /// <summary>
        /// Implements the operator != between <see cref="Guid"/> and <see cref="HiveIdValue"/>.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        /// <remarks></remarks>
        public static bool operator !=(Guid left, HiveIdValue right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
        /// <remarks></remarks>
        public bool Equals(HiveIdValue other)
        {
            if (this.Type != other.Type) return false;
            switch (this.Type)
            {
                case HiveIdValueTypes.Uri:
                    return ((Uri) Value).Equals((Uri)other.Value);
                case HiveIdValueTypes.String:
                    // Because null HiveIdValues are represented by string|(null) when serialized
                    // we check here for both values either being null or "(null)"
                    return ((string) Value ?? "(null)").InvariantEquals((string)other.Value ?? "(null)");
                case HiveIdValueTypes.Int32:
                    return ((int) Value) == (int) other.Value;
                case HiveIdValueTypes.Guid:
                    return (Guid) Value == (Guid) other.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Value == null ? 1 : Value.GetHashCode();                
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is HiveIdValue))
                return false;
            return Equals((HiveIdValue) obj);
        }

        public static bool operator ==(HiveIdValue left, HiveIdValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HiveIdValue left, HiveIdValue right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            if (Value == null) return "(null)";
            switch (Type)
            {
                case HiveIdValueTypes.Guid:
                    return ((Guid) Value).ToString("N");
            }
            return Value.ToString();
        } 
    }
}