using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Hive.Domain.DataManagement;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;

namespace Sandbox.PersistenceProviders.MockedInMemory
{
  public class ReadWriteRepository : IPersistenceRepository
  {
    public ReadWriteRepository(string @alias)
    {
      RepositoryKey = @alias;
    }

    public string RepositoryKey { get; set; }

    public IList<PersistedEntity> GetAssociations(dynamic callerId)
    {
      return _entities.Where(a => a.Value.ParentKey == callerId).Select(pair => pair.Value).ToList();
    }

    private Dictionary<string, PersistedEntity> _entities = new Dictionary<string, PersistedEntity>();

    public IUnitOfWork UnitOfWork { get; set; }

    public int Count
    {
      get { return _entities.Count; }
    }

    public PersistedEntity Get(dynamic id)
    {
      //TODO: Normalise the string in a guaranteed way
      return _entities.ContainsKey(id.ToString()) ? _entities[id.ToString()] : null;
    }

    public void Add(PersistedEntity persistedEntity)
    {
      if (_entities.ContainsKey(persistedEntity.Key.ToString()))
        _entities.Remove(persistedEntity.Key.ToString());

      _entities.Add(persistedEntity.Key.ToString(), persistedEntity);
    }
  }
}
