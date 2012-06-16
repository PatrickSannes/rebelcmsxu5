namespace Umbraco.Framework.Linq
{
    using System.Collections.Generic;

    using Remotion.Linq;

    using Umbraco.Framework.Linq.CriteriaGeneration.ExpressionVisitors;

    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;

    public class Executor : IQueryExecutor
    {
        private readonly ObjectBinder _binder;

        public Executor(IQueryableDataSource queryableDataSource, ObjectBinder binder)
        {
            _binder = binder;
            QueryableDataSource = queryableDataSource;
        }

        protected IQueryableDataSource QueryableDataSource { get; set; }

        internal QueryDescription LastGeneratedDescription { get; set; }

        protected QueryDescription GetQueryDescription(Remotion.Linq.QueryModel queryModel)
        {
            return (LastGeneratedDescription = QueryModelVisitor.FromQueryModel(queryModel));
        }

        public virtual T ExecuteScalar<T>(Remotion.Linq.QueryModel queryModel)
        {
            return QueryableDataSource.ExecuteScalar<T>(GetQueryDescription(queryModel), _binder);
        }

        public virtual T ExecuteSingle<T>(Remotion.Linq.QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return QueryableDataSource.ExecuteSingle<T>((GetQueryDescription(queryModel)), _binder);
            // return returnDefaultWhenEmpty ? ExecuteCollection<T>(queryModel).SingleOrDefault() : ExecuteCollection<T>(queryModel).Single();
        }

        public virtual IEnumerable<T> ExecuteCollection<T>(Remotion.Linq.QueryModel queryModel)
        {
            return QueryableDataSource.ExecuteMany<T>((GetQueryDescription(queryModel)), _binder);
        }
    }
}
