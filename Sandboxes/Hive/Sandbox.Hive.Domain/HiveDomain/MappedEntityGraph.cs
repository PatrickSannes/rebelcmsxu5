using Sandbox.Hive.Domain.ServiceRepositoryDomain.MappingModel;

namespace Sandbox.Hive.Domain.HiveDomain
{
  public class MappedEntityGraph : MappedEntity
  {
    public MappedEntityGraph Children { get; set; }

    public MappedEntityAssociateList Associates { get; set; }

    public MappedEntity Parent { get; set; }
  }
}