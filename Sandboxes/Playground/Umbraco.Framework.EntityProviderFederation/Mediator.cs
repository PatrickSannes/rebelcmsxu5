using System;
using Umbraco.Framework.EntityGraph.DataPersistence;
using Umbraco.Framework.EntityGraph.Domain.EntityAdaptors;
using Umbraco.Framework.EntityProviderFederation.Resources;


namespace Umbraco.Framework.EntityProviderFederation
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Mediator
    {
        protected Mediator()
        {
        }

        /// <summary>
        /// Gets the content resolver.
        /// </summary>
        /// <param name="alias">The alias of the content resolver to find.</param>
        /// <returns></returns>
        public virtual IContentResolver GetContentResolver(string alias)
        {
            throw new ArgumentException(string.Format(Messages.IContentResolver_AliasNotFound, alias));
        }

        /// <summary>
        /// Gets the content resolver.
        /// </summary>
        /// <param name="providerManifest">The provider manifest.</param>
        /// <returns></returns>
        public virtual IContentResolver GetContentResolver(IProviderManifest providerManifest)
        {
            return GetContentResolver(providerManifest.MappingAlias);
        }

        /// <summary>
        /// Gets the entity repository reader.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public virtual EntityRepositoryReader GetEntityRepositoryReader(string alias)
        {
            throw new ArgumentException(string.Format(Messages.EntityRepositoryReader_AliasNotFound, alias));
        }

        /// <summary>
        /// Gets the entity repository reader.
        /// </summary>
        /// <param name="providerManifest">The provider manifest.</param>
        /// <returns></returns>
        public virtual EntityRepositoryReader GetEntityRepositoryReader(IProviderManifest providerManifest)
        {
            return GetEntityRepositoryReader(providerManifest.MappingAlias);
        }
    }
}