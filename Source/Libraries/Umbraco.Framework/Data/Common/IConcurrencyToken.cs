namespace Umbraco.Framework.Data.Common
{
    /// <summary>
    /// Defines mechanisms necessary for tracking whether an object has changed
    /// between tiers
    /// </summary>
    public interface IConcurrencyToken
    {
        /// <summary>
        ///   Gets or sets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        object ConcurrencyToken { get; set; }
    }
}
