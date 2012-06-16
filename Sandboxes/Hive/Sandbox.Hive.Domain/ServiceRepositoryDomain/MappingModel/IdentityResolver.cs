using AutoMapper;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;

namespace Sandbox.Hive.Domain.ServiceRepositoryDomain.MappingModel
{
  public class IdentityResolver : ValueResolver<PersistedEntity, MappedIdentifier>
  {
    protected override MappedIdentifier ResolveCore(PersistedEntity source)
    {
      return new MappedIdentifier(){ProviderKey = source.PersistenceProviderKey, Value = source.Key};
    }
  }
}