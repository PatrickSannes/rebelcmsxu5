using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.Stubs;
using File = Umbraco.Framework.Persistence.Model.IO.File;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        [TestCase]
        public void DistinctBy_ReturnsDistinctElements_AndResetsIteratorCorrectly()
        {
            // Arrange
            var tuple1 = new Tuple<string, string>("fruit", "apple");
            var tuple2 = new Tuple<string, string>("fruit", "orange");
            var tuple3 = new Tuple<string, string>("fruit", "banana");
            var tuple4 = new Tuple<string, string>("fruit", "banana"); // Should be filtered out
            var list = new List<Tuple<string, string>>()
                           {
                               tuple1,
                               tuple2,
                               tuple3,
                               tuple4
                           };

            // Act
            var iteratorSource = list.DistinctBy(x => x.Item2);

            // Assert
            // First check distinction
            Assert.AreEqual(3, iteratorSource.Count());

            // Check for iterator block mistakes - reset to original query first
            iteratorSource = list.DistinctBy(x => x.Item2);
            Assert.AreEqual(iteratorSource.Count(), iteratorSource.ToList().Count());
        }
    }
}

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class HiveManagerTests
    {
        [TestCase("content://")]
        [TestCase("storage://stylesheets/")]
        [TestCase("storage", ExpectedException = typeof(UriFormatException))]
        public void RepositoryTypeAttribute_ExposesUriScheme(string uri)
        {
            // Arrange
            var attrib = new RepositoryTypeAttribute(uri);

            // Assert
            Assert.That(attrib.ProviderGroupRoot, Is.EqualTo(new Uri(uri)));
        }

        [TestCase("content://")]
        public void ContentRepositoryType_WithTypeAttribute_ExposesUriScheme(string uri)
        {
            // Arrange
            var attrib = RepositoryTypeAttribute.GetFrom<IContentStore>();
            var attrib2 = RepositoryTypeAttribute.GetFrom(typeof(IContentStore));

            // Assert
            Assert.NotNull(attrib);
            Assert.That(attrib.ProviderGroupRoot, Is.EqualTo(new Uri(uri)));
            Assert.That(attrib2.ProviderGroupRoot, Is.EqualTo(new Uri(uri)));
        }

        [TestCase("content://mypage.htm", "content://*", true)]
        [TestCase("content://mypage.htm", "storage://*", false)]
        [TestCase("storage://mypage.htm", "storage://stylesheets/", false)]
        [TestCase("storage://stylesheets/this/is/my/folder/file.css", "storage://*", true)]
        public void ProviderRouteMatch_IsMatchForUri(string assertUri, string wildcardMatch, bool shouldPass)
        {
            // Arrange
            var match = new ProviderRouteMatch(wildcardMatch);

            // Assert
            Assert.That(shouldPass, Is.EqualTo(match.IsMatchForUri(new Uri(assertUri))));
        }

        [TestCase("content://mypage.htm", new[] { "content://*", "content://a/folder/for/me/*" }, true)]
        [TestCase("storage://a/folder/file.htm", new[] { "content://*", "storage://a/folder/for/me/*" }, false)]
        [TestCase("storage://a/folder/for/me/file.htm", new[] { "content://*", "storage://a/folder/for/me/*" }, true)]
        public void ProviderMappingGroup_IsMatchForUri(string assertUri, string[] wildcardMatches, bool shouldMatch)
        {
            // Arrange
            var matches = wildcardMatches.Select(x => new WildcardUriMatch(x)).ToList();
            var context = new FakeFrameworkContext();
            var metadata = new ProviderMetadata("test", new Uri("unimportant://"), true, false);
            var readonlySetup = new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0);
            var setup = new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0);
            var group = new ProviderMappingGroup("default", matches, Enumerable.Repeat(readonlySetup, 1), Enumerable.Repeat(setup, 1), new FakeFrameworkContext());

            // Assert
            Assert.That(shouldMatch, Is.EqualTo(group.IsMatchForUri(new Uri(assertUri)).Success));
        }

        [Test]
        public void HiveManagerExtension_InfersProviderMatch_FromRepositoryType()
        {
            // Arrange
            var context = new FakeFrameworkContext();
            var metadata = new ProviderMetadata("test", new Uri("unimportant://"), true, false);
            var groups = new[]
                             {
                                 new ProviderMappingGroup("default-content",
                                                          new WildcardUriMatch("content://*"),
                                                          new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          context),
                                 new ProviderMappingGroup("default-storage",
                                                          new WildcardUriMatch("storage://*"),
                                                          new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          context),
                                 new ProviderMappingGroup("default-assets",
                                                          new WildcardUriMatch("assets://*"),
                                                          new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          context)
                             };
            var manager = new HiveManager(groups, context);

            // Act
            var work = manager.GetProviderGroupByType<IContentStore>();

            // Assert
            Assert.NotNull(work);
        }

        [TestCase("content://hello")]
        [TestCase("custom://hello", ExpectedException = typeof(InvalidOperationException))]
        [TestCase("storage://hello")]
        [TestCase("assets://hello")]
        public void HiveManagerExtension_GetsProviderMatch_FromCustomRoot(string customRoot)
        {
            // Arrange
            var context = new FakeFrameworkContext();
            var metadata = new ProviderMetadata("test", new Uri("unimportant://"), true, false);
            var groups = new[]
                             {
                                 new ProviderMappingGroup("default-content",
                                                          new WildcardUriMatch("content://*"),
                                                          new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          context),
                                 new ProviderMappingGroup("default-storage",
                                                          new WildcardUriMatch("storage://*"),
                                                          new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          context),
                                 new ProviderMappingGroup("default-assets",
                                                          new WildcardUriMatch("assets://*"),
                                                          new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          context)
                             };
            HiveManager manager = new HiveManager(groups, context);

            // Act
            var work = manager.GetProviderGroup(new Uri(customRoot));

            // Assert
            Assert.NotNull(work);
        }

        [Test]
        public void HiveManager_OpensContentWriter_InferredFromRepositoryType()
        {
            // Arrange
            var context = new FakeFrameworkContext();
            var metadata = new ProviderMetadata("test", new Uri("unimportant://"), true, false);
            var groups = new[]
                             {
                                 new ProviderMappingGroup("default-content",
                                                          new WildcardUriMatch("content://*"),
                                                          new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          context),
                                 new ProviderMappingGroup("default-storage",
                                                          new WildcardUriMatch("storage://*"),
                                                          new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          context),
                                 new ProviderMappingGroup("default-assets",
                                                          new WildcardUriMatch("assets://*"),
                                                          new UninstalledReadonlyProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          new UninstalledProviderSetup(metadata, context, new NoopProviderBootstrapper(), 0), 
                                                          context)
                             };
            HiveManager manager = new HiveManager(groups , context);

            // Act
            var writer = manager.OpenWriter<IContentStore>();

            // Assert
            Assert.IsNotNull(writer);
        }

        [TestCase]
        public void FileProvider_ViaManager_GetRelations_DefaultRelationType()
        {
            // Arrange
            var ioTestSetup = new IoHiveTestSetupHelper();
            var unitFactory = new ProviderUnitFactory(ioTestSetup.EntityRepositoryFactory);
            var readonlyUnitFactory = new ReadonlyProviderUnitFactory(ioTestSetup.EntityRepositoryFactory);
            var provider = new ProviderSetup(unitFactory, ioTestSetup.ProviderMetadata, null, null, 0);
            var readonlyProvider = new ReadonlyProviderSetup(readonlyUnitFactory, ioTestSetup.ProviderMetadata, null, null, 0);

            var providerMappingGroup = new ProviderMappingGroup("default", new WildcardUriMatch("storage://*"), readonlyProvider, provider, ioTestSetup.FrameworkContext);

            var manager = new HiveManager(new[] { providerMappingGroup }, ioTestSetup.FrameworkContext);

            var actualFile = ioTestSetup.TestDirectory.GetFiles(ioTestSetup.Settings.SupportedExtensions, SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(ioTestSetup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();
            var parentFolder = actualFile.Directory;

            // Act
            using (var store = manager.OpenReader<IFileStore>())
            {
                var file = store.Repositories.Get<File>(new HiveId(actualFile.FullName));
                var parentRelations = store.Repositories.GetParentRelations(file.Id, FixedRelationTypes.DefaultRelationType);
                var parentsViaHandyMethod = store.Repositories.GetParentFileRelations(file);
                var firstParentRelation = store.Repositories.Get<File>(parentRelations.First().SourceId);

                //Assert
                // Check for iterator block mistakes
                Assert.That(parentRelations, Is.EquivalentTo(parentsViaHandyMethod));
                Assert.That(parentRelations.Count(), Is.GreaterThanOrEqualTo(1));
                Assert.AreEqual(parentFolder.FullName.NormaliseDirectoryPath(), firstParentRelation.RootedPath.NormaliseDirectoryPath());
            }
        }
    }
}
