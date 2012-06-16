using System;
using System.Collections.Generic;
using System.Web.Security;

namespace Umbraco.Hive.Providers.Membership.Linq
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    public class MembershipProviderQuery
    {

        /// <summary>
        /// The query type
        /// </summary>
        public MembershipQueryType Type { get; private set; }

        public ValuePredicateType SearchTermPredicateType { get; private set; }

        /// <summary>
        /// The object used to search against the QueryType
        /// </summary>
        public object SearchValue { get; set; }

        /// <summary>
        /// A filter for a custom query
        /// </summary>
        public Func<IEnumerable<MembershipUser>, IEnumerable<MembershipUser>> QueryFilter { get; set; }

        public MembershipProviderQuery(MembershipQueryType type)
        {
            Type = type;
            SearchTermPredicateType = ValuePredicateType.Equal;
        }

        public MembershipProviderQuery(MembershipQueryType type, object searchValue)
            : this(type)
        {
            SearchValue = searchValue;
            SearchTermPredicateType = ValuePredicateType.Equal;
        }

        public MembershipProviderQuery(MembershipQueryType type, object searchValue, ValuePredicateType predicateType)
            : this(type)
        {
            SearchValue = searchValue;
            SearchTermPredicateType = predicateType;
        }

        public MembershipProviderQuery(MembershipQueryType type, Func<IEnumerable<MembershipUser>, IEnumerable<MembershipUser>> filter)
            : this(type)
        {
            QueryFilter = filter;
            SearchTermPredicateType = ValuePredicateType.Equal;
        }

    }
}