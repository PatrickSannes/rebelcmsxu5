namespace Sandbox.Hive.Domain.ServiceRepositoryDomain.MappingModel
{
  public class MappedEntity
  {
    public MappedEntity()
    {
      //Children = new List<MappedEntity>();
      //Associations = new Dictionary<AssociationType, IEnumerable<MappedIdentifier>>();
    }

    public MappedIdentifier Id { get; set; }

    //public MappedEntity Parent { get; set; }

    //public IEnumerable<MappedEntity> Children { get; set; }

    //public IDictionary<AssociationType, IEnumerable<MappedIdentifier>> Associations { get; set; }

    public string Value { get; set; }
  }
}
