using System.Collections.Generic;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.MappingModel;

namespace Sandbox.Hive.Domain.HiveDomain
{
  public class MappedEntityAssociateList : Dictionary<AssociationType, IEnumerable<MappedEntity>>
  {
    
  }
}