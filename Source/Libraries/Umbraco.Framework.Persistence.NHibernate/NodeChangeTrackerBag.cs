using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate
{
    internal class NodeChangeTrackerBag : DisposableObject
    {
        private readonly HashSet<KeyValuePair<IReferenceByGuid, AbstractEntity>> _innerHash = new HashSet<KeyValuePair<IReferenceByGuid, AbstractEntity>>();
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public void Add(IReferenceByGuid rdbmsEntity, AbstractEntity stackEntity)
        {
            Mandate.ParameterNotNull(rdbmsEntity, "rdbmsEntity");
            Mandate.ParameterNotNull(stackEntity, "stackEntity");

            using (new WriteLockDisposable(Locker))
            {
                _innerHash.Add(new KeyValuePair<IReferenceByGuid, AbstractEntity>(rdbmsEntity, stackEntity));
            }
        }

        public IEnumerable<KeyValuePair<IReferenceByGuid, AbstractEntity>> FlushWhere(Func<KeyValuePair<IReferenceByGuid, AbstractEntity>, bool> predicate)
        {
            using (new WriteLockDisposable(Locker))
            {
                var items = _innerHash.Where(predicate).ToArray();
                foreach (var keyValuePair in items)
                {
                    _innerHash.Remove(keyValuePair);
                }
                return items;
            }
        }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _innerHash.Clear();
        }

        #endregion
    }

    internal class RevisionChangeTrackerBag : DisposableObject
    {
        private readonly List<KeyValuePair<IReferenceByGuid, object>> _innerList = new List<KeyValuePair<IReferenceByGuid, object>>();
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public void Add<T>(IReferenceByGuid rdbmsEntity, Revision<T> stackRevision) where T : TypedEntity
        {
            Mandate.ParameterNotNull(rdbmsEntity, "rdbmsEntity");
            Mandate.ParameterNotNull(stackRevision, "stackRevision");

            using (new WriteLockDisposable(Locker))
            {
                _innerList.Add(new KeyValuePair<IReferenceByGuid, object>(rdbmsEntity, stackRevision));
            }
        }

        public IEnumerable<KeyValuePair<IReferenceByGuid, object>> FlushWhere(Func<KeyValuePair<IReferenceByGuid, object>, bool> predicate)
        {
            using (new WriteLockDisposable(Locker))
            {
                var items = _innerList.Where(predicate).ToArray();
                foreach (var keyValuePair in items)
                {
                    _innerList.Remove(keyValuePair);
                }
                return items;
            }
        }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _innerList.Clear();
        }

        #endregion
    }
}