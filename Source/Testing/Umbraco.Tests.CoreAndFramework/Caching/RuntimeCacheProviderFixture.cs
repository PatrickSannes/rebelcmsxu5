namespace Umbraco.Tests.CoreAndFramework.Caching
{
    using System.Web;
    using NUnit.Framework;
    using Umbraco.Framework.Caching;

    [TestFixture]
    public class RuntimeCacheProviderFixture : AbstractCacheProviderFixture
    {
        #region Public Methods

        [TestFixtureSetUp]
        public override void SetUp()
        {
            CacheProvider = new RuntimeCacheProvider();
        }

        #endregion
    }
}