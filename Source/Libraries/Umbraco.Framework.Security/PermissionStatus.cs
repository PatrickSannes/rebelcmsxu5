using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Umbraco.Framework.Security
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PermissionStatus
    {
        Allow,
        Deny,
        Inherit
    }
}
