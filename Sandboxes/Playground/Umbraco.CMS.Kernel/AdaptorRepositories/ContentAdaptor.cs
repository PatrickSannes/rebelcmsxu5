using System;
using System.Collections.Generic;
using Umbraco.Framework;
using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.DataPersistence;
using Umbraco.Framework.EntityGraph.Domain.EntityAdaptors;
using Umbraco.Framework.EntityGraph.Domain.EntityAdaptors.ObjectModel;
using Umbraco.Framework.EntityGraph.Domain.ObjectModel;

namespace Umbraco.CMS.Kernel.AdaptorRepositories
{
    internal class ContentAdaptor : IContentResolver
    {
        private IEntityRepositoryReader<TypedEntityCollection<ContentViewData>, TypedEntityGraph<Content>, ContentViewData, Content> _repositoryReader;

        public ContentAdaptor(IEntityRepositoryReader<TypedEntityCollection<ContentViewData>, TypedEntityGraph<Content>, ContentViewData, Content> repositoryReader)
        {
            this._repositoryReader = repositoryReader;
        }

        #region Implementation of ISupportsProviderInjection

        public virtual IProviderManifest Provider { get; set; }

        #endregion

        #region Implementation of IRepositoryReader<out TypedEntityAdaptorCollection<IContentEntity>,out TypedEntityAdaptorGraph<IContentVertex>,IContentEntity,out IContentVertex>

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
        public IContentEntity Get(IMappedIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the specified IEntity objects.
        /// </summary>
        /// <param name="identifiers">The identifiers of the entities.</param>
        /// <remarks>By default, the implementation should not return ascendents or descendents.</remarks>
        /// <returns></returns>
        public TypedEntityAdaptorCollection<IContentEntity> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all IEntity objects in the repository.
        /// </summary>
        /// <returns></returns>
        public TypedEntityAdaptorGraph<IContentVertex> GetAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the specified IEntity, including descdendents, to a given depth.
        /// </summary>
        /// <param name="identifier">The identifier of the IEntity.</param>
        /// <param name="traversalDepth">The traversal depth.</param>
        /// <returns></returns>
        public IContentVertex Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the specified IEntity objects to a given depth.
        /// </summary>
        /// <param name="identifiers">The identifiers.</param>
        /// <param name="traversalDepth">The traversal depth.</param>
        /// <returns></returns>
        public TypedEntityAdaptorGraph<IContentVertex> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets Ientity objects from the root to a given traversal depth.
        /// </summary>
        /// <param name="traversalDepth">The traversal depth.</param>
        /// <returns></returns>
        public TypedEntityAdaptorGraph<IContentVertex> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the parent IEntity for the given IEntity.
        /// </summary>
        /// <param name="forEntity">For entity.</param>
        /// <returns></returns>
        public IContentVertex GetParent(IContentEntity forEntity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the parent IEntity for the given identifier.
        /// </summary>
        /// <param name="forEntityIdentifier">For entity identifier.</param>
        /// <returns></returns>
        public IContentVertex GetParent(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all the descendents of the given IEntity as a list.
        /// </summary>
        /// <param name="forEntity">The ascendent entity.</param>
        /// <returns></returns>
        public TypedEntityAdaptorCollection<IContentEntity> GetDescendents(IContentEntity forEntity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets all the descendents of the IEntity with the given identifier as a list.
        /// </summary>
        /// <param name="forEntityIdentifier">The ascendent entity identifier.</param>
        /// <returns></returns>
        public TypedEntityAdaptorCollection<IContentEntity> GetDescendents(IMappedIdentifier forEntityIdentifier)
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
