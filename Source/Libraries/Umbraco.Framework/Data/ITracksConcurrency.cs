namespace Umbraco.Framework.Data
{
    using Umbraco.Framework.Data.Common;

    /// <summary>
    ///   Defines a concurrency token for tracking object changes between tiers
    /// </summary>
    public interface ITracksConcurrency
    {
        /// <summary>
        /// Gets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        IConcurrencyToken ConcurrencyToken { get; set; }
    }
}