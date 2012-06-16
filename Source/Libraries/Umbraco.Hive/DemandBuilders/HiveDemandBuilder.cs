using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.Tasks;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.Diagnostics;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.Tasks;

namespace Umbraco.Hive.DemandBuilders
{
    using Umbraco.Framework.Configuration;

    public class HiveDemandBuilder : IDependencyDemandBuilder
    {
        #region Implementation of IDependencyDemandBuilder

        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            var config2 = context.ConfigurationResolver.GetConfigSection(HiveConfigSection.ConfigXmlKey) as HiveConfigSection;

            if (config2 == null)
                throw new ConfigurationErrorsException(string.Format("Cannot find {0} section in application configuration.", HiveConfigSection.ConfigXmlKey));

            var providerMapElements = DeepConfigManager.Default.GetPluginSettings<HiveConfigSection, ProviderMappingElementCollection>(HiveConfigSection.ConfigXmlKey, x => x.ProviderMappings).SelectMany(x => x);
            var mappings = providerMapElements.Distinct().ToArray();

            var mergedMappings = mappings.Select(x => new
                {
                    MappingName = x.Key,
                    Original = x,
                    ReadWriters = mappings.Where(y => y.Key == x.Key).SelectMany(y => y.ReadWriters).Distinct().ToArray(),
                    Readers = mappings.Where(y => y.Key == x.Key).SelectMany(y => y.Readers).Distinct().ToArray(),
                    UriMatches = mappings.Where(y => y.Key == x.Key).SelectMany(y => y.UriMatches).Distinct().ToArray(),
                    TotalOriginalsWithSameKey = mappings.Count(y => y.Key == x.Key)
                }).ToArray();

            foreach (var providerMapping in mergedMappings.DistinctBy(x => x.MappingName))
            {
                // Check for duplicates in config
                var mappingName = providerMapping.MappingName;

                if (providerMapping.TotalOriginalsWithSameKey > 1)
                    LogHelper.TraceIfEnabled<HiveDemandBuilder>("Warning: more than one provider group has been specified in Hive configuration with key '{0}', possibly as a result of a plugin's config. {1} sets of configuration have been merged.".InvariantFormat(mappingName, providerMapping.TotalOriginalsWithSameKey));

                var mappedReaderKeys = providerMapping.Readers.OrderBy(x => x.Ordinal).Select(x => x.ProviderKey).ToArray();
                var mappedReadWriterKeys = providerMapping.ReadWriters.OrderBy(x => x.Ordinal).Select(x => x.ProviderKey).ToArray();

                foreach (ReadWriterTypeReferenceElement readWriter in providerMapping.ReadWriters)
                {
                    // Register ProviderSetups for each read-writer
                    ReadWriterTypeReferenceElement writer = readWriter;
                    var mappingKey = mappingName;
                    containerBuilder.ForFactory(x =>
                            {
                                var metadata = new ProviderMetadata(writer.ProviderKey, null, false, writer.IsPassthrough);

                                var getBootstrapper = x.TryResolve<AbstractProviderBootstrapper>(writer.ProviderKey);
                                if (!getBootstrapper.Success)
                                    LogHelper.Warn<HiveDemandBuilder>("No bootstrapper was registered for Hive provider {0}", writer.ProviderKey);

                                var getRepositoryFactory = x.TryResolve<AbstractEntityRepositoryFactory>(writer.ProviderKey);
                                if (getRepositoryFactory.Success)
                                {
                                    var unitFactory = new ProviderUnitFactory(getRepositoryFactory.Value);
                                    return new ProviderSetup(unitFactory, metadata, x.Resolve<IFrameworkContext>(),
                                                             getBootstrapper.Value, 0);
                                }

                                LogHelper.Warn<HiveDemandBuilder>(
                                    "Config '{0}' wants read-write provider with key {1} but it's not registered - check the available providers section. Error: {2}",
                                    mappingKey, writer.ProviderKey, getRepositoryFactory.Error.Message);

                                // We couldn't get a session factory, so register an "UninstalledProviderSetup" instead
                                return new UninstalledProviderSetup(metadata, x.Resolve<IFrameworkContext>(), getBootstrapper.Value, 0);
                            })
                        //.KnownAsSelf()
                        .Named<ProviderSetup>(writer.ProviderKey)
                        .ScopedAs.Singleton();
                }

                foreach (ReaderTypeReferenceElement reader in providerMapping.Readers)
                {
                    // Register ReadonlyProviderSetups for each reader
                    ReaderTypeReferenceElement readerLocalCopy = reader;
                    var mappingKey = mappingName;
                    containerBuilder.ForFactory(x =>
                            {
                                var metadata = new ProviderMetadata(readerLocalCopy.ProviderKey, null, false, readerLocalCopy.IsPassthrough);

                                var getBootstrapper = x.TryResolve<AbstractProviderBootstrapper>(readerLocalCopy.ProviderKey);
                                if (!getBootstrapper.Success)
                                    LogHelper.Warn<HiveDemandBuilder>("No bootstrapper was registered for Hive provider {0}", readerLocalCopy.ProviderKey);

                                var getRepositoryFactory = x.TryResolve<AbstractEntityRepositoryFactory>(readerLocalCopy.ProviderKey);
                                if (getRepositoryFactory.Success)
                                {
                                    var unitFactory = new ReadonlyProviderUnitFactory(getRepositoryFactory.Value);
                                    return new ReadonlyProviderSetup(unitFactory, metadata,
                                                                     x.Resolve<IFrameworkContext>(),
                                                                     getBootstrapper.Value, 0);
                                }

                                LogHelper.Warn<HiveDemandBuilder>(
                                    "Config '{0}' wants reader provider with key {1} but it's not registered or can't be instantiated - check the available providers section. Error: {2}",
                                    mappingKey, readerLocalCopy.ProviderKey, getRepositoryFactory.Error.Message);

                                // We couldn't get a session factory, so register an "UninstalledReadonlyProviderSetup" instead
                                return new UninstalledReadonlyProviderSetup(metadata, x.Resolve<IFrameworkContext>(), getBootstrapper.Value, 0);
                            })
                        //.KnownAsSelf()
                        .Named<ReadonlyProviderSetup>(readerLocalCopy.ProviderKey)
                        .ScopedAs.Singleton();
                }


                // Register IUriMatches for each mapping group
                foreach (UriMatchElement uriMatch in providerMapping.UriMatches)
                {
                    // Copy to a locally-scoped instance since the variable usage is wrapped in an anon delegate for later use
                    var match = uriMatch;

                    containerBuilder.ForFactory(x =>
                                                    {
                                                        if (!Uri.IsWellFormedUriString(match.UriPattern, UriKind.Absolute))
                                                            throw new InvalidOperationException("Hive config is invalid: '{0}' needs to be changed to a valid absolute Uri".InvariantFormat(match.UriPattern));

                                                        return new WildcardUriMatch(new Uri(match.UriPattern));
                                                    })
                        .Named<WildcardUriMatch>(mappingName)
                        .ScopedAs.Singleton();
                }

                // For every mapping group in config, register a DefaultHiveMappingGroup
                // with a constructor which accepts the map key, and all the available readers and writers which
                // map the keys selected in this group
                containerBuilder.ForFactory(
                    x => GenerateMappingGroup(mappingName, x, mappedReadWriterKeys, mappedReaderKeys))
                    .KnownAsSelf() // Register unnamed as well as named services to aid passing all registered groups into ctor params of type IEnumerable<ProviderMappingGroup>
                    .Named<ProviderMappingGroup>(mappingName)
                    .ScopedAs.Singleton();

            }

            //TODO: Remove magic strings for perf counter declarations
            containerBuilder
                .ForInstanceOfType(new HiveCounterManager("Hive 5.0", "TBD", "DEV"))
                .ScopedAs.Singleton();

            containerBuilder.ForFactory(x =>
                {
                    var groups = x.ResolveAll<ProviderMappingGroup>().DistinctBy(y => y.Key);
                    var framework = x.Resolve<IFrameworkContext>();
                    var perf = x.TryResolve<HiveCounterManager>();
                    return new HiveManager(groups, perf.Success ? perf.Value : null, framework);
                })
                .KnownAs<IHiveManager>()
                .KnownAsSelf()
                .ScopedAs.Singleton();

            containerBuilder.ForFactory(x => GetCacheWatcherTaskFactory(x, TaskTriggers.Hive.PostAddOrUpdateOnUnitComplete))
                .KnownAs<Lazy<AbstractTask, TaskMetadata>>()
                .ScopedAs.Singleton();

            containerBuilder.ForFactory(x => GetCacheWatcherTaskFactory(x, TaskTriggers.Hive.PostReadEntity))
                .KnownAs<Lazy<AbstractTask, TaskMetadata>>()
                .ScopedAs.Singleton();
        }

        private static Lazy<AbstractTask, TaskMetadata> GetCacheWatcherTaskFactory(IResolutionContext x, string trigger)
        {
            return new Lazy<AbstractTask, TaskMetadata>(
                () => new CacheWatcherTask(x.Resolve<IFrameworkContext>()),
                new TaskMetadata(trigger, true));
        }

        private static ProviderMappingGroup GenerateMappingGroup(string mappingName, IResolutionContext resolutionContext, string[] mappedReadWriterKeys, string[] mappedReaderKeys)
        {
            var tryReadersInGroup = TryResolveGroupReaders(resolutionContext, mappedReaderKeys).ToArray();
            var tryReadWritersInGroup = TryResolveGroupReadWriters(resolutionContext, mappedReadWriterKeys).ToArray();

            var readersInGroup = tryReadersInGroup.Where(x => x.Success);
            var readWritersInGroup = tryReadWritersInGroup.Where(x => x.Success);

            if (!readersInGroup.Any() && !readWritersInGroup.Any())
            {
                var errorBuilder = new StringBuilder();
                errorBuilder.AppendLine("Writers:");
                foreach (var namedResolutionAttempt in tryReadWritersInGroup)
                {
                    errorBuilder.AppendLine("Key: {0}, Error: {1}".InvariantFormat(namedResolutionAttempt.Key, namedResolutionAttempt.Error.ToString()));
                }
                errorBuilder.AppendLine();
                errorBuilder.AppendLine("Readers:");
                foreach (var namedResolutionAttempt in tryReadersInGroup)
                {
                    errorBuilder.AppendLine("Key: {0}, Error: {1}".InvariantFormat(namedResolutionAttempt.Key, namedResolutionAttempt.Error.ToString()));
                }
                throw new InvalidOperationException(
                    "Could not create mapping group '{0}'. Errors:\n{1}".InvariantFormat(mappingName,
                                                                                         errorBuilder.ToString()));
            }

            var uriMatches = resolutionContext.Resolve<IEnumerable<WildcardUriMatch>>(mappingName);
            var frameworkContext = resolutionContext.Resolve<IFrameworkContext>();

            var readonlyProviderSetups = readersInGroup.Select(x => x.Value).ToArray();
            var providerSetups = readWritersInGroup.Select(x => x.Value).ToArray();

            return new ProviderMappingGroup(mappingName, uriMatches, readonlyProviderSetups, providerSetups, frameworkContext);
        }

        private static IEnumerable<NamedResolutionAttempt<ProviderSetup>> TryResolveGroupReadWriters(IResolutionContext x, string[] mappedReadWriterKeys)
        {
            var readWriters = mappedReadWriterKeys.Select(s =>
                                                              {
                                                                  var resolveResult = x.TryResolve<ProviderSetup>(s);
                                                                  return resolveResult;
                                                              })
                //.Where(y => y.Success).Select(y => y.Value)
                .OrderBy(y => y.Success ? y.Value.PriorityOrdinal : 999).ToList();

            return readWriters;
        }

        private static IEnumerable<NamedResolutionAttempt<ReadonlyProviderSetup>> TryResolveGroupReaders(IResolutionContext ctx, string[] mappedReaderKeys)
        {
            // First get the readers. Not all specified reader keys might exist if a provider is just read-write,
            // so for any that we don't find, we'll try to resolve a writer that matches the key

            var readerResolutionAttempts = mappedReaderKeys.Select(s =>
                                                           {
                                                               var resolveResult = ctx.TryResolve<ReadonlyProviderSetup>(s);
                                                               return resolveResult;
                                                           })
                //.Where(y => y.Success).Select(y => y.Value).ToList();
                .OrderBy(x => x.Success ? x.Value.PriorityOrdinal : 999).ToList();

            // These readers were not found
            var notFoundReaderKeys = readerResolutionAttempts.Where(y => !y.Success).Select(y => y.Key).ToArray();

            // So try to resolve writers with matching keys
            var tryFindWriters = TryResolveGroupReadWriters(ctx, notFoundReaderKeys);

            // We need to return a sequence of ResolutionAttempt<ReadonlyProviderSetup>
            // Where we have found neither a reader nor a writer, need to include the error from both
            var expressWritersAsReaders = tryFindWriters
                .Select(y => 
                    {
                        if (!y.Success)
                        {
                            return new NamedResolutionAttempt<ReadonlyProviderSetup>(y.Key, y.Error);
                        }

                        var providerSetup = y.Value;
                        var newProviderSetup = (providerSetup is UninstalledProviderSetup)
                                                   ? new UninstalledReadonlyProviderSetup(
                                                         providerSetup.ProviderMetadata,
                                                         providerSetup.FrameworkContext,
                                                         providerSetup.Bootstrapper,
                                                         providerSetup.PriorityOrdinal)
                                                   : new ReadonlyProviderSetup(
                                                         new ReadonlyProviderUnitFactory
                                                             (providerSetup.UnitFactory.EntityRepositoryFactory),
                                                         providerSetup.ProviderMetadata,
                                                         providerSetup.FrameworkContext,
                                                         providerSetup.Bootstrapper,
                                                         providerSetup.PriorityOrdinal);

                        return new NamedResolutionAttempt<ReadonlyProviderSetup>(y.Key, true, newProviderSetup);
                    });

            // Where the reader and writer resolution attempts contain the same key, 
            // we need to merge the error messages for the caller if it wasn't a success
            // or overwrite the values if it was

            // First get a sequence of the readers we know we can return
            var totalItemsToReturn =
                new List<NamedResolutionAttempt<ReadonlyProviderSetup>>(
                    readerResolutionAttempts.Where(x => !notFoundReaderKeys.Contains(x.Key)));

            // Now check the items where we tried to resolve a reader, and then had to try to resolve a writer
            var keysInBoth = readerResolutionAttempts.Select(x => x.Key).Intersect(expressWritersAsReaders.Select(x => x.Key));
            foreach (var key in keysInBoth)
            {
                var originalReaderAttempt = readerResolutionAttempts.FirstOrDefault(x => x.Key == key);
                var writerAttempt = expressWritersAsReaders.FirstOrDefault(x => x.Key == key);
                if (originalReaderAttempt != null && writerAttempt != null)
                    if (!writerAttempt.Success)
                    {
                        totalItemsToReturn.Add(new NamedResolutionAttempt<ReadonlyProviderSetup>(writerAttempt.Key,
                            new InvalidOperationException("Could not resolve a writer or a reader for key {0}. \nReader error: {1}. \nWriter error: {2}"
                                                              .InvariantFormat(key, originalReaderAttempt.Error.ToString(),
                                                                               writerAttempt.Error.ToString()))));
                    }
                    else
                    {
                        totalItemsToReturn.Add(writerAttempt);
                    }
            }

            // Now return the total set
            return totalItemsToReturn.OrderBy(x => x.Success ? x.Value.PriorityOrdinal : 999);
        }

        #endregion
    }
}
