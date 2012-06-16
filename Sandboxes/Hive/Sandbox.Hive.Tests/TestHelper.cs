using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Id;
using Sandbox.Hive.Domain.ServiceRepositoryDomain;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;

namespace Sandbox.Hive.Tests
{
  public static class TestHelper
  {
    public static PersistedEntity CreatePersistedEntity()
    {
      return CreatePersistedEntity("testrepo");
    }

    public static PersistedEntity CreatePersistedEntity(string persistenceProviderKey)
    {
      var persistedEntity = new PersistedEntity();
      persistedEntity.Key = new UUIDStringGenerator().Generate(null, null).ToString();
      persistedEntity.ParentKey = string.Empty;
      persistedEntity.PersistenceProviderKey = persistenceProviderKey;
      persistedEntity.Value = "test value";
      return persistedEntity;
    }
  }
}
