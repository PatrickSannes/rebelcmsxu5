using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Context;

namespace Umbraco.Tests.CoreAndFramework.Linq
{
    using Umbraco.Framework.Linq;

    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;

    public class NullQueryableDataSource : IQueryableDataSource
    {
        public QueryDescription GeneratedQueryDescription { get; set; }

        #region Implementation of IQueryableDataSource

        public T ExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            GeneratedQueryDescription = query;
            return default(T);
        }

        public T ExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            GeneratedQueryDescription = query;
            return default(T);
        }

        public IEnumerable<T> ExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            GeneratedQueryDescription = query;
            return Enumerable.Empty<T>();
        }

        #endregion

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get { throw new NotImplementedException(); } }

        #endregion
    }
}
