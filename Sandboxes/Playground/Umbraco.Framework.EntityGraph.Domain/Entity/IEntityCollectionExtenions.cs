using System;
using System.Linq;

namespace Umbraco.Framework.EntityGraph.Domain.Entity
{
    /// <summary>
    /// Extension methods for the <see cref="IEntityCollection{T}"/> class
    /// </summary>
    public static class IEntityCollectionExtenions
    {
        /// <summary>
        /// Determines whether the type is registered
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TAllowedType">Entity type to lookup</typeparam>
        /// <param name="coll">The collection to check.</param>
        /// <returns>
        /// 	<c>true</c> if this type is allowed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRegistered<TEntity, TAllowedType>(this IEntityCollection<TEntity> coll) 
            where TAllowedType : IEntity 
            where TEntity : IEntity
        {
            if (coll == null)
                throw new ArgumentNullException("coll");
            return IsRegistered(coll, typeof(TAllowedType));
        }

        /// <summary>
        /// Determines whether the type is registered
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="coll">The collection to check.</param>
        /// <param name="type">The type to lookup.</param>
        /// <returns>
        /// 	<c>true</c> if this type is allowed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRegistered<TEntity>(this IEntityCollection<TEntity> coll, System.Type type)
            where TEntity : IEntity
        {
            if (coll == null)
                throw new ArgumentNullException("coll");
            if (type == null)
                throw new ArgumentNullException("type");

            if (!typeof(IEntity).IsAssignableFrom(type))
                throw new ArgumentException(string.Format("Type {0} is not assignable from {1}", type.FullName, typeof(IEntity).FullName));

            return coll.Any(x => x.GetType() == type);
        }
    }
}
