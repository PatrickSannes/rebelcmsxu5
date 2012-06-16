using System;
using System.Collections.Generic;
using System.Threading;

using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Persistence.XmlStore.DataManagement;

namespace Umbraco.Framework.Persistence.XmlStore
{
    public class RepositoryReadWriter : RepositoryReader, IRepositoryReadWriter
    {
        private readonly DataContext _dataContext;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public RepositoryReadWriter(DataContext dataContext) : base(dataContext)
        {
        }


        #region Implementation of IRepositoryReadWriter

        public void AddOrUpdate(AbstractEntity persistedEntity)
        {
            using (new WriteLockDisposable(_locker))
            {
                throw new NotImplementedException();
            }
        }

        public void Delete<T>(HiveId entityId)
        {
            throw new NotImplementedException();
        }

        public void AddOrUpdate<T>(Revision<T> revision) where T : TypedEntity
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
