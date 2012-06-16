using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using NSubstitute.Core;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Extensions
{
    using Umbraco.Hive.ProviderSupport;

    /// <summary>
    /// Utility class used to mock hive
    /// </summary>
    public static class MockHiveManager
    {
        /// <summary>
        /// Gets a mocked IHiveManager.
        /// </summary>
        /// <param name="frameworkContext">The framework context.</param>
        /// <returns></returns>
        public static IHiveManager GetManager(IFrameworkContext frameworkContext = null)
        {
            if (frameworkContext == null) frameworkContext = new FakeFrameworkContext();

            var hive = Substitute.For<IHiveManager>();
            hive.FrameworkContext.Returns(frameworkContext);

            return hive;
        }

        /// <summary>
        /// Creates a new IHiveManager and creates substitutes for the IFileStore
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="readonlyEntityRepositoryGroup">The readonly entity session.</param>
        /// <param name="readonlySchemaRepositoryGroup">The readonly schema session.</param>
        /// <param name="entityRepository">The entity session.</param>
        /// <param name="schemaSession">The schema session.</param>
        /// <returns></returns>
        public static IHiveManager MockFileStore(
            this IHiveManager hive,
            out IReadonlyEntityRepositoryGroup<IFileStore> readonlyEntityRepositoryGroup,
            out IReadonlySchemaRepositoryGroup<IFileStore> readonlySchemaRepositoryGroup,
            out IEntityRepositoryGroup<IFileStore> entityRepository,
            out ISchemaRepositoryGroup<IFileStore> schemaSession)
        {
            return hive.MockStore("storage://", out readonlyEntityRepositoryGroup, out readonlySchemaRepositoryGroup, out entityRepository, out schemaSession);
        }

        /// <summary>
        /// Creates a new IHiveManager and creates substitutes for the IMediaStore
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="readonlyEntityRepositoryGroup">The readonly entity session.</param>
        /// <param name="readonlySchemaRepositoryGroup">The readonly schema session.</param>
        /// <param name="entityRepository">The entity session.</param>
        /// <param name="schemaSession">The schema session.</param>
        /// <returns></returns>
        public static IHiveManager MockMediaStore(
            this IHiveManager hive,
            out IReadonlyEntityRepositoryGroup<IMediaStore> readonlyEntityRepositoryGroup,
            out IReadonlySchemaRepositoryGroup<IMediaStore> readonlySchemaRepositoryGroup,
            out IEntityRepositoryGroup<IMediaStore> entityRepository,
            out ISchemaRepositoryGroup<IMediaStore> schemaSession)
        {
            return hive.MockStore("media://", out readonlyEntityRepositoryGroup, out readonlySchemaRepositoryGroup, out entityRepository, out schemaSession);
        }

        /// <summary>
        /// Creates a new IHiveManager and creates substitutes for the IContentStore
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="readonlyEntityRepositoryGroup">The readonly entity session.</param>
        /// <param name="readonlySchemaRepositoryGroup">The readonly schema session.</param>
        /// <param name="entityRepository">The entity session.</param>
        /// <param name="schemaSession">The schema session.</param>
        /// <returns></returns>
        public static IHiveManager MockContentStore(
            this IHiveManager hive,
            out IReadonlyEntityRepositoryGroup<IContentStore> readonlyEntityRepositoryGroup,
            out IReadonlySchemaRepositoryGroup<IContentStore> readonlySchemaRepositoryGroup,
            out IEntityRepositoryGroup<IContentStore> entityRepository,
            out ISchemaRepositoryGroup<IContentStore> schemaSession)
        {
            return hive.MockStore("content://", out readonlyEntityRepositoryGroup, out readonlySchemaRepositoryGroup, out entityRepository, out schemaSession);
        }

        /// <summary>
        /// Creates a new IHiveManager and creates substitutes for the ISecurityStore
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="readonlyEntityRepositoryGroup">The readonly entity session.</param>
        /// <param name="readonlySchemaRepositoryGroup">The readonly schema session.</param>
        /// <param name="entityRepository">The entity session.</param>
        /// <param name="schemaRepository">The schema session.</param>
        /// <returns></returns>
        public static IHiveManager MockSecurityStore(
            this IHiveManager hive,
            out IReadonlyEntityRepositoryGroup<ISecurityStore> readonlyEntityRepositoryGroup,
            out IReadonlySchemaRepositoryGroup<ISecurityStore> readonlySchemaRepositoryGroup,
            out IEntityRepositoryGroup<ISecurityStore> entityRepository,
            out ISchemaRepositoryGroup<ISecurityStore> schemaRepository)
        {
            return hive.MockStore("security://", out readonlyEntityRepositoryGroup, out readonlySchemaRepositoryGroup, out entityRepository, out schemaRepository);
        }

        /// <summary>
        /// Creates a new IHiveManager and creates substitutes for the IDictionaryStore
        /// </summary>
        /// <param name="hive">The hive.</param>
        /// <param name="readonlyEntityRepositoryGroup">The readonly entity session.</param>
        /// <param name="readonlySchemaRepositoryGroup">The readonly schema session.</param>
        /// <param name="entityRepository">The entity session.</param>
        /// <param name="schemaRepository">The schema session.</param>
        /// <returns></returns>
        public static IHiveManager MockDictionaryStore(
            this IHiveManager hive,
            out IReadonlyEntityRepositoryGroup<IDictionaryStore> readonlyEntityRepositoryGroup,
            out IReadonlySchemaRepositoryGroup<IDictionaryStore> readonlySchemaRepositoryGroup,
            out IEntityRepositoryGroup<IDictionaryStore> entityRepository,
            out ISchemaRepositoryGroup<IDictionaryStore> schemaRepository)
        {
            return hive.MockStore("dictionary://", out readonlyEntityRepositoryGroup, out readonlySchemaRepositoryGroup, out entityRepository, out schemaRepository);
        }

        /// <summary>
        /// Mocks a store on the provided IHiveManager mock
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <param name="hive">The hive.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="readonlyEntitySession">The readonly entity session.</param>
        /// <param name="readonlySchemaSession">The readonly schema session.</param>
        /// <param name="entityRepository">The entity session.</param>
        /// <param name="schemaRepository">The schema session.</param>
        /// <returns></returns>
        public static IHiveManager MockStore<TFilter>(
            this IHiveManager hive,
            string uri,
            out IReadonlyEntityRepositoryGroup<TFilter> readonlyEntitySession,
            out IReadonlySchemaRepositoryGroup<TFilter> readonlySchemaSession,
            out IEntityRepositoryGroup<TFilter> entityRepository,
            out ISchemaRepositoryGroup<TFilter> schemaRepository)
            where TFilter : class, IProviderTypeFilter
        {

            //mock hive
            var hiveReader = Substitute.For<IReadonlyGroupUnit<TFilter>>();
            var hiveWriter = Substitute.For<IGroupUnit<TFilter>>();
            hiveReader.FrameworkContext.Returns(hive.FrameworkContext);
            hiveWriter.FrameworkContext.Returns(hive.FrameworkContext);

            //mock readers
            readonlyEntitySession = Substitute.For<IReadonlyEntityRepositoryGroup<TFilter>>();
            readonlySchemaSession = Substitute.For<IReadonlySchemaRepositoryGroup<TFilter>>();
            readonlyEntitySession.Schemas.Returns(readonlySchemaSession);
            hiveReader.Repositories.Returns(readonlyEntitySession);

            //mock writers
            entityRepository = Substitute.For<IEntityRepositoryGroup<TFilter>>();
            entityRepository.FrameworkContext.Returns(hive.FrameworkContext);

            schemaRepository = Substitute.For<ISchemaRepositoryGroup<TFilter>>();
            entityRepository.Schemas.Returns(schemaRepository);
            hiveWriter.Repositories.Returns(entityRepository);

            RepositoryContext fakeRepositoryContext = FakeHiveCmsManager.CreateFakeRepositoryContext(hive.FrameworkContext);
            var readonlyGroupUnitFactory = Substitute.For<ReadonlyGroupUnitFactory<TFilter>>(Enumerable.Empty<ReadonlyProviderSetup>(), new Uri(uri), fakeRepositoryContext,  hive.FrameworkContext);
            readonlyGroupUnitFactory.CreateReadonly().Returns(hiveReader);
            readonlyGroupUnitFactory.CreateReadonly<TFilter>().Returns(hiveReader);

            var groupUnitFactory = Substitute.For<GroupUnitFactory<TFilter>>(Enumerable.Empty<ProviderSetup>(), new Uri(uri), fakeRepositoryContext, hive.FrameworkContext);
            groupUnitFactory.Create().Returns(hiveWriter);
            groupUnitFactory.Create<TFilter>().Returns(hiveWriter);

            hive.GetReader<TFilter>().Returns(readonlyGroupUnitFactory);
            hive.GetReader<TFilter>(null).ReturnsForAnyArgs(readonlyGroupUnitFactory);

            hive.GetWriter<TFilter>().Returns(groupUnitFactory);
            hive.GetWriter<TFilter>(null).ReturnsForAnyArgs(groupUnitFactory);

            return hive;
        }

        public static Func<CallInfo, IEnumerable<T>> MockReturnForGet<T>()
            where T : TypedEntity, new()
        {
            return x =>
                   x.Args()
                       .OfType<HiveId[]>()
                       .First()
                       .Select(id => new T() {Id = id}).ToList();
        }
    }
}
