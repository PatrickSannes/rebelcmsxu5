using System;
using System.Collections.Concurrent;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;

namespace Umbraco.Hive.ProviderSupport
{
    public sealed class ProviderUnit : DisposableObject, IProviderUnit
    {
        private AbstractEntityRepository _entitySession;

        public ProviderUnit(AbstractEntityRepository entityRepository, AbstractScopedCache unitScopedCache)
        {
            EntityRepository = entityRepository;
            WorkCompleting += (obj, target) =>
                                  {
                                      EntityRepository.PrepareForCompletion();
                                      EntityRepository.Revisions.PrepareForCompletion();
                                      EntityRepository.Schemas.PrepareForCompletion();
                                      EntityRepository.Schemas.Revisions.PrepareForCompletion();
                                  };
            FrameworkContext = entityRepository.FrameworkContext;
            UnitScopedCache = unitScopedCache;
        }

        public EventHandler<WorkEventArgs> WorkAbandoning;
        public EventHandler<WorkEventArgs> WorkAbandoned;
        public EventHandler<WorkEventArgs> WorkCompleting;
        public EventHandler<WorkEventArgs> WorkComplete;

        #region IProviderUnit Members

        public AbstractEntityRepository EntityRepository
        {
            get
            {
                this.CheckThrowObjectDisposed(IsDisposed, "ProviderUnit");
                return _entitySession;
            }
            protected set { _entitySession = value; }
        }

        public void Complete()
        {
            this.CheckThrowObjectDisposed(IsDisposed, "ProviderUnit");

            try
            {
                // Check the first transaction to see if it's already been committed.
                // TODO: Check the other transactions too; however, this causes a problem with the API
                // because a) there's no easy way to share a transaction between Entity / Schema / Revision factories
                // plus there's no guarantee that the other factories can use the same transaction type anyway. 
                // However despite this the Nh provider does share the same underlying transaction and would cause
                // errors if we check for "WasCommitted" in the second try-finally in the below block (APN 08/Nov/11)
                CheckThrowTransactionActive(EntityRepository.Transaction);

                WorkCompleting.IfNotNull(x => x.Invoke(this, new WorkEventArgs(this)));
                ConditionalCommit(EntityRepository.Transaction);
            }
            finally
            {
                try
                {
                    ConditionalCommit(EntityRepository.Schemas.Transaction);
                }
                finally
                {
                    try
                    {
                        ConditionalCommit(EntityRepository.Schemas.Revisions.Transaction);
                    }
                    finally
                    {
                        ConditionalCommit(EntityRepository.Revisions.Transaction);
                        WorkComplete.IfNotNull(x => x.Invoke(this, new WorkEventArgs(this)));
                    }
                }
            }
        }

        public void Abandon()
        {
            this.CheckThrowObjectDisposed(IsDisposed, "ProviderUnit");

            try
            {
                WorkAbandoning.IfNotNull(x => x.Invoke(this, new WorkEventArgs(this)));
                ConditionalRollback(EntityRepository.Transaction);
            }
            finally
            {
                try
                {
                    ConditionalRollback(EntityRepository.Schemas.Transaction);
                }
                finally
                {
                    try
                    {
                        ConditionalRollback(EntityRepository.Schemas.Revisions.Transaction);
                    }
                    finally
                    {
                        ConditionalRollback(EntityRepository.Revisions.Transaction);
                        WorkAbandoned.IfNotNull(x => x.Invoke(this, new WorkEventArgs(this)));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the unit-scoped cache.
        /// </summary>
        /// <value>The unit scoped cache.</value>
        public AbstractScopedCache UnitScopedCache { get; private set; }

        #endregion

        private void CheckThrowTransactionActive(IProviderTransaction providerTransaction)
        {
            if (providerTransaction.IsTransactional && providerTransaction.WasCommitted)
                throw new TransactionCompletedException("Transaction cannot be completed as it is not active. Transaction type: " + providerTransaction.GetType().FullName);
        }

        private readonly ConcurrentBag<IProviderTransaction> _committedTransactions = new ConcurrentBag<IProviderTransaction>();
        private void ConditionalCommit(IProviderTransaction providerTransaction)
        {
            if (providerTransaction.IsTransactional && providerTransaction.IsActive)
            {
                providerTransaction.Commit(this);
                _committedTransactions.Add(providerTransaction);
            }
        }

        private void ConditionalRollback(IProviderTransaction providerTransaction)
        {
            if (providerTransaction.IsTransactional && providerTransaction.IsActive)
                providerTransaction.Rollback(this);
        }

        protected override void DisposeResources()
        {
            Abandon();
            EntityRepository.Dispose();
        }

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; private set; }

        #endregion
    }
}