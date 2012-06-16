namespace Umbraco.Framework.Linq
{
    using System.Collections.Generic;

    using Umbraco.Framework.Context;

    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;

    public interface IQueryableDataSource : IRequiresFrameworkContext
    {
        T ExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder);
        T ExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder);
        IEnumerable<T> ExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder);
    }
}