using System.Diagnostics.Contracts;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Dynamic;

namespace Umbraco.Framework.EntityGraph.Domain
{
    /// <summary>
    /// Extensions for the IEntity and certain subtypes
    /// </summary>
    public static class IEntityExtensions
    {
        /// <summary>
        /// Converts an instance of <see cref="ITypedEntity"/> to a dynamic object
        /// </summary>
        /// <param name="entity">The entity to convert.</param>
        /// <returns>An instance of <see cref="DynamicTypedEntity"/> which is a dynamic object</returns>
        public static dynamic AsDynamic(this ITypedEntity entity)
        {
            Contract.Requires(entity != null);
            return new DynamicTypedEntity(entity);
        }

        /// <summary>
        /// Converts an instance of <see cref="IEntity"/> to a dynamic object
        /// </summary>
        /// <param name="entity">The entity to convert.</param>
        /// <returns>An instance of <see cref="DynamicEntity"/> which is a dynamic object</returns>
        public static dynamic AsDynamic(this IEntity entity)
        {
            Contract.Requires(entity != null);
            return new DynamicEntity(entity);
        }
    }
}