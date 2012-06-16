using System;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Hive
{
    [TestFixture]
    public class HiveProviderHelperTests
    {
        [Test]
        public void MultipleTransactionCommits_ThrowsError()
        {
            // Arrange
            var context = new FakeFrameworkContext();
            var provider = GroupedProviderMockHelper.MockEntityRepositoryFactory(1, new ProviderMetadata("test", new Uri("test://"), true, false), context);
            var factory = new ProviderUnitFactory(provider);

            // Act
            using (var uow = factory.Create())
            {
                uow.Complete();
                Assert.Throws<TransactionCompletedException>(uow.Complete, "Second completion did not throw exception");
            }
        }

        [Test]
        public void  WhenProviderMetadataIsNotPassthrough_SettingProviderId_OnHiveId_Succeeds()
        {
            // Arrange
            var metadata = new ProviderMetadata("test", new Uri("mappingroot://"), false, false);
            var originalId = new HiveId(1);

            // Act
            var newId = ProviderRepositoryHelper.CreateMappedProviderId(metadata, originalId);

            // Assert
            Assert.NotNull(newId.ProviderId);
            Assert.AreEqual(metadata.Alias, newId.ProviderId);
        }

        [Test]
        public void WhenProviderMetadataIsPassthrough_ProviderId_IsNotSet_OnHiveId()
        {
            // Arrange
            var metadata = new ProviderMetadata("test", new Uri("mappingroot://"), false, true);
            var originalId = new HiveId(1);

            // Act
            var newId = ProviderRepositoryHelper.CreateMappedProviderId(metadata, originalId);

            // Assert
            Assert.IsNull(newId.ProviderId);
            Assert.AreNotEqual(metadata.Alias, newId.ProviderId);
        }

        [Test]
        public void WhenProviderMetadataIsNotPassthrough_ButProviderIdIsAlreadySet_ProviderId_IsNotOverriden_OnHiveId()
        {
            // Arrange
            var metadata = new ProviderMetadata("test", new Uri("mappingroot://"), false, false);
            var originalId = new HiveId(new Uri("myroot://"), "myprovider", new HiveIdValue(1));

            // Act
            var newId = ProviderRepositoryHelper.CreateMappedProviderId(metadata, originalId);

            // Assert
            Assert.NotNull(newId.ProviderId);
            Assert.AreNotEqual(metadata.Alias, newId.ProviderId);
            Assert.AreEqual(originalId.ProviderId, newId.ProviderId);
        }
    }
}
