using System;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive
{
    using System.Collections.Generic;

    public static class HiveManagerExtensions
    {
        public static WriterResult<TFilter> AutoCommitTo<TFilter>(this IHiveManager manager, Action<IGroupUnit<TFilter>> repository) 
            where TFilter : class, IProviderTypeFilter
        {
            using (var unit = manager.OpenWriter<TFilter>())
            {
                try
                {
                    repository.Invoke(unit);
                    unit.Complete();
                    return new WriterResult<TFilter>(true, false);
                }
                catch (Exception outerException)
                {
                    try
                    {
                        unit.Abandon();
                        return new WriterResult<TFilter>(false, true, outerException);
                    }
                    catch (Exception innerException)
                    {
                        return new WriterResult<TFilter>(false, false, outerException, innerException);
                    }
                }
                
            }
        }

        public static ProviderGroupMatchResult GetProviderGroupByType<TFilter>(this IHiveManager manager)
            where TFilter : class, IProviderTypeFilter
        {
            return manager.GetProviderGroupByType(typeof(TFilter));
        }

        public static ProviderGroupMatchResult GetProviderGroupByType(this IHiveManager manager, Type type)
        {
            var attrib = RepositoryTypeAttribute.GetFrom(type);
            if (attrib == null) throw new InvalidOperationException("In order to load a provider group by RepositoryType, the supplied type must be decorated with a RepositoryTypeAttribute");
            Uri providerGroupRoot = attrib.ProviderGroupRoot;
            return manager.GetFirstGroupOrThrow(providerGroupRoot);
        }

        public static IEnumerable<ProviderGroupMatchResult> GetGroups(this IHiveManager manager, Uri providerGroupRoot)
        {
            return manager.ProviderGroups
                .Select(x =>
                {
                    var isMatch = x.IsMatchForUri(providerGroupRoot);
                    return isMatch.Success ? new ProviderGroupMatchResult(isMatch.MatchingUri, x) : null;
                })
                .WhereNotNull()
                .AsEnumerable();
        }

        public static ReadonlyGroupUnitFactory<TFilter> TryGetReader<TFilter>(this IHiveManager manager, Uri providerGroupRoot) 
            where TFilter : class, IProviderTypeFilter
        {
            var group = manager.TryGetFirstGroup(providerGroupRoot);
            if (group == null) return null;
            return new ReadonlyGroupUnitFactory<TFilter>(group.Group.Readers, group.UriMatch.Root, manager.Context, manager.FrameworkContext);
        }

        public static ProviderGroupMatchResult TryGetFirstGroup(this IHiveManager manager, Uri providerGroupRoot)
        {
            return GetGroups(manager, providerGroupRoot).SingleOrDefault();
        }

        private static ProviderGroupMatchResult GetFirstGroupOrThrow(this IHiveManager manager, Uri providerGroupRoot)
        {
            var groups = GetGroups(manager, providerGroupRoot).ToArray();

            if (!groups.Any()) throw new InvalidOperationException("No provider group matches the requested route '{0}' - check the Hive configuration".InvariantFormat(providerGroupRoot));
            
            if (groups.Length > 1)
            {
                //if more than 1 is found, then we need to take the one with the longest Uri match                
                var maxUriMatchLength = groups.Select(x => x.UriMatch.Root.ToString().Length).Max();
                var match = groups.Where(x => x.UriMatch.Root.ToString().Length == maxUriMatchLength).ToArray();
                //if there are more than one that match the same length, then there must be duplicates.
                if (match.Length > 1)
                {
                    throw new InvalidOperationException("More than one provider group matches the requested route '{0}' - check the Hive configuration to ensure no conflicts exist".InvariantFormat(providerGroupRoot));    
                }
                return match.Single();
            }
                
            return groups.FirstOrDefault();
        }

        public static ProviderGroupMatchResult GetProviderGroup(this IHiveManager manager, Uri customMappingRoot)
        {
            return manager.GetFirstGroupOrThrow(customMappingRoot);
        }

        public static IReadonlyGroupUnit<TFilter> OpenReader<TFilter>(this IHiveManager hiveManager, Uri providerMappingRoot)
            where TFilter : class, IProviderTypeFilter
        {
            var factory = hiveManager.GetReader<TFilter>(providerMappingRoot);
            return factory.CreateReadonly();
        }

        public static IReadonlyGroupUnit<TFilter> OpenReader<TFilter>(this IHiveManager hiveManager, AbstractScopedCache overrideCacheScope = null) 
            where TFilter : class, IProviderTypeFilter
        {
            var factory = hiveManager.GetReader<TFilter>();
            return factory.CreateReadonly<TFilter>(overrideCacheScope);
        }

        public static IGroupUnit<TFilter> OpenWriter<TFilter>(this IHiveManager hiveManager, AbstractScopedCache overrideCacheScope = null)
            where TFilter : class, IProviderTypeFilter
        {
            var factory = hiveManager.GetWriter<TFilter>();
            return factory.Create<TFilter>(overrideCacheScope);
        }

        public static IGroupUnit<TFilter> OpenWriter<TFilter>(this IHiveManager hiveManager, Uri providerMappingRoot)
            where TFilter : class, IProviderTypeFilter
        {
            var factory = hiveManager.GetWriter<TFilter>(providerMappingRoot);
            return factory.Create<TFilter>();
        }
    }
}