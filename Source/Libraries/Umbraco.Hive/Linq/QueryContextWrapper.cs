namespace Umbraco.Hive.Linq
{
    using Umbraco.Framework.Linq;

    public class QueryContextWrapper<T> : AbstractQueryContext<T>
    {
        public QueryContextWrapper(IQueryableDataSource queryableDataSource) : base(queryableDataSource)
        {
        }
    }
}
