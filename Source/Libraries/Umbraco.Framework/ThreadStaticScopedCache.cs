using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;


namespace Umbraco.Framework
{
    public class ThreadStaticScopedCache : AbstractScopedCache
    {
        [ThreadStatic]
        private static ConcurrentDictionary<string, object> _reference;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        protected static ConcurrentDictionary<string, object> Instances
        {
            get { return _reference ?? (_reference = new ConcurrentDictionary<string, object>()); }
        }

        #region Overrides of AbstractScopedCache

        public override void AddOrChange(string key, Func<string, object> factory)
        {
            Instances.AddOrUpdate(key, factory, (e, u) => factory(e));
        }

        public override object GetOrCreate(string key, Func<object> callback)
        {
            Mandate.ParameterNotNull(Instances, "[ThreadStatic]_reference");
            Mandate.ParameterNotNull(callback, "callback");

            return Instances.GetOrAdd(key, x => callback.Invoke());
        }

        /// <summary>
        /// Removes any item from the cache that match the regex pattern
        /// </summary>
        /// <param name="pattern"></param>
        public override int InvalidateItems(string pattern)
        {
            var toRemove = (from i in Instances
                            let key = i.Key
                            where Regex.IsMatch(key, pattern)
                            select i.Key).ToList();

            foreach (var i in toRemove)
            {
                object output;
                Instances.TryRemove(i, out output);
            }
            return toRemove.Count;
        }

        public override void ScopeComplete()
        {
            //TODO: This lock shim might not be needed as we're using ConcurrentDictionary (APN)
            using (new WriteLockDisposable(_locker))
            {
                foreach (var item in Instances)
                {
                    if (item.Value != null && item.Value is IDisposable)
                    {
                        ((IDisposable)item.Value).Dispose();
                    }
                }
                Instances.Clear();
            }
        }

        #endregion
    }
}