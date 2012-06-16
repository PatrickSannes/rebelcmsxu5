using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoFramework;
using UmbracoFramework.EntityGraph.DataPersistence;
using UmbracoFramework.EntityGraph.Domain;
using UmbracoFramework.EntityGraph.Domain.Entity;

namespace Prototyping.UmbracoFramework
{
    public class MockEntityRepository : IEntityRepositoryReader
    {
        #region Implementation of IEntityRepositoryReader

        public virtual IEntity Get(IMappedIdentifier identifier)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IEntity> Get(IEnumerable<IMappedIdentifier> identifiers)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IEntity> GetAll()
        {
            throw new NotImplementedException();
        }

        public virtual IEntity Get(IMappedIdentifier identifier, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IEntity> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IEntity> GetAll(int traversalDepth)
        {
            throw new NotImplementedException();
        }

        public virtual IEntity GetParent(IEntity forEntity)
        {
            throw new NotImplementedException();
        }

        public virtual IEntity GetParent(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IEntity> GetDescendents(IEntity forEntity)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<IEntity> GetDescendents(IMappedIdentifier forEntityIdentifier)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
