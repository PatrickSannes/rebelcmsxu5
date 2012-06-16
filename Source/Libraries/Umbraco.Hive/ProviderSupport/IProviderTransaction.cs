using System;

namespace Umbraco.Hive.ProviderSupport
{
    using System.Collections.Generic;

    public interface IProviderTransaction : IDisposable
    {
        bool IsTransactional { get; }
        bool IsActive { get; }

        /// <summary>
        /// Gets a value indicating whether this instance was committed.
        /// </summary>
        /// <value><c>true</c> if committed; otherwise, <c>false</c>.</value>
        bool WasCommitted { get; }

        /// <summary>
        /// Gets a value indicating whether this instance was rolled back.
        /// </summary>
        /// <value><c>true</c> if rolled back; otherwise, <c>false</c>.</value>
        bool WasRolledBack { get; }

        /// <summary>
        /// Gets a list of actions that will be performed on completion, for example cache updating
        /// </summary>
        /// <value>The on completion actions.</value>
        List<Action> CacheFlushActions { get; }

        void Rollback(IProviderUnit work);
        void Commit(IProviderUnit work);
        string GetTransactionId();
    }
}