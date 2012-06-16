using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

using Umbraco.Framework.Context;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.DataManagement.Linq;
using Umbraco.Framework.DataManagement.Linq.QueryModel;
using Umbraco.Framework.DataManagement.Linq.ResultBinding;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Persistence.XmlStore.DataManagement.Linq;

namespace Umbraco.Framework.Persistence.XmlStore.DataManagement
{
    public class DataContext : AbstractDataContext, IQueryableDataSource
    {
        private readonly ConcurrentStack<Transaction> _openTransactions = new ConcurrentStack<Transaction>();
        private readonly string _path;
        private readonly XDocument _xmlDoc;
        private readonly Func<object, SourceFieldBinder> _fieldBinderFactory;

        public DataContext(IHiveProvider hiveProvider, XDocument xmlDoc, string path, Func<object, SourceFieldBinder> fieldBinderFactory)
            : base(hiveProvider)
        {
            _path = path;
            _xmlDoc = xmlDoc;
            _fieldBinderFactory = fieldBinderFactory;
            FrameworkContext = hiveProvider.FrameworkContext;
        }

        public XDocument XmlDoc
        {
            get
            {
                this.CheckThrowObjectDisposed(base.IsDisposed, "XmlStore...DataContext:XmlDoc");
                return _xmlDoc;
            }
        }


        /// <summary>
        /// Begins a transaction.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public override ITransaction BeginTransaction()
        {
            this.CheckThrowObjectDisposed(base.IsDisposed, "XmlStore...DataContext:BeginTransaction");
            var tranny = new Transaction(XmlDoc, _path);
            _openTransactions.Push(tranny);
            return tranny;
        }

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        /// <remarks></remarks>
        public override ITransaction CurrentTransaction
        {
            get
            {
                Transaction wrapper = null;
                _openTransactions.TryPeek(out wrapper);
                return wrapper;
            }
        }

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; private set; }

        /// <summary>
        /// Flushes this instance. This action is optional, for example if a DataContext should be flushed multiple times before committing or rolling back a transaction.
        /// </summary>
        /// <remarks></remarks>
        public override void Flush()
        {
            this.CheckThrowObjectDisposed(base.IsDisposed, "XmlStore...DataContext:Flush");
            XmlDoc.Save(_path); // The XmlStore transaction method is to backup at the start of a transaction, and rollback overwrites from the backup, so just save to normal path
        }


        internal XElement FindRootItem()
        {
            return FindElementByExpression("/root").Descendants().First();
        }


        internal XElement FindItemByUrl(string url)
        {
            var slashChar = new[] { '/' };
            var periodChar = new[] { '.' };

            string[] urlParts = url.Split(slashChar, StringSplitOptions.RemoveEmptyEntries);

            if (urlParts.Length == 0) return FindRootItem();

            XElement xmlElement = FindPageByUrlName(urlParts);
            if (xmlElement != null)
            {
                return FindWithUmbracoRedirect(xmlElement);
            }

            xmlElement = FindPageByUrlAlias(url.TrimStart(slashChar).Split(periodChar)[0].TrimEnd(slashChar));
            if (xmlElement != null)
            {
                return FindWithUmbracoRedirect(xmlElement);
            }

            xmlElement = FindPageById(urlParts.Last().Split(periodChar)[0]);

            return xmlElement != null ? FindWithUmbracoRedirect(xmlElement) : null;
        }

        internal XElement FindPageById(string pageId)
        {
            int result = 0;
            if (int.TryParse(pageId, out result))
            {
                string expression = "/root//*[string-length(@id)>0 and @id=" + pageId + "]";
                return FindElementByExpression(expression);
            }
            return null;
        }

        internal XElement FindElementByExpression(string expression)
        {
            return FindElementsByExpression(expression).FirstOrDefault();
        }

        internal IEnumerable<XElement> FindElementsByExpression(string expression)
        {
            return ((IEnumerable)XmlDoc.XPathEvaluate(expression)).Cast<XElement>();
        }

        internal XElement FindPageByUrlAlias(string alias)
        {
            string expression = string.Concat(
                "/root//*[string-length(@id)>0 and data[@alias='umbracoUrlAlias']/text()=\"", alias, "\"]");
            return FindElementByExpression(expression);
        }

        internal XElement FindPageByUrlName(string[] urlParts)
        {
            string expression = "/root/";
            if (urlParts.Length != 0)
            {
                expression = urlParts
                    .Aggregate(
                        expression,
                        (current, part) => current + string.Format("/*[@urlName=\"{0}\"]", part.Split(new[] { '.' })[0]));
                try
                {
                    return FindElementByExpression(expression);
                }
                catch (ArgumentNullException exception)
                {
                    throw new ArgumentNullException(
                        "Error evaluating XPath statement: \"" + expression + "\" against URL: \"" +
                        string.Join("/", urlParts) + "\"", exception);
                }
            }
            return null;
        }

        internal XElement FindWithUmbracoRedirect(XElement pageXml)
        {
            IEnumerable<XElement> redirectElements = FindElementsByExpression("./data[@alias='umbracoRedirect']");

            if (redirectElements.Count() > 0)
            {
                var elementAsString = (string)redirectElements.First();
                if (string.IsNullOrEmpty(elementAsString))
                {
                    return pageXml;
                }
                XElement element = FindPageById(elementAsString);
                if (element != null)
                {
                    pageXml = element;
                }
            }
            return pageXml;
        }


        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            this.CheckThrowObjectDisposed(base.IsDisposed, "XmlStore...DataContext:Close");

            foreach (Transaction openTransaction in _openTransactions.Where(openTransaction => !openTransaction.WasCommitted))
            {
                openTransaction.Rollback();
            }
        }

        public T ExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            throw new NotImplementedException();
        }

        public T ExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            // TODO: Evaluate query here because it means we can avoid running a whole query if all we need is the first etc.
            return ExecuteMany<T>(query, objectBinder).FirstOrDefault();
        }

        public IEnumerable<T> ExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var sourceElement = _xmlDoc.Descendants().Where(x => IdMatches(x, query.From.StartId)).FirstOrDefault();
            var direction = query.From.HierarchyScope;

            var expression = new XElementCriteriaVisitor().Visit(query.Criteria);

            var allMatches = _xmlDoc.Descendants().Where(expression.Compile());

            var results = Enumerable.Empty<XElement>();
            switch (direction)
            {
                case HierarchyScope.AllOrNone:
                    results = _xmlDoc.Descendants().Where(x => allMatches.Contains(x));
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

            return typeof(T).IsAssignableFrom(query.ResultFilter.ResultType)
                       ? results.Select(xElement => objectBinder.Execute(_fieldBinderFactory.Invoke(xElement))).Cast<T>()
                       : Enumerable.Empty<T>();
        }

        private static bool IdMatches(XElement xElement, string id)
        {
            return xElement.Attributes("id").Any() && string.Equals((string)xElement.Attribute("id"), id, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}