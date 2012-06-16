using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoFramework;
using UmbracoFramework.EntityGraph.DataPersistence;
using UmbracoFramework.EntityGraph.Domain;
using UmbracoFramework.EntityGraph.Domain.Entity.Graph;
using UmbracoFramework.EntityGraph.Domain.ObjectModel;
using UmbracoFramework.EntityGraph.Synonyms;

namespace Prototyping.UmbracoFramework
{
    public class MockContentBroker : IContentResolver
    {
        #region Implementation of IContentResolver

        public IEntitySynonymVertex<IContentVertex> Get(IMappedIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEntitySynonymVertex<IContentVertex> Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public EntitySynonymGraph<IContentVertex> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public IEntityRepositoryReader RepositoryReader { get; set; }
        public IEntityRepositoryWriter RepositoryWriter { get; set; }

        #endregion
    }
}
