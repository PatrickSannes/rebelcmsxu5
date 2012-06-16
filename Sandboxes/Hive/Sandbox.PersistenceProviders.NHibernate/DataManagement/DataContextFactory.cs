using System;
using NHibernate;
using Sandbox.Hive.Domain.DataManagement;

namespace Sandbox.PersistenceProviders.NHibernate.DataManagement
{
  public class DataContextFactory : IDataContextFactory
  {
    private readonly ISessionFactory _sessionFactory;

    public DataContextFactory(ISessionFactory sessionFactory)
    {
      _sessionFactory = sessionFactory;
    }

    public IDataContext CreateDataContext()
    {
      return new DataContext(_sessionFactory.OpenSession());
    }

    public void Dispose()
    {
      _sessionFactory.Dispose();
    }
  }
}