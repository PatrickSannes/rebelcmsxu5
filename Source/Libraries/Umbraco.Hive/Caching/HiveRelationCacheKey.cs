using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Hive.Caching
{
    using System.Linq.Expressions;
    using Umbraco.Framework;
    using Umbraco.Framework.Caching;
    using Umbraco.Framework.Data;
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;
    using Umbraco.Framework.Linq.QueryModel;
    using Umbraco.Framework.Persistence.Model.Associations;

    public class HiveRelationCacheKey //: CacheKey<HiveRelationCacheKey>
    {
        public enum RepositoryTypes
        {
            Entity = 0,
            Schema = 2
        }

        public HiveRelationCacheKey()
        {
        }

        public HiveRelationCacheKey(RepositoryTypes repositoryType, HiveId entityId, Direction direction, RelationType relationType)
        {
            RepositoryType = repositoryType;
            EntityId = entityId;
            Direction = direction;
            RelationType = relationType;
        }

        public RepositoryTypes RepositoryType { get; set; }

        public HiveId EntityId { get; set; }

        public Direction Direction { get; set; }

        public RelationType RelationType { get; set; }
    }

    public class HiveQueryCacheKey //: CacheKey<QueryDescription>
    {
        public HiveQueryCacheKey() // Used for deserialization from Json
        {}

        public HiveQueryCacheKey(QueryDescription queryDescription)
            : this(queryDescription.From, queryDescription.ResultFilter, queryDescription.Criteria, queryDescription.SortClauses.ToArray())//, queryDescription.Criteria as FieldPredicateExpression)
        {

        }

        public HiveQueryCacheKey(FromClause fromClause, ResultFilterClause resultFilterClause, Expression criteria, params SortClause[] sortClauses)
        {
            From = fromClause;
            ResultFilter = resultFilterClause;
            Criteria = criteria;
            SortClauses = sortClauses;
        }

        public FromClause From { get; set; }

        public ResultFilterClause ResultFilter { get; set; }

        public IEnumerable<SortClause> SortClauses { get; set; }

        // TODO: FPE can de/serialize but a BinaryExpression of two or more can't, easily fixable with a custom tree but clearing cache by criteria
        // a bit of an edge case (e.g. where schema type == blah)
        //public FieldPredicateExpression Criteria { get; set; }

        // For now it's here as obviously it needs serializing in order to generate a unique cache key, but you can't deserialize it
        public Expression Criteria { get; protected set; }
    }
}
