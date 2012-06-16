using System;
using System.Web;

namespace Umbraco.Framework
{
    public class HttpRequestScopedFinalizer : AbstractFinalizer
    {
        private NestedLifetimeFinalizer _helper = new NestedLifetimeFinalizer();
        private const string ContextKey = "HttpRequestScopedFinalizer-8h3mnfs";

        #region Overrides of AbstractFinalizer

        public override void AddFinalizerToScope<T>(T obj, Action<T> finalizer)
        {
            Helper.AddFinalizerToScope(obj, finalizer);
        }

        public override void FinalizeScope()
        {
            Helper.FinalizeScope();
            Helper.Dispose();
        }

        #endregion

        protected NestedLifetimeFinalizer Helper
        {
            get
            {
                return HttpContext.Current.Items[ContextKey] as NestedLifetimeFinalizer ?? 
                       ((HttpContext.Current.Items[ContextKey] = new NestedLifetimeFinalizer()) as NestedLifetimeFinalizer);
            }
        }
    }
}