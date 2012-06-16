using System.Collections.Generic;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;

namespace Sandbox.Hive.Domain.HiveDomain
{
  public class PersistenceProviderCollection : Dictionary<string, IPersistenceProvider>
  {
    //TODO: Replace dictionary value with persistence provider not persistence repository
  }
}
