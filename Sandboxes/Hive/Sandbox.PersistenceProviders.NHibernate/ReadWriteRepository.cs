using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using NHibernate;
using Sandbox.Hive.Domain.DataManagement;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;
using Sandbox.Hive.Foundation;
using Sandbox.PersistenceProviders.NHibernate.DataManagement;

namespace Sandbox.PersistenceProviders.NHibernate
{
  public class ReadWriteRepository : GenericPersistenceRepository<PersistedEntity>, IPersistenceRepository
  {
    //private ISession _session;

    public ReadWriteRepository() : base(DependencyResolver.Current.Resolve<IUnitOfWork>())
    {}



    //public ReadWriteRepository(ISession session) : this()
    //{
    //  _session = session;
    //}

    private DataContext InternalDataContext
    {
      get { return UnitOfWork.DataContext as DataContext; }
    }

    public string RepositoryKey { get; set; }

    public IList<PersistedEntity> GetAssociations(dynamic callerId)
    {
      Contract.Assert(callerId != null);

      //Convert incoming ID to string since the interface requires that we have a dynamic param
      //but we can't query over NHibernate using dynamics in an expression tree
      //TODO: Centralise string conversion of dynamic IDs to ensure normalisation
      string callerIdVvalue = callerId.ToString();

      return
        InternalDataContext.NhibernateSession.QueryOver<PersistedEntity>().SelectList(
          a => a.Select(b => b.ParentKey == callerIdVvalue)).List();
    }

    public override IUnitOfWork UnitOfWork { get; set; }

    public override int Count { get { return InternalDataContext.NhibernateSession.QueryOver<PersistedEntity>().RowCount(); } }

    public override PersistedEntity Get(dynamic id)
    {
      return UnitOfWork.DataContext.Get<PersistedEntity>(id);
    }

    public override void Add(PersistedEntity persistedEntity)
    {
      UnitOfWork.DataContext.Save(persistedEntity);
    }
  }
}
