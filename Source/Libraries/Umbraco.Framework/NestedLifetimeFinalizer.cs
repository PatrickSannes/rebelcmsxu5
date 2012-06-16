using System;
using System.Collections.Concurrent;
using System.Threading;

using Umbraco.Framework.Diagnostics;

namespace Umbraco.Framework
{
    public class NestedLifetimeFinalizer : AbstractFinalizer
    {
        private ConcurrentQueue<Tuple<IDisposable, Action<IDisposable>>> _reference;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        protected ConcurrentQueue<Tuple<IDisposable, Action<IDisposable>>> Instances
        {
            get { return _reference ?? (_reference = new ConcurrentQueue<Tuple<IDisposable, Action<IDisposable>>>()); }
        }

        #region Overrides of AbstractFinalizer

        public override void AddFinalizerToScope<T>(T obj, Action<T> finalizer)
        {
            using (new WriteLockDisposable(_locker))
            {
                Action<IDisposable> castDelegate = x => finalizer.Invoke((T)x);
                Instances.Enqueue(new Tuple<IDisposable, Action<IDisposable>>(obj, castDelegate));
            }
        }
        
        public override void FinalizeScope()
        {
            using (new WriteLockDisposable(_locker))
            {
                LogHelper.TraceIfEnabled<NestedLifetimeFinalizer>("Finalizing {0} IDisposables", () => Instances.Count);
                foreach (var instance in Instances)
                {
                    // Call the provided finalizer delegate
                    instance.Item2.Invoke(instance.Item1);
                    // Dispose the item
                    instance.Item1.Dispose();
                }
            }
        }

        #endregion
    }
}