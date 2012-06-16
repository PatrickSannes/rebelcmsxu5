using System;
using System.Collections.Generic;
using System.Data;
using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;

namespace Umbraco.Framework.EntityGraph.DataPersistence
{
    public abstract class EntityRepositoryReader
        :
            IEntityRepositoryReader
                <TypedEntityCollection<TypedEntity>, TypedEntityGraph<TypedEntityVertex>, TypedEntity,
                TypedEntityVertex>
    {
        #region Implementation of ISupportsProviderInjection

        public virtual IProviderManifest Provider { get; set; }

        #endregion

        #region Implementation of IRepositoryReader<out TypedEntityCollection<TypedEntity>,out TypedEntityGraph<TypedEntityVertex>,TypedEntity,out TypedEntityVertex>

        /// <summary>
        /// Gets the db context.
        /// </summary>
        /// <value>The db context.</value>
        public virtual IDbContext DbContext { get; private set; }

        /// <summary>
        /// Gets the specified IEntity.
        /// </summary>
        /// <param name="identifier">The identifier of the entity.</param>
        /// <remarks>By default, the implementation should not return ascendents or descendents.</remarks>
        /// <returns></returns>
        public abstract TypedEntity Get(IMappedIdentifier identifier);

        /// <summary>
        /// Gets the specified IEntity objects.
        /// </summary>
        /// <param name="identifiers">The identifiers of the entities.</param>
        /// <remarks>By default, the implementation should not return ascendents or descendents.</remarks>
        /// <returns></returns>
        public abstract TypedEntityCollection<TypedEntity> Get(IEnumerable<IMappedIdentifier> identifiers);

        /// <summary>
        /// Gets all IEntity objects in the repository.
        /// </summary>
        /// <returns></returns>
        public abstract TypedEntityGraph<TypedEntityVertex> GetAll();

        /// <summary>
        /// Gets the specified IEntity, including descdendents, to a given depth.
        /// </summary>
        /// <param name="identifier">The identifier of the IEntity.</param>
        /// <param name="traversalDepth">The traversal depth.</param>
        /// <returns></returns>
        public abstract TypedEntityVertex Get(IMappedIdentifier identifier, int traversalDepth);

        /// <summary>
        /// Gets the specified IEntity objects to a given depth.
        /// </summary>
        /// <param name="identifiers">The identifiers.</param>
        /// <param name="traversalDepth">The traversal depth.</param>
        /// <returns></returns>
        public abstract TypedEntityGraph<TypedEntityVertex> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth);

        /// <summary>
        /// Gets Ientity objects from the root to a given traversal depth.
        /// </summary>
        /// <param name="traversalDepth">The traversal depth.</param>
        /// <returns></returns>
        public abstract TypedEntityGraph<TypedEntityVertex> GetAll(int traversalDepth);

        /// <summary>
        /// Gets the parent IEntity for the given IEntity.
        /// </summary>
        /// <param name="forEntity">For entity.</param>
        /// <returns></returns>
        public abstract TypedEntityVertex GetParent(TypedEntity forEntity);

        /// <summary>
        /// Gets the parent IEntity for the given identifier.
        /// </summary>
        /// <param name="forEntityIdentifier">For entity identifier.</param>
        /// <returns></returns>
        public abstract TypedEntityVertex GetParent(IMappedIdentifier forEntityIdentifier);

        /// <summary>
        /// Gets all the descendents of the given IEntity as a list.
        /// </summary>
        /// <param name="forEntity">The ascendent entity.</param>
        /// <returns></returns>
        public abstract TypedEntityCollection<TypedEntity> GetDescendents(TypedEntity forEntity);

        /// <summary>
        /// Gets all the descendents of the IEntity with the given identifier as a list.
        /// </summary>
        /// <param name="forEntityIdentifier">The ascendent entity identifier.</param>
        /// <returns></returns>
        public abstract TypedEntityCollection<TypedEntity> GetDescendents(IMappedIdentifier forEntityIdentifier);

        /// <summary>
        /// Resolves the identifier in the format that this repository understands
        /// </summary>
        /// <typeparam name="TIdentifierValue">The type of the identifier value.</typeparam>
        /// <param name="value">The value of the object to resolve.</param>
        /// <returns></returns>
        public abstract IMappedIdentifier ResolveIdentifier<TIdentifierValue>(TIdentifierValue value);

        #endregion
    }
}