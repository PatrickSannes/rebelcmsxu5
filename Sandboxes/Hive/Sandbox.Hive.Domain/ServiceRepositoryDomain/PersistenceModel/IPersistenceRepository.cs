using System.Collections.Generic;
using Sandbox.Hive.Domain.DataManagement;

namespace Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel
{
  public interface IRepository<TEntityType> where TEntityType : class, new()
  {
    IUnitOfWork UnitOfWork { get; set; }
    int Count { get; }
    TEntityType Get(dynamic id);
    void Add(TEntityType persistedEntity);
  }

  public interface IPersistenceRepository : IRepository<PersistedEntity>
  {
    //TODO: Investigate why methods with a dynamic parameter declared on a base interface are not inherited
    PersistedEntity Get(dynamic id);

    string RepositoryKey { get; set; }

    IList<PersistedEntity> GetAssociations(dynamic callerId);
  }

  public abstract class GenericPersistenceRepository<TEntityType> : IRepository<TEntityType> where TEntityType : class, new()
  {
   protected GenericPersistenceRepository(IUnitOfWork unitOfWork)
    {
      UnitOfWork = unitOfWork;
    }

    public abstract IUnitOfWork UnitOfWork { get; set; }
    public abstract int Count { get; }
    public abstract TEntityType Get(dynamic id);
    public abstract void Add(TEntityType persistedEntity);
  }
}