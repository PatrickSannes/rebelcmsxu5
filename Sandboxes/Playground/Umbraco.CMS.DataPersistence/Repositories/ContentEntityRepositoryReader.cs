using System;
using System.Collections.Generic;
using Umbraco.Framework;
using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.Domain.EntityAdaptors;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;

namespace Umbraco.CMS.DataPersistence.Repositories
{
    public class ContentEntityRepositoryReader
        : IRepositoryReader<TypedEntityCollection<ContentViewData>, TypedEntityGraph<Content>, ContentViewData, Content>
    {
        #region Implementation of ISupportsProviderInjection

        public IProviderManifest Provider { get; set; }

        #endregion

        #region Implementation of IRepositoryReader<out TypedEntityCollection<ContentViewData>,out TypedEntityGraph<Content>,ContentViewData,out Content>

        /// <summary>
        /// Gets the db context.
        /// </summary>
        /// <value>The db context.</value>
        public IDbContext DbContext { get; private set; }

        /// <summary>
        /// Gets the specified IEntity.
        /// </summary>
        /// <param name="identifier">The identifier of the entity.</param>
        /// <remarks>By default, the implementation should not return ascendents or descendents.</remarks>
        /// <returns></returns>
        public ContentViewData Get(IMappedIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the specified IEntity objects.
        /// </summary>
        /// <param name="identifiers">The identifiers of the entities.</param>
        /// <remarks>By default, the implementation should not return ascendents or descendents.</remarks>
        /// <returns></returns>
        public TypedEntityCollection<ContentViewData> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all IEntity objects in the repository.
        /// </summary>
        /// <returns></returns>
        public TypedEntityGraph<Content> GetAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the specified IEntity, including descdendents, to a given depth.
        /// </summary>
        /// <param name="identifier">The identifier of the IEntity.</param>
        /// <param name="traversalDepth">The traversal depth.</param>
        /// <returns></returns>
        public Content Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the specified IEntity objects to a given depth.
        /// </summary>
        /// <param name="identifiers">The identifiers.</param>
        /// <param name="traversalDepth">The traversal depth.</param>
        /// <returns></returns>
        public TypedEntityGraph<Content> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets Ientity objects from the root to a given traversal depth.
        /// </summary>
        /// <param name="traversalDepth">The traversal depth.</param>
        /// <returns></returns>
        public TypedEntityGraph<Content> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the parent IEntity for the given IEntity.
        /// </summary>
        /// <param name="forEntity">For entity.</param>
        /// <returns></returns>
        public Content GetParent(ContentViewData forEntity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the parent IEntity for the given identifier.
        /// </summary>
        /// <param name="forEntityIdentifier">For entity identifier.</param>
        /// <returns></returns>
        public Content GetParent(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all the descendents of the given IEntity as a list.
        /// </summary>
        /// <param name="forEntity">The ascendent entity.</param>
        /// <returns></returns>
        public TypedEntityCollection<ContentViewData> GetDescendents(ContentViewData forEntity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all the descendents of the IEntity with the given identifier as a list.
        /// </summary>
        /// <param name="forEntityIdentifier">The ascendent entity identifier.</param>
        /// <returns></returns>
        public TypedEntityCollection<ContentViewData> GetDescendents(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public IMappedIdentifier ResolveIdentifier<TIdentifierValue>(TIdentifierValue value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
