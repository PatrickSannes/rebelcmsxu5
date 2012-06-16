using Sandbox.Hive.Domain.ServiceRepositoryDomain;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;
using Sandbox.Hive.Foundation;

namespace Sandbox.Hive.Domain.HiveDomain
{
  public class HiveRepository
  {
    private readonly IDependencyResolver _dependencyResolver;

    public HiveRepository()
    {
      _dependencyResolver = DependencyResolver.Current;
    }

    public HiveRepository(IDependencyResolver dependencyResolver)
    {
      _dependencyResolver = dependencyResolver;
    }

    public MappedEntityGraph Get(MappedIdentifier identifier, int traversalDepth)
    {
      /*
       * Get the repository with the given key
       * Get the entity with the given key (to depth)
       * Check with the Hive Store whether other providers have children for this repo-entity pair
       * If so, get the repo-entity keypairs from Hive Store
       * For each repo, get the entities
       * Walk the lists adding the entities to a graph
       * 
       */

      var providerTuple = _dependencyResolver.TryResolve<IPersistenceProvider>(identifier.ProviderKey);

      MappedEntityGraph mappedEntityGraph = null;

      if (providerTuple.Success)
      {
        PersistedEntity entity = providerTuple.Value.Reader.Get(identifier.Value);

        if (entity != null)
        {
          //TODO: Replace with AutoMapper
          //TODO: Pad-out attributes etc.
          //TODO: Find children from other providers
          mappedEntityGraph = new MappedEntityGraph();
          mappedEntityGraph.Id = identifier;
          mappedEntityGraph.Value = entity.Value;
        }
      }

      return mappedEntityGraph;
    }
  }
}
