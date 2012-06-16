using System;

namespace Umbraco.Framework
{
    public enum HiveIdFormatStyle
    {
        /// <summary>
        /// The serialization format persists the <see cref="HiveId"/> as a Uri.
        /// </summary>
        AsUri,
        /// <summary>
        /// The serialization format persists the <see cref="HiveId"/> as a dot-separated value, for example
        /// <value>scheme.provider-group-root.provider-id.value-type.value</value>
        /// </summary>
        UriSafe,
        /// <summary>
        /// The serialization format persists only the raw value as a string and attempts to auto-detect the type (<see cref="Guid"/>, <see cref="Int32"/> etc.) based on checking the validity of the incoming value for each type.
        /// </summary>
        AutoSingleValue
    }
}