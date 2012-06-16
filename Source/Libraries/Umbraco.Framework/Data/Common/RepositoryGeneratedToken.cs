using System;

namespace Umbraco.Framework.Data.Common
{
    /// <summary>
    /// A no-op concurrency token for use when objects have yet to be created in the repository.
    /// </summary>
    public class RepositoryGeneratedToken : IConcurrencyToken
    {
        #region Implementation of IConcurrencyToken

        /// <summary>
        ///   Gets or sets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        public object ConcurrencyToken
        {
            get { return Guid.Empty; }
            set { return; }
        }

        #endregion
    }
}
