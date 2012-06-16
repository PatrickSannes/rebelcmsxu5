namespace Umbraco.Framework.Diagnostics
{
    /// <summary>
    /// Enumeration depicting the availability of counters for write operations.
    /// </summary>
    public enum CounterAvailability
    {
        /// <summary>
        /// Availability has not yet been determined
        /// </summary>
        Unknown,
        /// <summary>
        /// Counters are not writable due to insufficient permissions
        /// </summary>
        NoneDueToPermissions,
        /// <summary>
        /// Counters are not writable due to a general error
        /// </summary>
        NoneDueToError,
        /// <summary>
        /// Counters are available for writing
        /// </summary>
        Available
    }
}