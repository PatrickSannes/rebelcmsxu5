using System;

namespace Sandbox.Hive.Domain.DataManagement
{
  public interface IDataContextFactory : IDisposable
  {
    IDataContext CreateDataContext();
  }
}