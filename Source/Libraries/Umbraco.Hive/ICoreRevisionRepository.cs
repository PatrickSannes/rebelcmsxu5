using System;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Hive
{
    public interface ICoreRevisionRepository<in TBaseEntity> 
        : IDisposable, ICoreReadonlyRevisionRepository<TBaseEntity> 
        where TBaseEntity : class, IVersionableEntity
    {
        void AddOrUpdate<TEntity>(Revision<TEntity> revision) where TEntity : class, TBaseEntity;
    }
}