using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.DataManagement.Linq;
using Umbraco.Framework.DataManagement.Linq.QueryModel;
using Umbraco.Framework.DataManagement.Linq.ResultBinding;

namespace Umbraco.Framework.Persistence.XmlStore.DataManagement.Linq
{
    public class XDocumentQueryableDataSource : IQueryableDataSource
    {
        private readonly Func<object, SourceFieldBinder> _fieldBinderFactory;
        private readonly XDocument _store;

        public XDocumentQueryableDataSource()
            : this(XDocument.Load("test.xml"), XElementSourceFieldBinder.New)
        {}

        public XDocumentQueryableDataSource(XDocument store, Func<object, SourceFieldBinder> fieldBinderFactory) : this(fieldBinderFactory)
        {
            _store = store;
        }

        protected XDocumentQueryableDataSource(Func<object, SourceFieldBinder> fieldBinderFactory)
        {
            _fieldBinderFactory = fieldBinderFactory;
        }

        public T ExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            return default(T);
        }

        public T ExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            return default(T);            
        }

        public IEnumerable<T> ExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var sourceElement = _store.Descendants().Where(x => IdMatches(x, query.From.StartId)).FirstOrDefault();
            var direction = query.From.HierarchyScope;

            var expression = new XElementCriteriaVisitor().Visit(query.Criteria);

            var allMatches = _store.Descendants().Where(expression.Compile());

            IEnumerable<XElement> results = Enumerable.Empty<XElement>();
            switch (direction)
            {
                case HierarchyScope.AllOrNone:
                    results = _store.Descendants().Where(x => allMatches.Contains(x));
                    break;
                case HierarchyScope.Ancestors:
                    results = sourceElement.Ancestors().Where(x => allMatches.Contains(x));
                    break;
                case HierarchyScope.AncestorsOrSelf:
                    results = sourceElement.AncestorsAndSelf().Where(x => allMatches.Contains(x));
                    break;
                case HierarchyScope.Descendents:
                    results = sourceElement.Descendants().Where(x => allMatches.Contains(x));
                    break;
                case HierarchyScope.DescendentsOrSelf:
                    results = sourceElement.DescendantsAndSelf().Where(x => allMatches.Contains(x));
                    break;
                case HierarchyScope.Parent:
                    results = new[] { sourceElement.Ancestors().Where(x => allMatches.Contains(x)).Last() };
                    break;
                case HierarchyScope.Children:
                    results = sourceElement.Elements().Where(x => allMatches.Contains(x));
                    break;
            }

            switch (query.ResultFilter.ResultFilterType)
            {
                case ResultFilterType.Take:
                    results = results.Take(query.ResultFilter.SelectorArgument);
                    break;
            }

            if (typeof(T).IsAssignableFrom(query.ResultFilter.ResultType))
            {
                return results.Select(xElement => objectBinder.Execute(_fieldBinderFactory.Invoke(xElement))).Cast<T>();
            }

            return Enumerable.Empty<T>();
        }

        private static bool IdMatches(XElement xElement, string id)
        {
            return xElement.Attributes("Id").Any() && string.Equals((string)xElement.Attribute("Id"), id, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
