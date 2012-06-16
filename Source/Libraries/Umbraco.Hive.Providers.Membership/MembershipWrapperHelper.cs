using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Hive.Providers.Membership.Config;
using Umbraco.Hive.Providers.Membership.Linq;

namespace Umbraco.Hive.Providers.Membership
{
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.QueryModel;

    using Umbraco.Framework.Linq.ResultBinding;

    public class MembershipWrapperHelper
    {
        private readonly IEnumerable<ProviderElement> _configuredProviders;
        private readonly IEnumerable<MembershipProvider> _membershipProviders;
        private readonly IFrameworkContext _frameworkContext;

        public MembershipWrapperHelper(
            IEnumerable<ProviderElement> configuredProviders,
            IEnumerable<MembershipProvider> membershipProviders, 
            IFrameworkContext frameworkContext)
        {
            _configuredProviders = configuredProviders;
            _membershipProviders = membershipProviders;
            _frameworkContext = frameworkContext;
        }

        public IEnumerable<T> PerformGetByQuery<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            IEnumerable<T> results;
            var criteria = new MembershipQueryVisitor().Visit(query.Criteria);
            switch (criteria.Type)
            {
                case MembershipQueryType.ByUsername:
                    results = PerformGetByUsername<T>((string)criteria.SearchValue, criteria.SearchTermPredicateType);
                    break;
                case MembershipQueryType.ById:
                    results = PerformGet<T>(true, new[] { HiveId.Parse(criteria.SearchValue.ToString()) });
                    break;
                case MembershipQueryType.ByEmail:
                    results = PerformGetByEmail<T>((string)criteria.SearchValue, criteria.SearchTermPredicateType);
                    break;
                case MembershipQueryType.Custom:
                    return criteria.QueryFilter(GetAllPagedData((m, i) =>
                        {
                            int totalUsers;
                            var result = m.GetAllUsers(i, 1000, out totalUsers);
                            return new Tuple<MembershipUserCollection, int>(result, totalUsers);
                        }))
                        .Select(x => _frameworkContext.TypeMappers.Map<T>(x));
                case MembershipQueryType.None:
                    results = Enumerable.Empty<T>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return results;
        }

        public IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids)
        {
            //TODO: because of allOrNothing, we cannot use yield return;

            var results = new List<T>();
            foreach (var i in ids)
            {
                //loop through the providers 
                foreach (var m in _membershipProviders)
                {
                    var found = m.GetUser(i.Value.Value, false);
                    //if a members is found by this id, add it to the list and don't keep scanning the remaining providers
                    if (found != null)
                    {
                        results.Add(_frameworkContext.TypeMappers.Map<T>(found));
                        break;
                    }
                }
            }

            return results;
        }

        private IEnumerable<MembershipUser> GetAllPagedData(Func<MembershipProvider, int, Tuple<MembershipUserCollection, int>> getPagedData)
        {
            var found = new Dictionary<object, MembershipUser>();

            //loop through the providers 
            foreach (var m in _membershipProviders)
            {
                int totalUsers;
                var current = 0;
                var index = 0;
                do
                {
                    var result = getPagedData(m, index);
                    totalUsers = result.Item2;
                    var paged = result.Item1;

                    index++;
                    current += paged.Count;
                    //dont add members previously added by another provider with the same provider user key
                    foreach (var p in paged.Cast<MembershipUser>().Where(p => !found.ContainsKey(p.ProviderUserKey)))
                    {
                        found.Add(p.ProviderUserKey, p);
                    }
                } while (totalUsers > current);
            }

            return found.Select(x => x.Value);
        }

        private IEnumerable<T> GetAllPagedData<T>(Func<MembershipProvider, int, Tuple<MembershipUserCollection, int>> getPagedData)
        {
            return GetAllPagedData(getPagedData).Select(x => _frameworkContext.TypeMappers.Map<T>(x));
        }

        public IEnumerable<T> PerformGetAll<T>()
        {
            return GetAllPagedData<T>((m, i) =>
                {
                    int totalUsers;
                    var result = m.GetAllUsers(i, 1000, out totalUsers);
                    return new Tuple<MembershipUserCollection, int>(result, totalUsers);
                });
        }

        public IEnumerable<T> PerformGetByEmail<T>(string email, ValuePredicateType predicateType)
        {
            return GetAllPagedData<T>((m,i) =>
                {
                    int totalUsers;
                    var result = m.FindUsersByEmail(FormatSearchTerm(email, m, predicateType), i, 1000, out totalUsers);
                    return new Tuple<MembershipUserCollection, int>(result, totalUsers);
                });
        }

        public IEnumerable<T> PerformGetByUsername<T>(string username, ValuePredicateType predicateType)
        {
            return GetAllPagedData<T>((m, i) =>
                {
                    int totalUsers;

                    var result = m.FindUsersByName(FormatSearchTerm(username, m, predicateType), i, 1000, out totalUsers);
                    return new Tuple<MembershipUserCollection, int>(result, totalUsers);
                }); 
        }

        private string FormatSearchTerm(string searchTerm, MembershipProvider m, ValuePredicateType predicateType)
        {
            var wildcard = _configuredProviders.Single(x => x.Name == m.Name).WildcardCharacter;

            switch (predicateType)
            {
                case ValuePredicateType.Equal:                    
                case ValuePredicateType.NotEqual:
                case ValuePredicateType.LessThan:
                case ValuePredicateType.GreaterThan:
                case ValuePredicateType.LessThanOrEqual:
                case ValuePredicateType.GreaterThanOrEqual:
                case ValuePredicateType.Empty:
                    //we currently dont change the value for any of the above.
                    return searchTerm;
                case ValuePredicateType.StartsWith:
                    return searchTerm + wildcard;                    
                case ValuePredicateType.EndsWith:
                    return wildcard + searchTerm;
                case ValuePredicateType.Contains:                    
                case ValuePredicateType.MatchesWildcard:
                    return wildcard + searchTerm + wildcard;
                default:
                    throw new ArgumentOutOfRangeException("predicateType");
            }

            
        }
    }
}