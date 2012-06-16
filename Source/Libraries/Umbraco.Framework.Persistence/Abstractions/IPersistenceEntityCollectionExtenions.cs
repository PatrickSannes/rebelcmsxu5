using System;
using System.Linq;

namespace Umbraco.Framework.Persistence.Abstractions
{
    /// <summary>
    /// Extension methods for the <see cref="IPersistenceEntityCollection"/> class
    /// </summary>
    public static class IPersistenceEntityCollectionExtenions
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
        public static bool IsRegistered<TEntity, TAllowedType>(this IPersistenceEntityCollection<TEntity> coll) 
            where TAllowedType : IPersistenceEntity 
            where TEntity : IPersistenceEntity
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
        public static bool IsRegistered<TEntity>(this IPersistenceEntityCollection<TEntity> coll, System.Type type)
            where TEntity : IPersistenceEntity
        {
            if (coll == null)
                throw new ArgumentNullException("coll");
            if (type == null)
                throw new ArgumentNullException("type");

            if (!typeof(IPersistenceEntity).IsAssignableFrom(type))
                throw new ArgumentException(string.Format("Type {0} is not assignable from {1}", type.FullName, typeof(IPersistenceEntity).FullName));

            return coll.Any(x => x.GetType() == type);
        }
    }
}
