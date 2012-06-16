using System;
using System.Collections.Generic;

using Umbraco.Framework.DataManagement.Linq;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Persistence.XmlStore.DataManagement;
using Umbraco.Framework.Persistence.XmlStore.DataManagement.Linq;

namespace Umbraco.Framework.Persistence.XmlStore
{
    public class RepositoryReader : DisposableObject, IRepositoryReader
    {
        private readonly DataContext _dataContext;

        public RepositoryReader(DataContext dataContext)
        {
            _dataContext = dataContext;
        }


        #region Implementation of IRepositoryReadWriter

        #endregion

        public IQueryContext<TypedEntity> QueryContext
        {
            get { return new XDocumentQueryContext<TypedEntity>(_dataContext); }
        }

        public T GetEntity<T>(HiveId id) where T : AbstractEntity
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetEntityByRelationType<T>(AbstractRelationType relationType, HiveId sourceId, params RelationMetaDatum[] metaDatum)
            where T : class, IRelatableEntity
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetEntities<T>() where T : AbstractEntity
        {
            throw new NotImplementedException();
        }

        public bool Exists<T>(HiveId id) where T : AbstractEntity
        {
            throw new NotImplementedException();
        }

        public RevisionCollection<T> GetRevisions<T>(HiveId entityId, RevisionStatusType revisionStatusType) where T : TypedEntity
        {
            throw new NotImplementedException();
        }

        public Revision<T> GetRevision<T>(HiveId entityId, HiveId revisionId) where T : TypedEntity
        {
            throw new NotImplementedException();
        }

        public EntitySnapshot<T> GetEntitySnapshot<T>(HiveId entityUri, HiveId revisionId) where T : TypedEntity
        {
            throw new NotImplementedException();
        }

        public EntitySnapshot<T> GetLatestSnapshot<T>(HiveId entityUri, RevisionStatusType revisionStatusType) where T : TypedEntity
        {
            throw new NotImplementedException();
        }

        public T GetByPath<T>(HiveId path, AbstractRelationType relationType, RevisionStatusType statusType) where T : TypedEntity
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            if (_dataContext != null) _dataContext.Dispose();
        }
    }
}