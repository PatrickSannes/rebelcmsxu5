using System;

namespace Sandbox.Hive.Domain.DataManagement
{
  public interface ITransaction : IDisposable
  {
    void Begin();
    void Commit();
    void Rollback();
  }
}