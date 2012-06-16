using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Umbraco.Framework.DataManagement;

namespace Umbraco.Framework
{
    /// <summary>A universal identifier for entities. </summary>
    /// <remarks>Doc updated, 14-Jan-2011.</remarks>
    [TypeConverter(typeof (HiveEntityUriTypeConverter))]
    [DebuggerDisplay("{ToFriendlyString()}")]
    public class
        HiveEntityUri : IEquatable<HiveEntityUri>, IEquatable<Guid>, IEquatable<int>
    {
        private const string HiveRoot = HiveScheme + "root/";
        private const string HiveRootTemplate = HiveRoot + "{0}/";

        ///<summary>
        /// The schema for the URI
        ///</summary>
        public const string HiveScheme = "hive://";

        public const string HtmlIdPrefix = "u_";

        private Uri _uri;

        /// <summary>
        /// This is only ever true if ConvertIntToGuid is called with a negative number.
        /// </summary>
        private bool? _isSystem = null;

        #region Constructors

        ///<summary>
        /// Constructor
        ///</summary>
        public HiveEntityUri()
            : this(HiveRoot, true)
        {
        }

        /// <summary>Creates an entity Uri from a <see cref="Guid"/></summary>
        /// <param name="guid">Base <see cref="Guid"/>.</param>
        public HiveEntityUri(Guid guid)
            : this(FormatGuid(guid), true)
        {
            SerializationType = DataSerializationTypes.Guid;
        }

        /// <summary>Creates an entity Uri from a <see cref="Int32"/></summary>
        /// <param name="id">Base <see cref="Int32"/>.</param>
        public HiveEntityUri(Int32 id)
            : this(FormatInt(id), true)
        {
            SerializationType = DataSerializationTypes.LargeInt;
        }

        /// <summary>
        /// Creates an entity from a generic string identitfier
        /// </summary>
        /// <param name="identifier"></param>
        /// <remarks>
        /// The usage of this constructor is for things such as file paths
        /// </remarks>
        public HiveEntityUri(string identifier)
        {
            ResetByParsing(identifier);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiveEntityUri"/> class.
        /// </summary>
        /// <param name="schemeName">Name of the scheme.</param>
        /// <param name="providerKey">The provider key.</param>
        /// <param name="id">The id.</param>
        public HiveEntityUri(string schemeName, string providerKey, int id)
        {
            SerializationType = DataSerializationTypes.LargeInt;
            var builder = new UriBuilder(schemeName, providerKey, -1, id.ToString());
            Reset(builder.ToString(), false);
        }

        public HiveEntityUri(string schemeName, string providerKey, string id)
        {
            SerializationType = DataSerializationTypes.String;
            var builder = new UriBuilder(schemeName, providerKey, -1, id);
            Reset(builder.ToString(), false);
        }

        public HiveEntityUri(string schemeName, string providerKey, Guid id)
        {
            SerializationType = DataSerializationTypes.Guid;
            var builder = new UriBuilder(schemeName, providerKey, -1, id.ToString("N"));
            Reset(builder.ToString(), false);
        }

        /// <summary>
        /// Creates an entity from a string identitfier
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="isUri">if the identifier is already a URI</param>
        /// <remarks>
        /// The usage of this constructor is for things such as file paths
        /// </remarks>
        private HiveEntityUri(string identifier, bool isUri)
        {
            SerializationType = DataSerializationTypes.String;
            Reset(isUri ? identifier : FormatString(identifier));
        }

        #endregion

        /// <summary>Gets or sets the serialization type of this Uri's value.</summary>
        /// <value>The type of the serialization.</value>
        public DataSerializationTypes SerializationType { get; set; }

        /// <summary>Gets the parts of this Uri as an <see cref="IEnumerable{T}"/> of type <see cref="string"/>.</summary>
        /// <value>The string parts.</value>
        public IEnumerable<string> StringParts
        {
            get
            {
                //return _uri.Segments.ToList();
                string[] betterSegments = _uri.LocalPath.Split('/');
                return betterSegments.Select(segment => segment.Trim('/')).Where(x => !string.IsNullOrWhiteSpace(x));
            }
        }

        /// <summary>
        /// Gets the entity type represented by this <see cref="HiveEntityUri"/>.
        /// </summary>
        /// <remarks></remarks>
        public string HiveEntityType
        {
            get { return _uri.Scheme; }
        }

        /// <summary>
        /// Gets the hive provider key which is ultimately responsible for the entity identified by this <see cref="HiveEntityUri"/>.
        /// </summary>
        /// <remarks></remarks>
        public string HiveOwnerProvider
        {
            get { return _uri.Host; }
            set { _uri = new UriBuilder(_uri.Scheme, value, -1, _uri.AbsolutePath).Uri; }
        }

        /// <summary>Gets the parts of this Uri as an <see cref="IEnumerable{Guid}"/>,
        /// 				 provided that the <seealso cref="SerializationType"/> is compatible.</summary>
        /// <value>The unique identifier parts.</value>
        public IEnumerable<Guid> GuidParts
        {
            get { return SerializationType == DataSerializationTypes.Guid ? GetGuidParts() : Enumerable.Empty<Guid>(); }
        }

        /// <summary>Gets the parts of this Uri as an <see cref="IEnumerable"/> of type <see cref="Int32"/>,
        /// 				 provided that the <seealso cref="SerializationType"/> is compatible.</summary>
        /// <value>The unique identifier parts.</value>
        public IEnumerable<Int32> IntParts
        {
            get
            {
                if (SerializationType == DataSerializationTypes.LargeInt || SerializationType == DataSerializationTypes.SmallInt)
                    return GetIntParts();
                return Enumerable.Empty<Int32>();
            }
        }


        /// <summary>Gets the value of this entity uri as a <see cref="Guid"/>.</summary>
        /// <value>Unique identifier of as.</value>
        public Guid AsGuid
        {
            get { return GuidParts.FirstOrDefault(); }
            //set { Reset(value); }
        }

        /// <summary>Gets the value of this entity uri as an <see cref="Int32"/>.</summary>
        /// <value>Unique identifier of as.</value>
        public int AsInt
        {
            get { return IntParts.FirstOrDefault(); }
            //set { Reset(value); }
        }

        /// <summary>
        /// Returns a string id that can be used for Html elements
        /// </summary>
        /// <returns></returns>
        public string GetHtmlId()
        {
            return HtmlIdPrefix + GetAllStringParts('-');
        }

        /// <summary>
        /// Concatenates all non-empty string parts together with the specified char as the delimiter
        /// </summary>
        /// <param name="delimiter">The delimiter (defaults to /).</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetAllStringParts(char delimiter = '/')
        {
            var builder = new StringBuilder();
            foreach (string s in StringParts)
            {
                builder.Append(s);
                builder.Append(delimiter);
            }
            string output = builder.ToString().Trim(delimiter);
            return output;
        }

        /// <summary>
        /// Outputs a string for use with display/presentation
        /// </summary>
        /// <returns></returns>
        public string ToFriendlyString()
        {
            //return SerializationType == DataSerializationTypes.String
            //           ? GetAllStringParts()
            //           : _uri.ToString();
            return _uri.ToString();
        }

        private IEnumerable<Guid> GetGuidParts()
        {
            foreach (string stringPart in StringParts)
            {
                Guid parseGuid = Guid.Empty;
                string trimmedPart = stringPart.Trim(new[] {'/'});
                if (!string.IsNullOrWhiteSpace(trimmedPart))
                {
                    if (Guid.TryParse(trimmedPart, out parseGuid)) 
                        yield return parseGuid;
                }
            }
        }

        private IEnumerable<Int32> GetIntParts()
        {
            foreach (string stringPart in StringParts)
            {
                string trimmedPart = stringPart.Trim(new[] {'/'});
                if (!string.IsNullOrWhiteSpace(trimmedPart))
                {
                    Int32 parsedInt;
                    if (Int32.TryParse(trimmedPart, out parsedInt)) 
                        yield return parsedInt;
                }
            }
        }

        private void ResetAsFormattedString(string baseUri = HiveRoot)
        {
            if (baseUri.StartsWith("/")) baseUri = "~" + baseUri;
            Reset(FormatString(baseUri.TrimStart('/')));
        }

        private void Reset(string baseUri = HiveRoot, bool detectSerialization = true)
        {
            if (baseUri.StartsWith("/")) 
                baseUri = "~" + baseUri;

            //set the uri
            _uri = new Uri(baseUri);

            if (detectSerialization)
            {
                // Note: the order of this is important; some Guids could parse as Ints, so check Guids first,

                // Some ids are also String serialized that contain Guids so just checking if it has Guids isn't 
                // enough to determine if it is in fact all guids, here's were ensuring it has guids and the string
                // parts match guids... then we assume it is a guid... unfortunately this isn't fail safe but
                // the chances of this causing erros is next to none since
                var guidPartCount = GetGuidParts().Count();
                if (guidPartCount > 0 && guidPartCount == StringParts.Count())
                {
                    //the guids must always be string formatted as "N"
                    var uriBuilder = new StringBuilder();
                    foreach (var g in GetGuidParts())
                    {
                        if (uriBuilder.Length == 0)
                        {
                            //need to append the first part of the original uri
                            var index = baseUri.IndexOf(g.ToString(), StringComparison.InvariantCultureIgnoreCase);
                            if (index == -1)
                                index = baseUri.IndexOf(g.ToString("N"), StringComparison.InvariantCultureIgnoreCase);
                            uriBuilder.Append(baseUri.Substring(0, index));
                        }
                        uriBuilder.Append(g.ToString("N"));
                        uriBuilder.Append("/");
                    }
                    baseUri = uriBuilder.ToString().TrimEnd('/');
                    SerializationType = DataSerializationTypes.Guid;
                    //reset the uri
                    _uri = new Uri(baseUri);
                }
                else if (GetIntParts().Any())
                {
                    SerializationType = DataSerializationTypes.LargeInt;
                }
            }
            
        }

        /// <summary>
        /// Returns the string representation of the HiveEntityUri for use in URLs
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This checks what type of SerializationType is being used, for most cases (GUIDs and Ints) the output will be formatted
        /// as a 'readable' string, for string types, the output will be formatted as a safely casted (for use in URLs) Base64 encoded string.
        /// </remarks>
        public override string ToString()
        {
            if (SerializationType == DataSerializationTypes.String)
            {
                return _uri.ToString().ToUrlBase64();
            }
            return HiveEntityUriEncode(_uri.ToString());
        }

        /// <summary>
        /// Determines if the HiveEntityUri is a system Uri, this is determined if the ConvertIntToGuid is called with a nagative int.
        /// </summary>
        /// <returns></returns>
        public bool IsSystem()
        {
            if (_isSystem.HasValue) return _isSystem.Value;
            
            //now we need to check if it is a system one since its been created based on the full string
            if (SerializationType != DataSerializationTypes.Guid)
                _isSystem = false;
            else
            {
                var guidAsString = string.Join("", GetGuidParts().Select(x => x.ToString()));
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
        
        internal bool ResetByParsing(string value)
        {
            // At least set _uri etc.
            Reset();

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            Guid guidId;
            if (Guid.TryParse(value, out guidId))
            {
                Reset(FormatGuid(guidId));
                return true;
            }
            int intId;
            if (int.TryParse(value, out intId))
            {
                Reset(FormatInt(intId));
                return true;
            }
            if (value.Contains("://"))
            {
                Reset(value);
                return true;
            }
            if (value.Contains('/'))
            {
                ResetAsFormattedString(value);
                return true;
            }
            //we've got this far, this could mean that it's probably encoded
            string decoded = HiveEntityUriDecode(value);
            if (decoded.Contains("://"))
            {
                Reset(decoded);
                return true;
            }
            //check if this is a base64 encoded string
            string base64 = value.FromUrlBase64();
            if (!string.IsNullOrEmpty(base64))
            {
                if (base64.Contains("://"))
                {
                    Reset(base64);
                    return true;
                }
            }

            // Final option: reset as string anyway
            ResetAsFormattedString(value);
            return false;
        }

        #region Operators

        /// <summary>
        /// Performs an Implicit conversion from <see cref="System.Guid"/> to <see cref="HiveEntityUri"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator HiveEntityUri(Guid value)
        {
            return new HiveEntityUri(value);
        }

        /// <summary>
        /// Performs an Explicit conversion from <see cref="HiveEntityUri"/> to <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Guid(HiveEntityUri value)
        {
            if (value == null)
                return default(Guid);

            return value.AsGuid;
        }

        ///<summary>
        /// Explicit operator to cast from HiveEntityUri to int
        ///</summary>
        ///<param name="value"></param>
        ///<returns></returns>
        public static explicit operator int(HiveEntityUri value)
        {
            if (value == null)
                return default(int);

            return value.AsInt;
        }

        /// <summary>
        /// Explicit operator to cast from int to HiveEntityUri
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static explicit operator HiveEntityUri(int value)
        {
            return new HiveEntityUri(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Umbraco.Framework.HiveEntityUri"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator string(HiveEntityUri value)
        {
            if (value == null) return string.Empty;
            return value.ToString();
        }

        ///<summary>
        /// Equality operator between HiveEntityUri's
        ///</summary>
        ///<param name="left"></param>
        ///<param name="right"></param>
        ///<returns></returns>
        public static bool operator ==(HiveEntityUri left, HiveEntityUri right)
        {
            // If they are both the same reference or both null, return true
            if (ReferenceEquals(left, right))
                return true;

            // If only one is null (because we got to this point) return false
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            try
            {
                return left.Equals(right);
            }
            catch (Exception)
            {
                return false;
            }
        }

        ///<summary>
        /// Inequality operator between HiveEntityUri's
        ///</summary>
        ///<param name="left"></param>
        ///<param name="right"></param>
        ///<returns></returns>
        public static bool operator !=(HiveEntityUri left, HiveEntityUri right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Equality operator between HiveEntityUri and int
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(HiveEntityUri left, int right)
        {
            if (ReferenceEquals(left, null))
                return false;

            try
            {
                return left.Equals(right);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Inequality operator between HiveEntityUri and int
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(HiveEntityUri left, int right)
        {
            return !(left == right);
        }


        /// <summary>
        /// Equality operator between HiveEntityUri and Guid
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(HiveEntityUri left, Guid right)
        {
            if (ReferenceEquals(left, null))
                return false;

            try
            {
                return left.Equals(right);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Inequality operator between HiveEntityUri and Guid
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(HiveEntityUri left, Guid right)
        {
            return !(left == right);
        }

        #endregion

        #region Equals/GetHashCode

        #region IEquatable<Guid> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Guid other)
        {
            return AsGuid.Equals(other);
        }

        #endregion

        #region IEquatable<HiveEntityUri> Members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(HiveEntityUri other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // If one is using the HiveRoot, or both are the same hive owner provider, and the serialization is the same, then just compare the value
            if (((HiveOwnerProvider == "root" || other.HiveOwnerProvider == "root")
                || (HiveOwnerProvider == other.HiveOwnerProvider))
                && SerializationType == other.SerializationType)
            {
                return ValueEquals(other);
            }            
            
            //lastly, compare the underlying Uri
            if (_uri.Equals(other._uri))
                return true;

            return false;
        }

        #endregion

        #region IEquatable<int> Members

        public bool Equals(int other)
        {
            return AsInt == other;
        }

        #endregion

        private bool ValueEquals(HiveEntityUri other)
        {
            switch (SerializationType)
            {
                case DataSerializationTypes.Guid:
                    return Equals(other.AsGuid);
                case DataSerializationTypes.LargeInt:
                case DataSerializationTypes.SmallInt:
                    return Equals(other.AsInt);
                default:
                    return string.Equals(GetAllStringParts(), other.GetAllStringParts(), StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public override bool Equals(object obj)
        {
            //NOTE: we were going to make Empty equal to null but this causes some issues
            //if its 'Empty' make it equivalent to null
            //if (ReferenceEquals(null, obj) && this == Empty) return true;

            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(obj as HiveEntityUri, null)) return false;
            
            return Equals((HiveEntityUri) obj);
        }

        /// <summary>
        /// Checks if the string is the same, if it is not, it checks if the UrlDecoded or Encoded version of the string is the same
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(string other)
        {
            if (string.IsNullOrEmpty(other)) return false;
            var otherCast = new HiveEntityUri(other);
            return Equals(otherCast);
            //return _uri.ToString().Equals(other) || HiveEntityUriDecode(_uri.ToString()).Equals(other) || HiveEntityUriEncode(_uri.ToString()).Equals(other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Generated by Resharper, though it seems like its good</remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                return (SerializationType.GetHashCode()*397) ^ (_uri != null ? _uri.GetHashCode() : 0);
            }
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Compares 2 strings that are hive ids and checks if they are equal
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        public static bool CompareString(string id1, string id2)
        {
            HiveEntityUri hiveId1;
            HiveEntityUri hiveId2;
            if (!TryParse(id1, out hiveId1))
            {
                return false;
            }
            if (!TryParse(id2, out hiveId2))
            {
                return false;
            }
            return hiveId1 == hiveId2;
        }

        /// <summary>
        /// Checks if the uri is null or 'Empty'
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(HiveEntityUri id)
        {
            if (id == null || id == Empty)
                return true;
            return false;
        }

        /// <summary>Creates an empty entity Uri based on a blank <see cref="Guid"/>.</summary>
        /// <value>The empty.</value>
        public static HiveEntityUri Empty
        {
            get { return Guid.Empty; }
        }

        /// <summary>
        /// Attempts to parse the value into a HiveEntityUri, this will check for types
        /// </summary>
        /// <param name="value"></param>
        /// <param name="uri">The HiveEntityUri that will be output if parsing is successful</param>
        /// <returns></returns>
        public static bool TryParse(string value, out HiveEntityUri uri)
        {
            try
            {
                // We use InternalStaticParse here to reduce the reliance on an exception
                uri = InternalStaticParse(value);
                return uri != null;
            }
            catch (FormatException)
            {
                uri = null;
                return false;
            }
        }

        /// <summary>
        /// Parses the string into a HiveEntityUri, if parsing fails and exception is thrown
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static HiveEntityUri Parse(string value)
        {
            Mandate.ParameterNotNull(value, "value");

            HiveEntityUri parsed = InternalStaticParse(value);
            if (parsed != null) return parsed;

            //if we've reached this point, we cannot parse this string into a HiveEntityUri    
            throw new FormatException("Could not parse the string value '{0}' into a HiveEntityUri".InvariantFormat(value));
        }

        /// <summary>
        /// Parses a string into a HiveEntityUri, returning null if parsing fails
        /// </summary>
        /// <param name="value">The value.</param>
        internal static HiveEntityUri InternalStaticParse(string value)
        {
            var output = new HiveEntityUri();
            return output.ResetByParsing(value) ? output : null;
        }

        /// <summary>Creates an entity Uri from a <see cref="string"/> base</summary>
        /// <param name="baseUri">Base Uri.</param>
        public static HiveEntityUri FromUriString(string baseUri)
        {
            var id = new HiveEntityUri();
            id.Reset(baseUri);
            return id;
        }

        /// <summary>
        /// Decodes the string to a Uri string
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns>
        /// </returns>
        private static string HiveEntityUriDecode(string encoded)
        {
            string decoded = encoded.Replace("$$", "://").Replace('$', '/');
            if (!decoded.EndsWith("/")) decoded += '/';
            return decoded;

            //Using the method based on the last comment... bse64 can still cause issues with the slash
            //http://stackoverflow.com/questions/591694/url-encoded-slash-in-url
            //return HttpUtility.UrlDecode(encoded.Replace('!', '%'));            
        }

        /// <summary>
        /// Encodes the uri string to be used inside of Urls
        /// </summary>
        /// <param name="decoded"></param>
        /// <returns></returns>
        private static string HiveEntityUriEncode(string decoded)
        {
            return decoded.Replace("://", "$$").Replace('/', '$').TrimEnd('$');

            //Using the method based on the last comment... bse64 can still cause issues with the slash
            //http://stackoverflow.com/questions/591694/url-encoded-slash-in-url
            //var encoded = HttpUtility.UrlEncode(decoded);
            //return encoded.Replace('%', '!');
        }

        ///<summary>
        ///</summary>
        ///<param name="id"></param>
        ///<returns></returns>
        public static HiveEntityUri ConvertIntToGuid(int id)
        {
            Mandate.ParameterCondition(id != 0, "id");

            string template = Guid.Empty.ToString("N");
            bool isSystem = false;
            if (id < 0)
            {
                // Reset it 
                id = id * -1;
                // Add a prefix to the generated Guid
                template = 1 + template.Substring(1);

                //because it is negative we will deem it a system Uri
                isSystem = true;
            }
            string number = id.ToString();
            string guid = template.Substring(0, template.Length - number.Length) + number;

            var hId = new HiveEntityUri(Guid.Parse(guid)) {_isSystem = isSystem};

            return hId;
        }

        private static string FormatGuid(Guid guid)
        {
            return string.Format(HiveRootTemplate, guid.ToString("N"));
        }

        private static string FormatInt(Int32 id)
        {
            return string.Format(HiveRootTemplate, id);
        }

        private static string FormatString(string id)
        {
            return string.Format(HiveRootTemplate, id);
        }

        #endregion
    }
}