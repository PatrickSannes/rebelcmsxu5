

using Umbraco.Framework.Context;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Framework.Persistence.XmlStore.DataManagement.ReadWrite
{
    public class ReadWriteUnitOfWork : AbstractReadWriteUnitOfWork
    {
        private readonly ITransaction _transaction;

        public ReadWriteUnitOfWork(AbstractDataContext dataContext)
            : base(dataContext)
        {
            Mandate.ParameterNotNull(dataContext, "dataContext");
            Mandate.ParameterCondition(dataContext is DataContext, "dataContext");

            _repo = new RepositoryReadWriter(dataContext as DataContext);
            _transaction = DataContext.BeginTransaction();
        }

        private readonly IRepositoryReadWriter _repo;

        protected override void DisposeTransaction()
        {
            if (_transaction != null) _transaction.Dispose();
        }

        public override IRepositoryReadWriter ReadWriteRepository
        {
            get { return _repo; }
        }

        /// <summary>
        /// Commits this unit of work.
        /// </summary>
        /// <remarks></remarks>
        public override void Commit()
        {
            this.CheckThrowObjectDisposed(base.IsDisposed, "XmlStore...ReadWriteUnitOfWork:Commit");
            _transaction.Commit();
        }

        public override IRepositoryReader ReadRepository
        {
            get { return _repo; }
        }
    }
}