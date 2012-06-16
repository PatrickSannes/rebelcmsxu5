using System;
using NHibernate;
using Sandbox.Hive.Domain.DataManagement;
using ITransaction = Sandbox.Hive.Domain.DataManagement.ITransaction;

namespace Sandbox.PersistenceProviders.NHibernate.DataManagement
{
  public class DataContext : IDataContext
  {
    private readonly ISession _nhibernateSession;

    public DataContext(ISession nhibernateSession)
    {
      _nhibernateSession = nhibernateSession;
    }

    public ISession NhibernateSession
    {
      get { return _nhibernateSession; }
    }

    #region IDataContext Members

    public ITransaction BeginTransaction()
    {
      return new Transaction(NhibernateSession.BeginTransaction());
    }

    public T Get<T>(string id) where T : class
    {
      return NhibernateSession.Get<T>(id);
    }

    public object Save(object data)
    {
      return NhibernateSession.Save(data);
    }

    public T Save<T>(T data) where T : class
    {
      return NhibernateSession.Save(data) as T;
    }

    public void Close()
    {
      NhibernateSession.Close();
    }

    public void Dispose()
    {
      NhibernateSession.Dispose();
    }

    #endregion
  }
}