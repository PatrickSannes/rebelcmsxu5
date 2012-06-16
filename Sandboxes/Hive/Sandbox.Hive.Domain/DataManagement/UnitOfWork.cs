using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Hive.Domain.DataManagement;

namespace Sandbox.PersistenceProviders.NHibernate.DataManagement
{
  public class UnitOfWork : IUnitOfWork 
  {
    private readonly IDataContextFactory _dataContextFactory;
    private readonly ITransaction _transaction;

    public UnitOfWork(IDataContextFactory dataContextFactory)
    {
      _dataContextFactory = dataContextFactory;

      DataContext = _dataContextFactory.CreateDataContext();
      _transaction = DataContext.BeginTransaction();
    }

    public void Dispose()
    {
      _dataContextFactory.Dispose();
    }

    public IDataContext DataContext { get; private set; }

    public void Commit()
    {
      _transaction.Commit();
    }
  }
}
