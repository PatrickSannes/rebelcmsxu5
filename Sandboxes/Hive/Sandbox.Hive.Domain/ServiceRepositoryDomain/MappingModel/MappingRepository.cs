using System.Collections.Generic;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;

namespace Sandbox.Hive.Domain.ServiceRepositoryDomain.MappingModel
{
  public class MappingRepository
  {
    //static MappingRepository()
    //{
    //  AutoMapper.Mapper.CreateMap<PersistedEntity, MappedEntity>()
    //    .ForMember(dest => dest.Id, opt => opt.ResolveUsing<IdentityResolver>());

    //  AutoMapper.Mapper.AssertConfigurationIsValid();
    //}

    //private readonly IDictionary<string, IPersistenceRepository> _dataStores;

    //public MappingRepository(IEnumerable<PersistenceRepository> dataStores) : this()
    //{
    //  foreach (PersistenceRepository persistenceRepository in dataStores)
    //    RegisterDataStore(persistenceRepository);
    //}

    //public MappingRepository()
    //{
    //  _dataStores = new Dictionary<string, IPersistenceRepository>();
    //}

    //public IDictionary<string, IPersistenceRepository> DataStores
    //{
    //  get { return _dataStores; }
    //}

    //public void RegisterDataStore(PersistenceRepository persistenceRepository)
    //{
    //  DataStores.Add(persistenceRepository.RepositoryKey, persistenceRepository);
    //}

    //#region Repository

    //public MappedEntity Get(MappedIdentifier id)
    //{
    //  IPersistenceRepository persistenceRepository = DataStores[id.ProviderKey];
    //  var entity = persistenceRepository.Get(id.Value);
    //  return AutoMapper.Mapper.Map<PersistedEntity, MappedEntity>(entity);
    //}

    //#endregion
  }
}