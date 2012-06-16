using System;
using Umbraco.Framework;

namespace Umbraco.Hive
{
    public interface ICoreRepository<in TBaseEntity>
        : IDisposable, ICoreRelationsRepository, ICoreReadonlyRepository<TBaseEntity> 
        where TBaseEntity : class, IReferenceByHiveId
    {
        void AddOrUpdate(TBaseEntity entity);
        void Delete<T>(HiveId id) where T : TBaseEntity;
    }
}