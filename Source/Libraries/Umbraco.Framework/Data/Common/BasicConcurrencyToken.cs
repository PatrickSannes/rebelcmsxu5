using System;

namespace Umbraco.Framework.Data.Common
{
    /// <summary>
    /// Represents a basic concurrency token using a serialised datestamp
    /// </summary>
    public struct BasicConcurrencyToken : IConcurrencyToken
    {
        /// <summary>
        /// Generates a new <see cref="BasicConcurrencyToken"/> from a <see cref="TimeSpan"/>
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public static BasicConcurrencyToken FromTimeSpan(TimeSpan timeSpan)
        {
            var basicConcurrentyToken = new BasicConcurrencyToken();
            basicConcurrentyToken.ConcurrencyToken = timeSpan.Ticks;
            return basicConcurrentyToken;
        }


        #region Implementation of IConcurrencyToken

        /// <summary>
        ///   Gets or sets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        public object ConcurrencyToken { get; set; }

        #endregion
    }
}
