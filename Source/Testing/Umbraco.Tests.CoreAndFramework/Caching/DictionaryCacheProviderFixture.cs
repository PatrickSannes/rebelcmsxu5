namespace Umbraco.Tests.CoreAndFramework.Caching
{
    using NUnit.Framework;
    using Umbraco.Framework.Caching;

    [TestFixture]
    public class DictionaryCacheProviderFixture : AbstractCacheProviderFixture
    {
        #region Public Methods

        [TestFixtureSetUp]
        public override void SetUp()
        {
            CacheProvider = new DictionaryCacheProvider();
        }

        #endregion
    }
}