using System;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    [TestFixture]
    public class LazyRelationTests
    {
        [Test]
        public void WhenSourceIsAccessed_LazyRelation_CallsRepository_WithBothSourceAndDestinationId()
        {
            // Arrange
            var context = new FakeFrameworkContext();
            var providerGroup = GroupedProviderMockHelper.GenerateProviderGroup(1, 0, 50, context);
            var idRoot = new Uri("oh-yeah://this-is-my-root/");
            var groupUnitFactory = new GroupUnitFactory(providerGroup.Writers, idRoot, FakeHiveCmsManager.CreateFakeRepositoryContext(context), context);
            HiveId int1 = HiveId.ConvertIntToGuid(1);
            HiveId int2 = HiveId.ConvertIntToGuid(2);

            // Act & Assert
            using (var uow = groupUnitFactory.Create())
            {
                var lazyRelation = new LazyRelation<TypedEntity>(uow.Repositories, FixedRelationTypes.DefaultRelationType, int1, int2, 0);

                IReadonlyRelation<IRelatableEntity, IRelatableEntity> blah = lazyRelation;
                IRelationById blah2 = lazyRelation;

                Assert.False(lazyRelation.IsLoaded);
                var source = lazyRelation.Source;
                Assert.True(lazyRelation.IsLoaded);
                var dest = lazyRelation.Destination;

                Assert.NotNull(source);
                Assert.NotNull(dest);
            }
        }
    }
}
