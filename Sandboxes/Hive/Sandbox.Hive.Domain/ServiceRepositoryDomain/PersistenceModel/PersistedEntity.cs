using System;
using System.Collections.Generic;

namespace Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel
{
  public class PersistedEntity
  {
    public PersistedEntity()
    {
      HasParentsInThisProvider = false;
      Attributes = new List<PersistedAttribute>();
    }

    public virtual string PersistenceProviderKey { get; set; }

    public virtual string ParentKey { get; set; }

    public virtual bool HasParentsInThisProvider { get; set; }

    public virtual IList<PersistedAttribute> Attributes { get; set; }

    public virtual string Key { get; set; }

    public virtual string Value { get; set; }
  }
}