using System;

namespace Sandbox.Hive.Domain.DataManagement
{
  public interface IUnitOfWork : IDisposable
  {
    IDataContext DataContext { get; }

    void Commit();
  }
}