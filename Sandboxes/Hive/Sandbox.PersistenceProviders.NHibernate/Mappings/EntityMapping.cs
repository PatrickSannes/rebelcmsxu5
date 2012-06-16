using System;
using FluentNHibernate.Mapping;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;

namespace Sandbox.PersistenceProviders.NHibernate.Mappings
{
  public class EntityMapping : ClassMap<PersistedEntity>
  {
    public EntityMapping()
    {
      Table("[Entities]");
      Id(x => x.Key, "Id").GeneratedBy.UuidHex("N");
      Map(x => x.ParentKey, "ParentId").Not.Nullable().Length(50).Default("");
      Map(x => x.Value, "Value").Nullable().Length(50);
      HasMany<PersistedAttribute>(x => x.Attributes)
        .KeyColumn("EntityId")
        .LazyLoad()
        .Cascade.AllDeleteOrphan();
    }
  }
}
