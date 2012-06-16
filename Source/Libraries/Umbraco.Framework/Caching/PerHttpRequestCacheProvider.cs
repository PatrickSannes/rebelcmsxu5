using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Caching
{
    using System.Threading;
    using System.Web;

    public class PerHttpRequestCacheProvider : AbstractCacheProvider
    {
        private readonly HttpContextBase _context;

        public PerHttpRequestCacheProvider()
        {
            _context = new HttpContextWrapper(HttpContext.Current);
        }

        public PerHttpRequestCacheProvider(HttpContextBase context)
        {
            _context = context;
        }

        public static PerHttpRequestCacheProvider Default
        {
            get
            {
                return new PerHttpRequestCacheProvider();
            }
        }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            foreach (var result in _context.Items.Values.OfType<IDisposable>())
            {
                result.Dispose();
            }
        }

        #endregion

        #region Overrides of AbstractCacheProvider

        public override CacheModificationResult AddOrChange<T>(CacheKey key, CacheValueOf<T> cacheObject)
        {
            using (new WriteLockDisposable(ThreadLocker))
            {
                var exists = Get<T>(key) != null;
                if (exists)
                {
                    _context.Items[key] = cacheObject;
                    return new CacheModificationResult(true, false);
                }
                _context.Items.Add(key, cacheObject);
            }
            return new CacheModificationResult(false, true);
        }

        public override void Clear()
        {
            _context.Items.Keys.OfType<CacheKey>().ToArray().ForEach(x => _context.Items.Remove(x));
        }

        public override bool Remove(CacheKey key)
        {
            if (!_context.Items.Contains(key)) return false;
            _context.Items.Remove(key);
            return true;
        }

        public override IEnumerable<CacheKey<T>> GetKeysMatching<T>(Func<T, bool> predicate)
        {
            return _context.Items.Keys.OfType<CacheKey<T>>().Where(k => predicate.Invoke(k.Original));
        }

        protected override CacheEntry<T> PerformGet<T>(CacheKey key)
        {
            var item = _context.Items[key] as ICacheValueOf<T>;
            return item != null ? new CacheEntry<T>(key, item) : null;
        }

        #endregion
    }
}
