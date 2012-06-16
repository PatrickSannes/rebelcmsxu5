namespace Umbraco.Framework.Linq
{
    using System.Linq;

    using Umbraco.Framework.Linq.ResultBinding;

    public abstract class AbstractQueryContext<T> : IQueryContext<T>
    {
        public IQueryableDataSource QueryableDataSource { get; set; }

        protected AbstractQueryContext(IQueryableDataSource queryableDataSource)
        {
            QueryableDataSource = queryableDataSource;
        }

        public T RenderItem { get; set; }
        public virtual IQueryable<T> Query() { return new Queryable<T>(new Executor(QueryableDataSource, Queryable<T>.GetBinderFromAssembly())); }
        public virtual IQueryable<TSpecific> Query<TSpecific>() { return new Queryable<TSpecific>(new Executor(QueryableDataSource, Queryable<TSpecific>.GetBinderFromAssembly())); }
        public virtual IQueryable<TSpecific> Query<TSpecific>(ObjectBinder objectBinder) { return new Queryable<TSpecific>(new Executor(QueryableDataSource, objectBinder)); }
    }
}
