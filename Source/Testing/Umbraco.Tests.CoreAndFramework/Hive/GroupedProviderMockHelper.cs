using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.Core;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    using Umbraco.Framework.Diagnostics;

    public class GroupedProviderMockHelper
    {
        public static readonly int MockRelationCount = 10;

        public static ProviderMappingGroup GenerateProviderGroup(int numberOfProviders, int numberOfPassthroughProviders, int numberOfMockedEntities, IFrameworkContext frameworkContext)
        {
            Func<int, bool, IEnumerable<Tuple<ProviderSetup, ReadonlyProviderSetup>>> generate = (count, isPassthrough) =>
                {
                    var generateProviders = new List<Tuple<ProviderSetup, ReadonlyProviderSetup>>();
                    for (int i = 0; i < count; i++)
                    {
                        var mockKey = "test" + (isPassthrough ? "_passthrough" : String.Empty) + i;
                        var metadata = new ProviderMetadata(mockKey, new Uri("content://"), true, isPassthrough);
                        var factory = MockEntityRepositoryFactory(numberOfMockedEntities, metadata, frameworkContext);
                        var setup = new ProviderSetup(new ProviderUnitFactory(factory), metadata, frameworkContext, null, 0);
                        var readonlySetup = new ReadonlyProviderSetup(new ReadonlyProviderUnitFactory(factory), metadata, frameworkContext, null, 0);
                        generateProviders.Add(new Tuple<ProviderSetup, ReadonlyProviderSetup>(setup, readonlySetup));
                    }
                    return generateProviders;
                };

            var uris = new[] { "content://boots/", "content://bags/", "content://shoes/" };
            var uriMatches = EnumerableExtensions.Range(count => new WildcardUriMatch(new Uri(uris[count])), 3);
            var injectMockedProviders = generate.Invoke(numberOfProviders, false)
                .Concat(generate.Invoke(numberOfPassthroughProviders, true));
            return new ProviderMappingGroup("test", uriMatches, injectMockedProviders.Select(x => x.Item2), injectMockedProviders.Select(x => x.Item1), new FakeFrameworkContext());
        }

        private class TransactionMock : IProviderTransaction
        {
            public void Dispose()
            {
                return;
            }

            public bool IsTransactional { get { return true; } }

            private bool _isActive = true;
            public bool IsActive { get { return _isActive; } }

            /// <summary>
            /// Gets a value indicating whether this instance was committed.
            /// </summary>
            /// <value><c>true</c> if committed; otherwise, <c>false</c>.</value>
            public bool WasCommitted { get; private set; }

            /// <summary>
            /// Gets a value indicating whether this instance was rolled back.
            /// </summary>
            /// <value><c>true</c> if rolled back; otherwise, <c>false</c>.</value>
            public bool WasRolledBack { get; private set; }

            public void Rollback(IProviderUnit work)
            {
                WasRolledBack = true;
                _isActive = false;
            }

            public void Commit(IProviderUnit work)
            {
                try
                {
                    CacheFlushActions.IfNotNull(x => x.ForEach(y => y.Invoke()));
                }
                catch (Exception ex)
                {
                    LogHelper.Error<TransactionMock>("Could not perform an action on completion of a transaction. The transaction will continue.", ex);
                } 
                WasCommitted = true;
                _isActive = false;
            }

            public string GetTransactionId()
            {
                return GetHashCode().ToString();
            }

            private readonly List<Action> _cacheFlushActions = new List<Action>();
            /// <summary>
            /// Gets a list of actions that will be performed on completion, for example cache updating
            /// </summary>
            /// <value>The on completion actions.</value>
            public List<Action> CacheFlushActions
            {
                get
                {
                    return _cacheFlushActions;
                }
            }
        }

        public static AbstractEntityRepositoryFactory MockEntityRepositoryFactory(int allItemCount, ProviderMetadata metadata, IFrameworkContext frameworkContext)
        {
            var entitySession = Substitute.For<AbstractEntityRepository>(metadata, new TransactionMock(), frameworkContext);

            entitySession.CanReadRelations.Returns(true);
            entitySession.CanWriteRelations.Returns(true);

            entitySession
                .PerformGet<TypedEntity>(Arg.Any<bool>(), Arg.Any<HiveId[]>())
                .Returns(x => EnumerableExtensions.Range(count => HiveModelCreationHelper.MockTypedEntity(HiveId.ConvertIntToGuid(count + 1)), 3));

            entitySession
                .PerformGetAll<TypedEntity>()
                .Returns(x => EnumerableExtensions.Range(count => HiveModelCreationHelper.MockTypedEntity(HiveId.ConvertIntToGuid(count + 5)), allItemCount));

            entitySession
                .PerformGetAncestorRelations(Arg.Any<HiveId>(), Arg.Any<RelationType>())
                .Returns(MockRelations(MockRelationCount));

            entitySession
                .PerformGetDescendentRelations(Arg.Any<HiveId>(), Arg.Any<RelationType>())
                .Returns(MockRelations(MockRelationCount));

            entitySession
                .PerformGetParentRelations(Arg.Any<HiveId>(), Arg.Any<RelationType>())
                .Returns(MockRelations(MockRelationCount));

            entitySession
                .PerformGetChildRelations(Arg.Any<HiveId>(), Arg.Any<RelationType>())
                .Returns(MockRelations(MockRelationCount));

            var factory = Substitute.For<AbstractEntityRepositoryFactory>(null, null, null, frameworkContext, null);
            factory.GetRepository().Returns(entitySession);
            return factory;
        }

        private static Func<CallInfo, IEnumerable<IReadonlyRelation<IRelatableEntity, IRelatableEntity>>> MockRelations(int allItemCount)
        {
            return x =>
                {
                    var items = EnumerableExtensions.Range(
                        count =>
                        new Relation(
                            FixedRelationTypes.DefaultRelationType,
                            HiveModelCreationHelper.MockTypedEntity(HiveId.ConvertIntToGuid(count + 1)), // We'll use ids starting at 1 for the 'source', with the dest being this + 5 so it creates some child / parent relations over the course of 50 created
                            HiveModelCreationHelper.MockTypedEntity(HiveId.ConvertIntToGuid(count + 5)),
                            0),
                        allItemCount).ToArray();
                    return items;
                };
        }
    }
}