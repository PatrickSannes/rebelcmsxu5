namespace Umbraco.Framework.Linq
{
    using System.Linq;

    using System.Linq.Expressions;

    using Remotion.Linq;

    using Remotion.Linq.Parsing.Structure;
    using Umbraco.Framework.Expressions.Remotion;

    using Umbraco.Framework.Linq.ResultBinding;

    public abstract class AbstractQueryable<T> : QueryableBase<T>
    {
        protected AbstractQueryable(IQueryProvider provider)
            : base(provider)
        {

        }

        protected AbstractQueryable(IQueryParser queryParser, IQueryExecutor executor)
            : base(queryParser, executor)
        {
        }

        // This constructor is called indirectly by LINQ's query methods, just pass to base.
        protected AbstractQueryable(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        { }
    }

    // This class should be able to return the abstracted expression tree having translated a query
    // This class should accept a queryableDataSource in order to tell it to run the abstracted expression tree
    // and return the results
    public class Queryable<T> : AbstractQueryable<T>
    {
        private ObjectBinder _objectBinder;

        public Queryable(IQueryProvider provider)
            : base(provider)
        {

        }

        // This constructor is called indirectly by LINQ's query methods, just pass to base.
        public Queryable(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        { }

        public Queryable(IQueryExecutor executor)
            : this(executor, CustomQueryParser.CreateDefault())
        {
        }

        public Queryable(IQueryExecutor executor, IQueryParser queryParser)
            : base(queryParser ?? QueryParser.CreateDefault(), executor)
        {
        }

        public static ObjectBinder GetBinderFromAssembly()
        {
            return null;
            //var appDomainTypesBadAndDirtyBadBadBadMan = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
            ////TODO: Add caching, move to a better helper place
            //var mapperType =
            //    appDomainTypesBadAndDirtyBadBadBadMan
            //        .Where(x =>
            //               x.GetCustomAttributes(typeof(QueryResultMapperForAttribute), true)
            //                   .Cast<QueryResultMapperForAttribute>().Where(y => y.ResultType.Equals(typeof(T))).Any())
            //        .FirstOrDefault();

            //if (mapperType == null)
            //    throw new InvalidOperationException(string.Format("Cannot find an ObjectBinder for type {0}",
            //                                                      typeof(T).Name));

            //return Activator.CreateInstance(mapperType) as ObjectBinder;
        }
    }
}
