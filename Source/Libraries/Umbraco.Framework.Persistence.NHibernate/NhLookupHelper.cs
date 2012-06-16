using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.NHibernate
{
    public class NhLookupHelper : AbstractLookupHelper
    {
        //private readonly ISessionFactory _nhSessionFactory;
        private readonly EntityRepositoryFactory _dataContextFactory;
        private readonly Dictionary<Tuple<Guid, Type>, IReferenceByGuid> _contextLookupCache = new Dictionary<System.Tuple<Guid, Type>, IReferenceByGuid>();
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        #region Overrides of AbstractLookupHelper

        /// <summary>
        /// Constructor used when an ISessionFactory is created by the DataContextFactory
        /// </summary>
        /// <param name="dataContextFactory"></param>
        public NhLookupHelper(EntityRepositoryFactory dataContextFactory)
        {
            //TODO: casting can be avoided when i figure out how to register the same instance as multiple types in our IoC framework
            _dataContextFactory = dataContextFactory;
        }

        public override T Lookup<T>(HiveId id)
        {
            // First try to get the object from the current NH session by id
            var session = _dataContextFactory.NhDependencyHelper.FactoryHelper.GetNHibernateSession(false);
            var item = session.Get<T>((Guid)id.Value);
            if (item != null)
            {
                return item;
            }
            LogHelper.TraceIfEnabled<NhLookupHelper>("Was asked for {0} with id {1} but found none", () => typeof(T).Name, () => id);

            //// If that didn't work, see if we have got a cached instance matching the type and id
            //var key = GetKey<T>(id.AsGuid);
            //IReferenceByGuid itemCast;
            //using (new WriteLockDisposable(_locker))
            //    _contextLookupCache.TryGetValue(key, out itemCast);
            //return itemCast as T;
            return null;
        }

        public override void CacheCreation<T>(T item)
        {
            // This method is ignored for now pending a thread-safe TypeMapperContext class
            using (new WriteLockDisposable(_locker))
            {
                var key = GetKey<T>(item.Id);

                if (!_contextLookupCache.ContainsKey(key))
                    _contextLookupCache.Add(key, item);
            }
        }

        private static System.Tuple<Guid, Type> GetKey<T>(Guid id) where T : IReferenceByGuid
        {
            return new System.Tuple<Guid, Type>(id, typeof(T));
        }

        #endregion
    }
}
