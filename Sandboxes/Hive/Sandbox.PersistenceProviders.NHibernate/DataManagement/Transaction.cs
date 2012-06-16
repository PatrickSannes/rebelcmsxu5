using Sandbox.Hive.Domain.DataManagement;

namespace Sandbox.PersistenceProviders.NHibernate.DataManagement
{
  public class Transaction : ITransaction
  {
    private readonly global::NHibernate.ITransaction _transaction;

    public Transaction(global::NHibernate.ITransaction transaction)
    {
      _transaction = transaction;
    }

    #region ITransaction Members

    public void Begin()
    {
      _transaction.Begin();
    }

    public void Commit()
    {
      _transaction.Commit();
    }

    public void Rollback()
    {
      _transaction.Rollback();
    }

    public void Dispose()
    {
      _transaction.Dispose();
    }

    #endregion
  }
}