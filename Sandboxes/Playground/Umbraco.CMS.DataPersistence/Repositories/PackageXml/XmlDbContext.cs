using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Data.PersistenceSupport;

namespace Umbraco.CMS.DataPersistence.Repositories.PackageXml
{
    public sealed class XmlDbContext : IDbContext
    {
        #region IDbContext Members

        public void CommitChanges()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDbTransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public System.Data.IDbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
