using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Umbraco.Framework
{
    [TypeConverter(typeof(HiveIdTypeConverter))]
    public struct HiveId : IEquatable<HiveId>
    {
        private const string UriSafeDelimiter = "$_";
        private const string UriDelimiter = "/";
        private const string ProviderPrefix = "p__";
        private const string ValuePrefix = "v__";

        public static readonly HiveId Empty = new HiveId(HiveIdValue.Empty);

        public HiveId(string value)
            : this(value, false)
        {

        }

        internal HiveId(string value, bool alreadyFormatted)
            : this(alreadyFormatted && value != null ? new HiveIdValue(value) : HiveIdValue.Empty)
        {
            if (alreadyFormatted) return;
            var possibleValue = TryParse(value);
            if (possibleValue.Success)
            {
                this._value = possibleValue.Result._value;
                this._providerGroupRoot = possibleValue.Result._providerGroupRoot;
                this._providerId = possibleValue.Result._providerId;
            }
            else _value = new HiveIdValue(value);
        }

        public HiveId(int value)
            : this(new HiveIdValue(value))
        {

        }

        public HiveId(Uri value)
            : this(value != null ? new HiveIdValue(value) : HiveIdValue.Empty)
        {

        }

        public HiveId(Guid value)
            : this(new HiveIdValue(value))
        {

        }

        public HiveId(HiveIdValue value)
        {
            _value = value;
            _providerGroupRoot = null;
            _providerId = null;
        }

        public HiveId(string providerGroupSchemeOnly, string providerId, HiveIdValue value)
            : this(new Uri(providerGroupSchemeOnly + "://"), providerId, value)
        {
        }

        public HiveId(Uri providerGroupRoot, string providerId, HiveIdValue value)
            : this(value)
        {
            ProviderGroupRoot = providerGroupRoot;
            ProviderId = providerId;
        }

        public static bool operator ==(HiveId left, HiveId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HiveId left, HiveId right)
        {
            return !(left == right);
        }

        public bool IsSystem()
        {
            return Value.IsSystem;
        }

        private Uri _providerGroupRoot;
        public Uri ProviderGroupRoot
        {
            get { return _providerGroupRoot; }
            private set { _providerGroupRoot = value; }
        }

        private string _providerId;
        public string ProviderId
        {
            get { return string.IsNullOrEmpty(_providerId) ? null : _providerId; }
            private set { _providerId = value; }
        }

        private HiveIdValue _value;
        public HiveIdValue Value
        {
            get { return _value; }
            private set { _value = value; }
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="System.Guid"/> to <see cref="Umbraco.Framework.HiveId"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks></remarks>
        public static explicit operator HiveId(Guid value)
        {
            return new HiveId(value);
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="System.Int32"/> to <see cref="Umbraco.Framework.HiveId"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks></remarks>
        public static explicit operator HiveId(int value)
        {
            return new HiveId(value);
        }

        private static string FormatValueFromUri(Uri subValue)
        {
            return HiveEntityUriEncode(subValue.ToString());
        }

        private static string FormatProviderId(string providerId)
        {
            return string.IsNullOrEmpty(providerId) ? string.Empty : ProviderPrefix + providerId.TrimStart(ProviderPrefix);
        }

        private static string FormatValue(string valueAsString)
        {
            return string.IsNullOrEmpty(valueAsString) ? string.Empty : ValuePrefix + valueAsString.TrimStart(ValuePrefix);
        }

        public Uri ToUri()
        {
            string formattedSubvalue = string.Empty;
            switch (Value.Type)
            {
                case HiveIdValueTypes.Uri:
                    formattedSubvalue = Value.Value != null ? FormatValueFromUri((Uri)Value.Value) : string.Empty;
                    break;
                default:
                    formattedSubvalue = HiveEntityUriEncode(Value.ToString());
                    break;
            }


            if (ProviderGroupRoot == null)
            {
                // Need to return a relative Uri
                // Firstly, if the provider id is null, then we don't need to use the ValuePrefix
                var formattedProviderId = FormatProviderId(ProviderId);
                var valueAsUri = "{0}/{1}".InvariantFormat(Value.Type.ToString().ToLower(), formattedSubvalue);

                var outputAsUri = string.IsNullOrEmpty(formattedProviderId) ? "/" + valueAsUri : formattedProviderId + UriDelimiter + FormatValue(valueAsUri);
                return new Uri(outputAsUri, UriKind.Relative);
            }

            // NOTE System.Uri has a bug where creating with scheme:// causes it to store scheme:/// (three slashes)
            // even when later combined with relative strings or relative Uris
            // So we first create the Uri, replace :/// with :// and return a new Uri from that
            // (nasty)
            var absolutePath = "/{0}/{1}/{2}".InvariantFormat(FormatProviderId(FormatForUriPart(ProviderId)),
                                                                 FormatValue(FormatForUriPart(Value.Type.ToString().ToLower())),
                                                                 FormatForUriPart(formattedSubvalue));
            var forFormatting = new Uri(ProviderGroupRoot, absolutePath.TrimStart("//"));

            return new Uri(forFormatting.ToString().Replace(":///", "://"), UriKind.Absolute);
        }

        private static string FormatForUriPart(string part)
        {
            return string.IsNullOrEmpty(part) ? string.Empty : part.Trim('/').Trim('\\').Trim();
        }

        public static HiveId ConvertIntToGuid(int value)
        {
            return new HiveId(HiveIdValue.ConvertIntToGuid(value));
        }

        public static HiveId ConvertIntToGuid(string providerGroupSchemeOnly, string providerId, int value)
        {
            return new HiveId(providerGroupSchemeOnly, providerId, HiveIdValue.ConvertIntToGuid(value));
        }

        public static HiveId ConvertIntToGuid(Uri providerGroupRoot, string providerId, int value)
        {
            return new HiveId(providerGroupRoot, providerId, HiveIdValue.ConvertIntToGuid(value));
        }

        private static string FormatForUriSafePart(string part)
        {
            if (string.IsNullOrEmpty(part)) return string.Empty;

            return HiveEntityUriEncode(FormatForUriPart(part).Trim(UriSafeDelimiter));
        }

        private static string FormatFromUriSafePart(string part)
        {
            if (string.IsNullOrEmpty(part)) return string.Empty;
            return HiveEntityUriDecode(part);
        }

        /// <summary>
        /// Decodes the string to a Uri string
        /// </summary>
        /// <param name="encoded"></param>
        /// <returns>
        /// </returns>
        internal static string HiveEntityUriDecode(string encoded)
        {
            string decoded = encoded
                .Replace("$empty_root$", ":///")
                .Replace("$net_root$", "://")
                .Replace("$uri_fileroot$", ":/")
                .Replace("$path_fileroot$", @":\")
                .Replace("$!", @"\")
                .Replace("$$", "/")
                .Replace(".c$html", ".cshtml")
                .Replace(".v$html", ".vbhtml");
            //if (!decoded.EndsWith("/")) decoded += '/';
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
        internal static string HiveEntityUriEncode(string decoded)
        {
            return decoded
                .Replace(":///", "$empty_root$")
                .Replace("://", "$net_root$")
                .Replace(":/", "$uri_fileroot$")
                .Replace(@":\", "$path_fileroot$")
                .Replace(@"\", "$!")
                .Replace("/", "$$")
                //need to encode .cshtml for templates since IIS won't serve that extension
                .Replace(".cshtml", ".c$html")
                .Replace(".vbhtml", ".v$html");

            //Using the method based on the last comment... bse64 can still cause issues with the slash
            //http://stackoverflow.com/questions/591694/url-encoded-slash-in-url
            //var encoded = HttpUtility.UrlEncode(decoded);
            //return encoded.Replace('%', '!');
        }

        public override bool Equals(object obj)
        {
            if (obj is Guid) return Equals((HiveId)(Guid)obj);
            if (!(obj is HiveId)) return false;
            return Equals((HiveId)obj);
        }

        public override int GetHashCode()
        {
            var providerGroupRoot = ProviderGroupRoot == null ? "(GroupIsNull)" : ProviderGroupRoot.ToString();
            var providerId = ProviderId ?? "(ProviderIdNull)";
            return (Value.ToString() + providerId + providerGroupRoot).GetHashCode();
        }

        public bool EqualsIgnoringProviderId(HiveId other)
        {
            return ProviderGroupRoot == other.ProviderGroupRoot
                && Value.Equals(other.Value);
        }

        public bool Equals(HiveId other)
        {
            return ProviderId == other.ProviderId
                   && EqualsIgnoringProviderId(other);
        }

        /// <summary>
        /// Outputs a string for use with display/presentation. A shortcut for <code>ToString(HiveIdFormatStyle.UriSafe)</code>.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public string ToFriendlyString()
        {
            return ToString(HiveIdFormatStyle.UriSafe);
        }

        public override string ToString()
        {
            return ToString(HiveIdFormatStyle.UriSafe);
        }

        public string ToString(HiveIdFormatStyle style)
        {
            switch (style)
            {
                default:
                case HiveIdFormatStyle.AsUri:
                    return ToUri().ToString();
                case HiveIdFormatStyle.UriSafe:
                    var valueType = Value.Type.ToString().ToLower();
                    var value = HiveEntityUriEncode(Value.ToString());

                    var valueTypeForOutput = ProviderGroupRoot == null && string.IsNullOrEmpty(ProviderId) ? valueType : FormatValue(valueType);
                    var outputBuilder = new StringBuilder();
                    if (ProviderGroupRoot != null)
                    {
                        // If it's just a root, don't trim it
                        var groupRoot = ProviderGroupRoot.ToString();
                        if (groupRoot.EndsWith("/") && !groupRoot.EndsWith("://") && !groupRoot.EndsWith(":///"))
                            groupRoot = groupRoot.TrimEnd('/');
                        outputBuilder.Append(HiveEntityUriEncode(groupRoot));
                        outputBuilder.Append(UriSafeDelimiter);
                    }
                    if (!string.IsNullOrEmpty(ProviderId))
                    {
                        outputBuilder.Append(FormatForUriSafePart(FormatProviderId(ProviderId)));
                        outputBuilder.Append(UriSafeDelimiter);
                    }
                    outputBuilder.Append(FormatForUriSafePart(valueTypeForOutput));
                    outputBuilder.Append(UriSafeDelimiter);
                    outputBuilder.Append(FormatForUriSafePart(value));

                    return outputBuilder.ToString();
            }
        }

        /// <summary>
        /// Parses the specified formatted value.
        /// </summary>
        /// <param name="formattedValue">The formatted value.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static HiveId Parse(string formattedValue)
        {
            var tried = TryParse(formattedValue);
            if (!tried.Success)
                throw new FormatException("Cannot parse '{0}' as a HiveId".InvariantFormat(formattedValue));
            return tried.Result;
        }

        public static AttemptTuple<HiveId> TryParse(string formattedValue)
        {
            if (string.IsNullOrEmpty(formattedValue)) return new AttemptTuple<HiveId>(false, HiveId.Empty);
            var style = DetectFormatStyleFromString(formattedValue);

            //I have removed the exception.. shouldn't a 'Try' method not throw exceptions? SD. 16/11/2011
            if (!style.Success)
            {
                return AttemptTuple<HiveId>.False;    
            }
            //Mandate.That(style.Success, x => new FormatException("Could not determine format of '{0}' in parameter formattedValue. Things to check: does the input string look like a valid Uri, but it contains invalid path characters?".InvariantFormat(formattedValue)));

            string splitter = " ";
            switch (style.Result)
            {
                case HiveIdFormatStyle.AsUri:
                    splitter = UriDelimiter;
                    break;
                case HiveIdFormatStyle.UriSafe:
                    splitter = UriSafeDelimiter;
                    break;
                case HiveIdFormatStyle.AutoSingleValue:
                    if (formattedValue == HiveIdValue.Empty.ToString() || formattedValue == HiveId.Empty.ToString())
                        return new AttemptTuple<HiveId>(true, HiveId.Empty);

                    // Try to parse first as a Guid, then an int, then assume it's a string
                    // Then just return from here as we don't need to parse anything else
                    Guid guidValue;
                    if (Guid.TryParse(formattedValue, out guidValue)) return new AttemptTuple<HiveId>(true, new HiveId(guidValue));
                    Int32 intValue;
                    if (Int32.TryParse(formattedValue, out intValue)) return new AttemptTuple<HiveId>(true, new HiveId(intValue));
                    return new AttemptTuple<HiveId>(true, new HiveId(formattedValue, true));
                    break;
                default:
                    return AttemptTuple<HiveId>.False;
            }

            try
            {
                // Check for just a root /
                if (style.Result == HiveIdFormatStyle.AsUri && formattedValue == "/")
                    return new AttemptTuple<HiveId>(true, new HiveId((Uri) null, null, new HiveIdValue("/")));

                // Check for value-less incoming Uris
                // If the string does not contain any of the HiveIdValueType enums, and it's parseable as a Uri, then
                // assume that it is a root value
                if (style.Result == HiveIdFormatStyle.AsUri
                    && Uri.IsWellFormedUriString(formattedValue, UriKind.Absolute))
                {
                    var containsValue = false;
                    foreach (var name in Enum.GetNames(typeof(HiveIdValueTypes)))
                    {
                        if (formattedValue.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            containsValue = true;
                            break;
                        }
                    }
                    if (!containsValue)
                        return new AttemptTuple<HiveId>(true, new HiveId(new Uri(formattedValue), string.Empty, new HiveIdValue("/")));
                }

                // Let's say the input is this:
                // storage://stylesheets/p__provider-name/v__type/value
                // We need to end up with:
                // storage://stylesheets/
                // provider-name
                // type
                // value
                var splitValue = new List<string>(formattedValue.Split(new[] { splitter }, StringSplitOptions.None));
                var stack = new Stack<string>(splitValue);

                var valueDetect = splitValue.Where(x => x.StartsWith(ValuePrefix)).FirstOrDefault() ?? string.Empty;
                var providerDetect = splitValue.Where(x => x.StartsWith(ProviderPrefix)).FirstOrDefault() ?? string.Empty;

                string extractedType = string.Empty;
                string extractedValue = string.Empty;
                string extractedProviderId = string.Empty;
                string extractedRemainder = string.Empty;

                if (string.IsNullOrEmpty(valueDetect)
                    && string.IsNullOrEmpty(providerDetect)
                    && splitValue.Count > 1)
                {
                    // Must be a short-form value with no root or provider id, so re-split clearing out blanks
                    var reSplit = formattedValue.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                    extractedType = reSplit[0];
                    extractedValue = FormatFromUriSafePart(reSplit[1]);
                }
                else
                {
                    var valueStartIndex = splitValue.IndexOf(valueDetect);
                    var wholeValueArray = splitValue.Skip(valueStartIndex + 1).ToArray();
                    var wholeValue = string.Join(splitter.ToString(), wholeValueArray.Select(x => FormatFromUriSafePart((x))));
                    extractedValue = wholeValue;
                    extractedType = valueDetect.TrimStart(ValuePrefix);

                    if (!string.IsNullOrEmpty(providerDetect))
                    {
                        var providerStartIndex = splitValue.IndexOf(providerDetect);
                        extractedProviderId = providerDetect.TrimStart(ProviderPrefix);
                        var extractedRemainderArray = splitValue.Take(providerStartIndex);
                        extractedRemainder = string.Join(splitter.ToString(), extractedRemainderArray);
                    }
                    else
                    {
                        var extractedRemainderArray = splitValue.Take(valueStartIndex);
                        extractedRemainder = string.Join(splitter.ToString(), extractedRemainderArray);
                    }
                }

                switch (style.Result)
                {
                    case HiveIdFormatStyle.UriSafe:
                        extractedRemainder = FormatFromUriSafePart(extractedRemainder);
                        break;
                }

                Uri providerGroupRoot = null;
                extractedRemainder = extractedRemainder.EndsWith(":/") ? extractedRemainder + "/" : extractedRemainder;
                if (!string.IsNullOrEmpty(extractedRemainder))
                    providerGroupRoot = new Uri(extractedRemainder);

                HiveIdValueTypes parsedType;
                var tryParseType = Enum.TryParse<HiveIdValueTypes>(extractedType, true, out parsedType);
                if (!tryParseType) return AttemptTuple<HiveId>.False;

                var tryCreateValue = HiveIdValue.TryCreate(extractedValue, parsedType);

                // Ensure provider id is null if it wasn't detected
                extractedProviderId = string.IsNullOrEmpty(extractedProviderId) ? null : extractedProviderId;

                return new AttemptTuple<HiveId>(true,
                                              new HiveId(providerGroupRoot, extractedProviderId, tryCreateValue.Result));
            }
            catch (UriFormatException ex)
            {
                //I have removed the exception.. shouldn't a 'Try' method not throw exceptions? SD. 16/11/2011
                //this one in particular was causing problems with the RoutingEngine trying to parse a 'user' URL
                //that was not part of Umbraco.
                return AttemptTuple<HiveId>.False;
                //throw new FormatException("Could not parse '{0}' as a HiveId; assumed it was {1} but did not parse correctly"
                //    .InvariantFormat(formattedValue, style.Result.ToString()), ex);
            }

        }

        private static string PopStackIfNotEmpty(Stack<string> stack)
        {
            return stack.Any() ? stack.Pop() : null;
        }

        internal static AttemptTuple<HiveIdFormatStyle> DetectFormatStyleFromString(string formattedValue)
        {
            if (string.IsNullOrEmpty(formattedValue)) return AttemptTuple<HiveIdFormatStyle>.False;

            // If it's parsable as a Uri assume all is OK
            if (Uri.IsWellFormedUriString(formattedValue, UriKind.Absolute))
                return new AttemptTuple<HiveIdFormatStyle>(true, HiveIdFormatStyle.AsUri);

            // Guard: if it starts with some-string:// but Uri doesn't think it's valid, throw an exception
            if (formattedValue.Contains("://"))
                return AttemptTuple<HiveIdFormatStyle>.False;

            // If it starts with ValuePrefix or a / and contains two / it may be a typed short value like /smallint/1234
            if (formattedValue.StartsWith(UriDelimiter) || formattedValue.StartsWith(ValuePrefix)
                && formattedValue.Split(new[] { UriDelimiter }, StringSplitOptions.RemoveEmptyEntries).Length == 2
                && Uri.IsWellFormedUriString(formattedValue, UriKind.Relative))
                return new AttemptTuple<HiveIdFormatStyle>(true, HiveIdFormatStyle.AsUri);

            // If it starts with ProviderPrefix and contains ValuePrefix and parses as a Uri...
            if (formattedValue.StartsWith(ProviderPrefix) 
                && formattedValue.IndexOf(ValuePrefix, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                if (formattedValue.Contains(UriSafeDelimiter))
                    return new AttemptTuple<HiveIdFormatStyle>(true, HiveIdFormatStyle.UriSafe);
                if (formattedValue.Contains(UriDelimiter) && Uri.IsWellFormedUriString(formattedValue, UriKind.Relative))
                    return new AttemptTuple<HiveIdFormatStyle>(true, HiveIdFormatStyle.AsUri);
            }
                


            // If not, check number of delimiters
            var split = formattedValue.Split(new[] { UriSafeDelimiter }, StringSplitOptions.None);
            if (split.Length > 1 && split.Length < 6) return new AttemptTuple<HiveIdFormatStyle>(true, HiveIdFormatStyle.UriSafe);

            // If it's a single value, but not a parseable Uri, assume it is an auto-detect value
            if (split.Length == 1) return new AttemptTuple<HiveIdFormatStyle>(true, HiveIdFormatStyle.AutoSingleValue);

            return AttemptTuple<HiveIdFormatStyle>.False;
        }
    }
}
