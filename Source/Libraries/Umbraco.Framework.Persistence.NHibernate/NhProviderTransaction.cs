using System;
using System.Diagnostics;
using System.Transactions;
using NHibernate;
using Umbraco.Framework.Diagnostics;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.NHibernate
{
    //[DebuggerDisplay("Is nested? {_isNestedInActiveTransaction} Transaction instance Id: {_transactionId}")]
    [DebuggerDisplay("Transaction instance Id: {_transactionId}")]
    public class NhProviderTransaction : AbstractProviderTransaction
    {

        private readonly ITransaction _nhTransaction;
        private readonly ISession _nhSession;
        private readonly Guid _transactionId = Guid.NewGuid();
        private readonly bool _isNestedInActiveTransaction;

        [ThreadStatic]
        private static int? _nestingLevel;

        protected static int NestingLevel
        {
            get
            {
                if (_nestingLevel == null) _nestingLevel = -1;
                return _nestingLevel.Value;
            }
            set
            {
                _nestingLevel = value;
            }
        }

        public void IncrementNestingLevel()
        {
            NestingLevel = NestingLevel + 1;
        }

        public void DecrementNestingLevel()
        {
            NestingLevel = NestingLevel - 1;
        }

        public NhProviderTransaction(global::NHibernate.ISession nhSession, bool isNestedInActiveTransaction = false)
        {

            _nhSession = nhSession;
            _isNestedInActiveTransaction = isNestedInActiveTransaction;
            //_nhTransaction = nhSession.Transaction;

            IncrementNestingLevel();

            if (!isNestedInActiveTransaction)
            {
                _nhTransaction = nhSession.BeginTransaction();
            }
            else
            {
                _nhTransaction = nhSession.Transaction;
            }

            LogHelper.TraceIfEnabled<NhProviderTransaction>("Constructing transaction {0} on instance {1}", () => _nhTransaction.GetHashCode(), () => GetHashCode());

            //if (isNestedInActiveTransaction)
            //{
            //    _nhTransaction = NhSession.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
            //}
            //else
            //{
            //    _nhTransaction = NhSession.Transaction;
            //}

            Begin();
        }

        public override string GetTransactionId()
        {
            return _nhTransaction.GetHashCode().ToString();
        }

        private void Begin()
        {
            this.CheckThrowObjectDisposed(base.IsDisposed, "NHibernate...Transaction:Begin");
            if (!_nhTransaction.IsActive)
            {
                LogHelper.TraceIfEnabled<NhProviderTransaction>("Starting new Nh transaction {0} on instance {1}", () => _nhTransaction.GetHashCode(), () => GetHashCode());
                _nhTransaction.Begin();
            }
        }

        public override bool WasCommitted
        {
            get { return _nhTransaction.WasCommitted; }
        }
        public override bool WasRolledBack
        {
            get { return _nhTransaction.WasRolledBack; }
        }
        public override bool IsActive
        {
            get { return _nhTransaction.IsActive; }
        }

        protected override bool PerformExplicitRollback()
        {
            if (!WasRolledBack && !WasCommitted)
            {
                LogHelper.TraceIfEnabled<NhProviderTransaction>("Rolling back transaction {0} on instance {1}", () => _nhTransaction.GetHashCode(), () => GetHashCode());
                _nhTransaction.Rollback();
                return _nhTransaction.WasRolledBack;
            }
            return false;
        }

        /// <summary>
        /// If its implicit we don't do an explicit rollback on the NH transaction as it does it itself on disposal.
        /// Currently we're just going to returns false sinse we're never actually going to be doing anything and since we're also overriding
        /// the WasRolledback property, it doesn't really make any difference.
        /// </summary>
        /// <returns></returns>
        protected override bool PerformImplicitRollback()
        {
            return false;
        }

        [ThreadStatic]
        private static int? _commitWasAttemptedWhileNested;

        protected override bool PerformCommit()
        {
            if (!WasCommitted)
            {
                var nestingLevel = NestingLevel;
                try
                {
                    if (_isNestedInActiveTransaction)
                    {
                        LogHelper.TraceIfEnabled<NhProviderTransaction>("Not committing Nh transaction {0} as it's nested (I'm instance {1}) (nesting level: {2})", () => _nhTransaction.GetHashCode(), () => GetHashCode(), () => nestingLevel);
                        _commitWasAttemptedWhileNested = NestingLevel;
                        return WasCommitted;
                    }
                    LogHelper.TraceIfEnabled<NhProviderTransaction>("Committing Nh transaction {0} on instance {1} (nesting level: {2})", () => _nhTransaction.GetHashCode(), () => GetHashCode(), () => nestingLevel);
                    _nhTransaction.Commit();
                    return WasCommitted;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<NhProviderTransaction>(ex.Message, ex);
                    throw;
                }
            }
            return false;
        }

        public override bool IsTransactional
        {
            get { return true; }
        }

        public ISession NhSession { get { return _nhSession; } }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            base.DisposeResources();
            var nestingLevel = NestingLevel;
            //don't dispose the underlying transactoin if this is a nested active transaction
            if (_nhTransaction != null)
            {
                if (_commitWasAttemptedWhileNested.HasValue && NestingLevel == 0)
                {
                    LogHelper.TraceIfEnabled<NhProviderTransaction>(
                        "In dispose, automatically committing Nh transaction {0} from instance {1} because a nested transaction tried to commit (nesting level: {2})",
                        () => _nhTransaction.GetHashCode(),
                        () => GetHashCode(),
                        () => nestingLevel);
                    PerformCommit();
                }
                
                if (!_isNestedInActiveTransaction)
                {
                    try
                    {
                        // Note, we don't call rollback here because NH does an implicit rollback upon disposal, if the transaction was not already rolled back.
                        // We have our own implicit mechanism which the Rollback method takes into account.
                        LogHelper.TraceIfEnabled<NhProviderTransaction>(
                            "Disposing Nh transaction {0} and instance {1} (nesting level: {2})",
                            () => _nhTransaction.GetHashCode(),
                            () => GetHashCode(),
                            () => nestingLevel);
                        _nhTransaction.Dispose();
                    }
                    catch (ObjectDisposedException oex)
                    {
                        /* Ignore if _nhTransaction has already been disposed. Unfortunately no way to find that out from NH's ITransaction interface. */
                        LogHelper.Error<NhProviderTransaction>(oex.Message, oex);
                    }
                }

                else
                {
                    LogHelper.TraceIfEnabled<NhProviderTransaction>(
                        "Not disposing transaction {0} but disposing instance {1} (nesting level: {2})",
                        () => _nhTransaction.GetHashCode(),
                        () => GetHashCode(),
                        () => nestingLevel);
                }
            }

            DecrementNestingLevel();
        }
    }
}