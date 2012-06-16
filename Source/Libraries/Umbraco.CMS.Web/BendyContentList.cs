namespace Umbraco.Cms.Web
{
    using Umbraco.Cms.Web.Model;
    using Umbraco.Framework;
    using Umbraco.Framework.Context;
    using Umbraco.Framework.Dynamics;
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.Model.Constants;
    using Umbraco.Framework.TypeMapping;
    using Umbraco.Hive;
    using Umbraco.Hive.RepositoryTypes;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using global::System.Linq.Expressions;

    public abstract class QueryWrapper<T> : IQueryable<T>
    {
        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public abstract IEnumerator<T> GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IQueryable

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </returns>
        public abstract Expression Expression { get; }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
        /// </returns>
        public abstract Type ElementType { get; }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
        /// </returns>
        public abstract IQueryProvider Provider { get; }

        #endregion
    }

    public class DynamicContentList : QueryWrapper<dynamic>
    {
        private readonly HiveId _sourceId;
        private IHiveManager _hiveManager;
        private object _defaultValueIfNull;
        private IEnumerable<HiveId> _containedIds;
        private IQueryable<TypedEntity> _queryProvider;

        public DynamicContentList(HiveId sourceId, IHiveManager hiveManager, object defaultValueIfNull, IEnumerable<HiveId> containedIds)
            : this(sourceId, hiveManager, defaultValueIfNull, containedIds, hiveManager.Query<TypedEntity, IContentStore>().OfRevisionType(FixedStatusTypes.Published).InIds(containedIds.ToArray()))
        { }

        public DynamicContentList(HiveId sourceId, IHiveManager hiveManager, object defaultValueIfNull, IEnumerable<HiveId> containedIds, IQueryable<TypedEntity> chainedExpression)
        {
            _containedIds = containedIds;
            _defaultValueIfNull = defaultValueIfNull;
            _hiveManager = hiveManager;
            _sourceId = sourceId;
            _queryProvider = chainedExpression;

            if (!containedIds.Any())
            {
                _queryProvider = Enumerable.Empty<Content>().AsQueryable();
            }
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override IEnumerator<dynamic> GetEnumerator()
        {
            return new DynamicContentEnumerator(_queryProvider.GetEnumerator(), _hiveManager, _defaultValueIfNull);
        }

        #endregion

        #region Implementation of IQueryable

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </returns>
        public override Expression Expression
        {
            get
            {
                return _queryProvider.Expression;
            }
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
        /// </returns>
        public override Type ElementType
        {
            get
            {
                return _queryProvider.ElementType;
            }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
        /// </returns>
        public override IQueryProvider Provider
        {
            get
            {
                return _queryProvider.Provider;
            }
        }

        #endregion

        public int Count()
        {
            return Queryable.Count(this);
        }

        public bool Any()
        {
            return Queryable.Any(this);
        }

        public bool Any(string predicate, params object[] substitutions)
        {
            return _queryProvider.Where<Content>(predicate, substitutions).Any();
        }

        public int Count(string predicate, params object[] substitutions)
        {
            return _queryProvider.Where<Content>(predicate, substitutions).Count();
        }

        public DynamicContentList Where(string predicate, params object[] substitutions)
        {
            return new DynamicContentList(_sourceId, _hiveManager, _defaultValueIfNull, _containedIds, _queryProvider.Where<Content>(predicate, substitutions));
        }

        public dynamic First()
        {
            return BendIfPossible(_queryProvider.First());
        }

        public dynamic FirstOrDefault()
        {
            return BendIfPossible(_queryProvider.FirstOrDefault());
        }

        public dynamic First(string predicate, params object[] substitutions)
        {
            return BendIfPossible(_queryProvider.Where<Content>(predicate, substitutions).First());
        }

        public dynamic FirstOrDefault(string predicate, params object[] substitutions)
        {
            return BendIfPossible(_queryProvider.Where<Content>(predicate, substitutions).FirstOrDefault());
        }

        public dynamic Single()
        {
            return BendIfPossible(_queryProvider.Single());
        }

        public dynamic SingleOrDefault()
        {
            return BendIfPossible(_queryProvider.SingleOrDefault());
        }

        public dynamic Single(string predicate, params object[] substitutions)
        {
            return BendIfPossible(_queryProvider.Where<Content>(predicate, substitutions).Single());
        }

        public dynamic SingleOrDefault(string predicate, params object[] substitutions)
        {
            return BendIfPossible(_queryProvider.Where<Content>(predicate, substitutions).SingleOrDefault());
        }

        public dynamic Last()
        {
            return BendIfPossible(_queryProvider.ToList().Last()); // Using ToList here until code coverage on generating Last query is better
        }

        public dynamic LastOrDefault()
        {
            return BendIfPossible(_queryProvider.ToList().LastOrDefault());
        }

        public dynamic Last(string predicate, params object[] substitutions)
        {
            return BendIfPossible(_queryProvider.Where<Content>(predicate, substitutions).ToList().Last());
        }

        public dynamic LastOrDefault(string predicate, params object[] substitutions)
        {
            return BendIfPossible(_queryProvider.Where<Content>(predicate, substitutions).ToList().LastOrDefault());
        }

        public dynamic ElementAt(int index)
        {
            return BendIfPossible(_queryProvider.ToList().ElementAt(index));
        }

        public dynamic ElementAtOrDefault(int index)
        {
            return BendIfPossible(_queryProvider.ToList().ElementAtOrDefault(index));
        }

        public IList<dynamic> ToList()
        {
            return _queryProvider.ToList().Select(x => BendIfPossible(x)).ToList();
        }

        public int IndexOf(dynamic item)
        {
            if (item == null) return -1;
            // This necessarily enumerates the inner set at the moment until there's a way for BendyObject
            // to reliably implement IComparable<T>
            int index = -1;
            foreach (var element in ToList())
            {
                index++;
                if (ReferenceEquals(element, item) || element == item) return index;
            }
            return -1;
        }

        public IEnumerable<dynamic> OrderBy(string fieldName)
        {
            return GetDynamicOrderItems(fieldName).OrderBy(x => x.Property).Select(x => x.Bendy).ToList();
        }

        private IEnumerable<DynamicOrderItem> GetDynamicOrderItems(string fieldName)
        {
            var innerItems = new List<DynamicOrderItem>();
            foreach (var bendy in ToList())
            {
                innerItems.Add(new DynamicOrderItem { Bendy = bendy, Property = bendy[fieldName] });
            }
            return innerItems;
        }

        public IEnumerable<dynamic> OrderByDescending(string fieldName)
        {
            return GetDynamicOrderItems(fieldName).OrderByDescending(x => x.Property).Select(x => x.Bendy).ToList();
        }

        private struct DynamicOrderItem
        {
            public dynamic Bendy { get; set; }
            public object Property { get; set; }
        }

        private dynamic BendIfPossible(TypedEntity entity)
        {
            return BendIfPossible(entity, _hiveManager, _defaultValueIfNull);
        }

        protected static dynamic BendIfPossible(TypedEntity entity, IHiveManager hiveManager, object defaultValueIfNull)
        {
            var asContent = MapIfPossible(entity, hiveManager.FrameworkContext.TypeMappers);
            if (asContent == null) return defaultValueIfNull;
            return asContent.Bend(hiveManager);
        }

        protected static Content MapIfPossible(TypedEntity entity, AbstractMappingEngine mappingEngineCollection)
        {
            if (entity == null) return null;
            var alreadyContent = entity as Content;
            if (alreadyContent != null) return alreadyContent;
            return mappingEngineCollection.Map<Content>(entity);
        }

        class DynamicContentEnumerator : EnumeratorWrapper<dynamic> 
        {
            private readonly object _defaultValueIfNull;

            public DynamicContentEnumerator(IEnumerator<TypedEntity> innerEnumerator, IHiveManager hiveManager, object defaultValueIfNull)
                : base(innerEnumerator)
            {
                _defaultValueIfNull = defaultValueIfNull;
                HiveManager = hiveManager;
            }

            public override dynamic Current
            {
                get
                {
                    var bent = BendIfPossible(InnerEnumerator.Current as TypedEntity, HiveManager, _defaultValueIfNull);
                    return bent ?? InnerEnumerator.Current;
                }
            }

            /// <summary>
            /// Gets the framework context.
            /// </summary>
            /// <remarks></remarks>
            public IHiveManager HiveManager { get; protected set; }
        }
    }


    //public class BendyContentList : QueryWrapperForDynamics<dynamic, Content>
    //{
    //    class BendyEnumerator : EnumeratorWrapper<dynamic>
    //    {
    //        public BendyEnumerator(IEnumerator<dynamic> innerEnumerator)
    //            : base(innerEnumerator)
    //        {
    //        }

    //        public override dynamic Current
    //        {
    //            get
    //            {
    //                var current = InnerEnumerator.Current as Content;
    //                if (current != null) return current.Bend();
    //                return InnerEnumerator.Current;
    //            }
    //        }
    //    }

    //    private readonly IHiveManager _hiveManager;
    //    private readonly object _defaultValueIfNull;
    //    private IEnumerable<HiveId> _localIdsList;

    //    public BendyContentList(IQueryable<dynamic> source, IHiveManager hiveManager, object defaultValueIfNull, IEnumerable<HiveId> localIdsList)
    //        : base(source)
    //    {
    //        _defaultValueIfNull = defaultValueIfNull;
    //        _hiveManager = hiveManager;
    //        _localIdsList = localIdsList ?? Enumerable.Empty<HiveId>();
    //    }

    //    //protected override IQueryable<dynamic> WrapForCasting(IQueryable<dynamic> source)
    //    //{
    //    //    return source.OfType<TypedEntity>();
    //    //}

    //    public override IEnumerator<dynamic> GetEnumerator()
    //    {
    //        return new BendyEnumerator(base.GetEnumerator());
    //    }

    //    public override dynamic FirstOrDefault()
    //    {
    //        return Queryable.FirstOrDefault<Content>(this.OfType<Content>()).IfNotNull(x => x.Bend());
    //    }

    //    //public override dynamic FirstOrDefault()
    //    //{
    //    //    //var item = base.FirstOrDefault() as Content;
    //    //    //if (item != null) return item.Bend();
    //    //    //return null;

    //    //    //var queryProvider = CmsHiveManagerExtensions.QueryContent(_hiveManager).OfRevisionType(FixedStatusTypes.Published);
    //    //    //var allLocalIds = GetAllLocalIds().ToArray();
    //    //    //return queryProvider.InIds(allLocalIds).Cast<Content>().FirstOrDefault().IfNotNull(x => x.Bend());

    //    //    //var allLocalIds = GetAllLocalIds().ToArray();

    //    //    //return InnerSet.InIds(allLocalIds).FirstOrDefault();
    //    //}

    //    //public override dynamic First()
    //    //{
    //    //    //var item = base.First() as Content;
    //    //    //if (item != null) return item.Bend();
    //    //    //return null;

    //    //    //var queryProvider = CmsHiveManagerExtensions.QueryContent(_hiveManager).OfRevisionType(FixedStatusTypes.Published);
    //    //    //var allLocalIds = GetAllLocalIds().ToArray();
    //    //    //return queryProvider.InIds(allLocalIds).Cast<Content>().First().IfNotNull(x => x.Bend());

    //    //    //var allLocalIds = GetAllLocalIds().ToArray();

    //    //    //return InnerSet.InIds(allLocalIds).First();
    //    //}

    //    // TODO: These methods are not performant because it has to filter items to this collection in memory
    //    // change it to include the hierarchy in the QueryDescription

    //    public dynamic First(string predicate, params object[] substitutions)
    //    {
    //        //var result = GetAllResults(predicate, substitutions);
    //        //var allIdsHere = GetAllLocalIds();

    //        //return GetFilteredResults(result, allIdsHere).First();

    //        return GetAllResults(predicate, substitutions).First();
    //    }

    //    public BendyContentList Where(string predicate, params object[] substitutions)
    //    {
    //        //var result = GetAllResults(predicate, substitutions);
    //        //var allIdsHere = GetAllLocalIds();

    //        //return new BendyContentList(GetFilteredResults(result, allIdsHere).AsQueryable(), _hiveManager, _defaultValueIfNull);
            
    //        return new BendyContentList(GetAllResults(predicate, substitutions), _hiveManager, _defaultValueIfNull, GetAllLocalIds());
    //    }

    //    public IEnumerable<dynamic> OrderBy(string fieldName)
    //    {
    //        return new BendyContentList(GetDynamicOrderItems(fieldName).OrderBy(x => x.Property).Select(x => x.Bendy).AsQueryable(), _hiveManager, _defaultValueIfNull, GetAllLocalIds());
    //    }

    //    private IEnumerable<DynamicOrderItem> GetDynamicOrderItems(string fieldName)
    //    {
    //        var innerItems = new List<DynamicOrderItem>();
    //        foreach (var bendy in this)
    //        {
    //            innerItems.Add(new DynamicOrderItem { Bendy = bendy, Property = bendy[fieldName] });
    //        }
    //        return innerItems;
    //    }

    //    public IEnumerable<dynamic> OrderByDescending(string fieldName)
    //    {
    //        return new BendyContentList(GetDynamicOrderItems(fieldName).OrderByDescending(x => x.Property).Select(x => x.Bendy).AsQueryable(), _hiveManager, _defaultValueIfNull, GetAllLocalIds());
    //    }

    //    private struct DynamicOrderItem
    //    {
    //        public dynamic Bendy { get; set; }
    //        public object Property { get; set; }
    //    }

    //    public dynamic FirstOrDefault(string predicate, params object[] substitutions)
    //    {
    //        //var result = GetAllResults(predicate, substitutions);
    //        //var allIdsHere = GetAllLocalIds();

    //        //return GetFilteredResults(result, allIdsHere)
    //        //    .DefaultIfEmpty(_defaultValueIfNull)
    //        //    .FirstOrDefault();

    //        return GetAllResults(predicate, substitutions).DefaultIfEmpty(_defaultValueIfNull).Cast<Content>().FirstOrDefault().IfNotNull(x => x.Bend());
    //    }

    //    public dynamic Single(string predicate, params object[] substitutions)
    //    {
    //        //var result = GetAllResults(predicate, substitutions);
    //        //var allIdsHere = GetAllLocalIds();

    //        //return GetFilteredResults(result, allIdsHere).Single();
    //        return GetAllResults(predicate, substitutions).Single();
    //    }

    //    public dynamic SingleOrDefault(string predicate, params object[] substitutions)
    //    {
    //        //var result = GetAllResults(predicate, substitutions);
    //        //var allIdsHere = GetAllLocalIds();

    //        //return GetFilteredResults(result, allIdsHere)
    //        //    .DefaultIfEmpty(_defaultValueIfNull)
    //        //    .SingleOrDefault();
    //        return GetAllResults(predicate, substitutions)
    //            .DefaultIfEmpty(_defaultValueIfNull)
    //            .SingleOrDefault();
    //    }

    //    public dynamic Last(string predicate, params object[] substitutions)
    //    {
    //        //var result = GetAllResults(predicate, substitutions);
    //        //var allIdsHere = GetAllLocalIds();

    //        //return GetFilteredResults(result, allIdsHere).Last();
    //        return GetAllResults(predicate, substitutions).Last();
    //    }

    //    public dynamic LastOrDefault(string predicate, params object[] substitutions)
    //    {
    //        //var result = GetAllResults(predicate, substitutions);
    //        //var allIdsHere = GetAllLocalIds();

    //        //return GetFilteredResults(result, allIdsHere)
    //        //    .DefaultIfEmpty(_defaultValueIfNull)
    //        //    .LastOrDefault();
    //        return GetAllResults(predicate, substitutions)
    //            .DefaultIfEmpty(_defaultValueIfNull)
    //            .LastOrDefault();
    //    }

    //    public bool Any(string predicate, params object[] substitutions)
    //    {
    //        //var result = GetAllResults(predicate, substitutions);
    //        //var allIdsHere = GetAllLocalIds();

    //        //return GetFilteredResults(result, allIdsHere).Any();
    //        var result = GetAllResults(predicate, substitutions);
    //        var allIdsHere = GetAllLocalIds();

    //        return GetAllResults(predicate, substitutions).Any();
    //    }

    //    public int Count(string predicate, params object[] substitutions)
    //    {
    //        //var result = GetAllResults(predicate, substitutions);
    //        //var allIdsHere = GetAllLocalIds();

    //        //return GetFilteredResults(result, allIdsHere).Count();
    //        return GetAllResults(predicate, substitutions).Count();
    //    }

    //    //private IEnumerable<dynamic> GetFilteredResults(IEnumerable<Content> allResults, IEnumerable<HiveId> allIds)
    //    //{
    //    //    foreach (var content in allResults)
    //    //    {
    //    //        foreach (var hiveId in allIds.Where(hiveId => content.Id == hiveId))
    //    //        {
    //    //            yield return content.Bend();
    //    //        }
    //    //    }
    //    //}

    //    private IEnumerable<HiveId> GetAllLocalIds()
    //    {
    //        if (_localIdsList.Any()) return _localIdsList;

    //        var allIdsHere = new List<HiveId>();
    //        foreach (var bendy in this)
    //        {
    //            var thisId = bendy.Id;
    //            if (thisId != null)
    //            {
    //                allIdsHere.Add((HiveId)thisId);
    //            }
    //        }

    //        _localIdsList = allIdsHere;

    //        return _localIdsList;
    //    }

    //    private IQueryable<Content> GetAllResults(string predicate, object[] substitutions)
    //    {
    //        var queryProvider = CmsHiveManagerExtensions.QueryContent(_hiveManager).OfRevisionType(FixedStatusTypes.Published);

    //        var queryable = RenderViewModelQueryExtensions.Where<Content>(queryProvider, predicate, substitutions);
    //        //var debug = queryable.ToList();
    //        //var debug2 = queryProvider.ToList();
    //        var allLocalIds = GetAllLocalIds().ToArray();
    //        //var debug3 = queryable.InIds(allLocalIds).ToList();
    //        var result = queryable.InIds(allLocalIds).Cast<Content>();
    //        return result;
    //    }

    //    //private Content[] GetAllResults(string predicate, object[] substitutions)
    //    //{
    //    //    var queryProvider = CmsHiveManagerExtensions.QueryContent(_hiveManager).OfRevisionType(FixedStatusTypes.Published);

    //    //    var result = RenderViewModelQueryExtensions.Where<Content>(queryProvider, predicate, substitutions).ToArray();
    //    //    return result;
    //    //}
    //}
}