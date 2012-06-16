using System;
using System.Xml.Linq;

using Umbraco.Framework.Context;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Persistence.XmlStore.DataManagement.Linq;

namespace Umbraco.Framework.Persistence.XmlStore.DataManagement
{
    public class DataContextFactory : AbstractDataContextFactory
    {
        private readonly string _xmlPath;
        private readonly Guid _contextId;
        public DataContextFactory(string xmlPath)
        {
            _xmlPath = xmlPath;
            _contextId = Guid.NewGuid();
        }

        protected override AbstractDataContext InstantiateDataContext(IHiveProvider hiveProvider)
        {
            return new DataContext(hiveProvider, XDocument.Load(_xmlPath), _xmlPath, XElementSourceFieldBinder.New);
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            return;
        }
    }
}