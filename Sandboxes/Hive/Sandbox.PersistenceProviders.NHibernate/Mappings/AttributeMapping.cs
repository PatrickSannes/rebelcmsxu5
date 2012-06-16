using FluentNHibernate.Mapping;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;

namespace Sandbox.PersistenceProviders.NHibernate.Mappings
{
  public class AttributeMapping : ClassMap<PersistedAttribute>
  {
    public AttributeMapping()
    {
      Table("[Attributes]");
      Id(x => x.Id, "Id").GeneratedBy.GuidComb();
      Map(x => x.Key, "[Key]").Not.Nullable();
      Map(x => x.Value, "Value").Not.Nullable();
    }
  }
}