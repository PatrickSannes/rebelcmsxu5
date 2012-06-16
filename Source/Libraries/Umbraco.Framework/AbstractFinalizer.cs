using System;


namespace Umbraco.Framework
{
    public abstract class AbstractFinalizer : DisposableObject
    {
        public abstract void AddFinalizerToScope<T>(T obj, Action<T> finalizer) where T : IDisposable;
        public abstract void FinalizeScope();

        protected override void DisposeResources()
        {
            FinalizeScope();
        }
    }
}