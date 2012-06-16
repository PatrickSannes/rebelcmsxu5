using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.Testing;
using Umbraco.Hive;
using Umbraco.Framework;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.Providers.IO;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Tests.Extensions
{
    using System.Threading;
    using Umbraco.Framework.Caching;

    public class NullCachingProvider : AbstractCacheProvider
    {
        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            return;
        }

        #endregion

        #region Overrides of AbstractCacheProvider

        public override CacheModificationResult AddOrChange<T>(CacheKey key, CacheValueOf<T> cacheObject)
        {
            return new CacheModificationResult(false, false);
        }

        public override void Clear()
        {
            return;
        }

        public override bool Remove(CacheKey key)
        {
            return true;
        }

        public override IEnumerable<CacheKey<T>> GetKeysMatching<T>(Func<T, bool> predicate)
        {
            yield break;
        }

        protected override CacheEntry<T> PerformGet<T>(CacheKey key)
        {
            return null;
        }

        #endregion
    }

    public static class FakeHiveCmsManager
    {
        private static readonly ReaderWriterLockSlim _fileCreationLocker = new ReaderWriterLockSlim();

        public static RepositoryContext CreateFakeRepositoryContext(IFrameworkContext frameworkContext)
        {
            return new RepositoryContext(new NullCachingProvider(), new NullCachingProvider(), frameworkContext);
        }

        /// <summary>
        /// Creates a Hive mapping group for templates
        /// </summary>
        /// <param name="frameworkContext"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ProviderMappingGroup CreateFakeTemplateMappingGroup(FakeFrameworkContext frameworkContext, string path = "")
        {
            string binPath;
            if (path.IsNullOrWhiteSpace())
            {
                binPath = Path.Combine(Common.CurrentAssemblyDirectory, "Templates");    
            }
            else
            {
                binPath = path;
            }

            // TODO: Would rather we weren't creating files during unit tests esp unless their paths are thread-agnostic and we're sure of deleting them too (APN)
            using (new WriteLockDisposable(_fileCreationLocker))
            {
                //ensure the folder exists
                if (!Directory.Exists(binPath))
                {
                    Directory.CreateDirectory(binPath);
                }

                //create 3 empty template files
                var homePagePath = Path.Combine(binPath, "home-page.cshtml");
                var textPagePath = Path.Combine(binPath, "text-page.cshtml");
                var faqPagePath = Path.Combine(binPath, "faq-page.cshtml");

                if (!File.Exists(homePagePath)) File.Create(homePagePath);
                if (!File.Exists(textPagePath)) File.Create(textPagePath);
                if (!File.Exists(faqPagePath)) File.Create(faqPagePath);
            }

            var providerMetadata = new ProviderMetadata("templates", new Uri("storage://templates"), true, false);
            var dependencyHelper = new DependencyHelper(new Settings("*.cshtml", binPath, "/", "", "", "~/"), providerMetadata);
            var entityRepositoryFactory = new EntityRepositoryFactory(providerMetadata, null, null, frameworkContext, dependencyHelper);
            var readUnitFactory = new ReadonlyProviderUnitFactory(entityRepositoryFactory);
            var readWriteUnitFactory = new ProviderUnitFactory(entityRepositoryFactory);
            var bootstrapper = new FakeHiveProviderBootstrapper();
            var readonlyProviderSetup = new ReadonlyProviderSetup(readUnitFactory, providerMetadata, frameworkContext, bootstrapper, 0);
            var providerSetup = new ProviderSetup(readWriteUnitFactory, providerMetadata, frameworkContext, bootstrapper, 0);
            var uriMatch = new WildcardUriMatch(new Uri("storage://templates"));
            var persistenceMappingGroup = new ProviderMappingGroup(
                "templates",
                uriMatch,
                readonlyProviderSetup,
                providerSetup,
                frameworkContext);
            return persistenceMappingGroup;
        }

        /// <summary>
        /// Creates a new HiveManager with only the NHibernate default mapping group
        /// </summary>
        /// <param name="frameworkContext"></param>
        /// <returns></returns>
        public static IHiveManager New(FakeFrameworkContext frameworkContext)
        {
            return NewWithNhibernate(new ProviderMappingGroup[] {}, frameworkContext);
        }

        /// <summary>
        /// Creates a new HiveManager with an NHibernate default mapping group and the supplied provider mapping groups
        /// appended
        /// </summary>
        /// <param name="providerMappingGroups"></param>
        /// <param name="frameworkContext"></param>
        /// <returns></returns>
        public static IHiveManager NewWithNhibernate(IEnumerable<ProviderMappingGroup> providerMappingGroups, FakeFrameworkContext frameworkContext)
        {
            var helper = new NhibernateTestSetupHelper(frameworkContext);
            var uriMatch = new WildcardUriMatch(new Uri("content://"));
            var persistenceMappingGroup = new ProviderMappingGroup(
                "default",
                uriMatch,
                helper.ReadonlyProviderSetup,
                helper.ProviderSetup,
                frameworkContext);

            return new HiveManager(new[] { persistenceMappingGroup }.Union(providerMappingGroups), frameworkContext);            
        }

        /// <summary>
        /// Creates a new HiveManager with an Examine default mapping group and the supplied provider mapping groups appended
        /// </summary>
        /// <param name="providerMappingGroups"></param>
        /// <param name="frameworkContext"></param>
        /// <returns></returns>
        public static IHiveManager NewWithExamine(IEnumerable<ProviderMappingGroup> providerMappingGroups, FakeFrameworkContext frameworkContext)
        {
            var helper = new ExamineTestSetupHelper(frameworkContext);
            var uriMatch = new WildcardUriMatch(new Uri("content://"));
            var persistenceMappingGroup = new ProviderMappingGroup(
                "default",
                uriMatch,
                helper.ReadonlyProviderSetup,
                helper.ProviderSetup,
                frameworkContext);

            return new HiveManager(new[] { persistenceMappingGroup }.Union(providerMappingGroups), frameworkContext);
        }

    }
}