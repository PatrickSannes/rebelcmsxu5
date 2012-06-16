using System;

namespace Sandbox.Hive.Domain.DataManagement
{
  public interface IDataContext : IDisposable
  {
    ITransaction BeginTransaction();

    object Save(object data);

    T Save<T>(T data) where T : class ;

    void Close();
    T Get<T>(string id) where T : class;
  }
}
