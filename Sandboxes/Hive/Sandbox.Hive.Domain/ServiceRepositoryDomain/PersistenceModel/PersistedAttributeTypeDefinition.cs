using System.ComponentModel.DataAnnotations;

namespace Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel
{
  public class PersistedAttributeTypeDefinition
  {
    public string Key { get; set; }

    public string GroupName { get; set; }

    public int Ordinal { get; set; }

    public DataType Type { get; set; }
  }
}