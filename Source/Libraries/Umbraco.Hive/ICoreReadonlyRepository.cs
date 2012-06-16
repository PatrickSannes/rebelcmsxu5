using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Hive
{
    public interface ICoreReadonlyRepository<in TBaseEntity>
        : ICoreReadonlyRelationsRepository, IDisposable 
        where TBaseEntity : class, IReferenceByHiveId
    {
        ///// <summary>
        ///// Gets a <see cref="TEntity"/> instance with the specified id.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of the entity.</typeparam>
        ///// <param name="id">The id.</param>
        ///// <returns>A <see cref="TEntity"/> matching the specified <paramref name="id"/>, or <code>null</code> if none is found.</returns>
        //TEntity Get<TEntity>(HiveId id) where TEntity : TBaseEntity;

        /// <summary>
        /// Gets a sequence of <see cref="TEntity"/> matching the specified ids.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="allOrNothing">If set to <c>true</c> all ids must match in order to return any <typeparamref name="TEntity"/> instances.</param>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        IEnumerable<TEntity> Get<TEntity>(bool allOrNothing, params HiveId[] ids) where TEntity : TBaseEntity;

        /// <summary>
        /// Gets all <see cref="TEntity"/> in the repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>An enumerable sequence of <see cref="TEntity"/> containing all that may be found in the repository. Returns a sequence of length 0 if none is found.</returns>
        IEnumerable<TEntity> GetAll<TEntity>() where TEntity : TBaseEntity;

        /// <summary>
        /// Identifies if a <see cref="TEntity"/> with matching <paramref name="id"/> can be found in this repository.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The id.</param>
        /// <returns><code>true</code> if the item with <paramref name="id"/> can be found, otherwise <code>false</code>.</returns>
        bool Exists<TEntity>(HiveId id) where TEntity : TBaseEntity;
    }
}