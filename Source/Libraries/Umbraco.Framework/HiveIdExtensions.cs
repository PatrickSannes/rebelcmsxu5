using System;
using System.Text;
using Newtonsoft.Json.Linq;


namespace Umbraco.Framework
{
    public static class HiveIdExtensions
    {
        public const string HtmlIdPrefix = "u_";

        /// <summary>
        /// Returns a JObject representing the HiveId for use in JavaScript
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static JObject ToJsonObject(this HiveId id)
        {
            //add a custom json id to the metadata
            return JObject.FromObject(new
            {
                htmlId = id.GetHtmlId(),
                rawValue = id,
                value = id.Value.Value == null ? "" : id.Value.Value.ToString(),
                valueType = id.Value.Type,
                provider = id.ProviderId.IsNullOrWhiteSpace() ? "" : id.ProviderId,
                scheme = id.ProviderGroupRoot == null ? "" : id.ProviderGroupRoot.ToString()
            });
        }

        /// <summary>
        /// Compares two strings by parsing them as <see cref="HiveId"/> to check equality.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="ignoreProviderId"></param>
        /// <returns></returns>
        public static bool CompareStrings(string left, string right, bool? ignoreProviderId = true)
        {
            var leftId = HiveId.TryParse(left);
            var rightId = HiveId.TryParse(right);
            if (leftId.Success && rightId.Success)
            {
                if (!ignoreProviderId.HasValue || ignoreProviderId.Value)
                    return leftId.Result.EqualsIgnoringProviderId(rightId.Result);
                return leftId.Result == rightId.Result;
            }
            return false;
        }

        public static AttemptTuple<HiveId> TryParseFromHtmlId(string htmlId)
        {
            // TODO: This is very loose (APN)
            // HtmlId version of the way HiveId is used by the backoffice generally starts with HtmlIdPrefix and will have a dot at the end with an item name
            // e.g. u_my-id-here.TemplateId
            // However the id itself can correctly contain dots (e.g. a file extension) so we have to trust that it is the last dot... hmm
            if (!htmlId.StartsWith(HtmlIdPrefix))
                return AttemptTuple<HiveId>.False;
            string forParsing = htmlId.TrimStart(HtmlIdPrefix).Replace("---", ".").Replace("___", "$");
            var lastDot = forParsing.LastIndexOf('.');
            if (lastDot < 0)
                return AttemptTuple<HiveId>.False;
            forParsing = forParsing.Remove(lastDot);
            return HiveId.TryParse(forParsing);
        }

        /// <summary>
        /// Convert the HiveId to a compatible Id used for Html elements
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetHtmlId(this HiveId id)
        {
            return HtmlIdPrefix + id.ToString(HiveIdFormatStyle.UriSafe).Replace(".", "---").Replace("$", "___");
        }

        // TODO: This is to be removed pending a refactor of GetByPath
        public static string[] StringParts(this HiveId id)
        {
            return ((string) id.Value).Split(new[] {'/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Determines whether <paramref name="id"/> is null or empty.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns><c>true</c> if [is null or empty] [the specified id]; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public static bool IsNullValueOrEmpty(this HiveId id)
        {
            if (id == HiveId.Empty) return true;
            if (id.Value.Type == HiveIdValueTypes.Guid && (Guid)id.Value == Guid.Empty) return true;
            //if (id.Value.Type == HiveIdValueTypes.String && ((string)id.Value) == null) return true;
            return id.Value == HiveIdValue.Empty;
        }

        public static bool IsNullValueOrEmpty(this HiveId? id)
        {
            if (!id.HasValue) return true;
            return id.Value.IsNullValueOrEmpty();
        }
        
        /// <summary>
        /// Returns null if the id is null or empty, otherwise returns the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static HiveId ConvertToEmptyIfNullValue(this HiveId id)
        {
            return id.IsNullValueOrEmpty() ? HiveId.Empty : id;
        }

        public static HiveId? NullIfEmpty(this HiveId id)
        {
            return id.IsNullValueOrEmpty() ? new HiveId?() : id;
        }
    }
}