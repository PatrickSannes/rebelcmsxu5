namespace Umbraco.Tests.CoreAndFramework.Caching
{
    using NUnit.Framework;
    using Umbraco.Framework.Caching;
    using Umbraco.Tests.Extensions;

    [TestFixture]
    public class PerHttpRequestCacheProviderFixture : AbstractCacheProviderFixture
    {
        #region Public Methods

        [TestFixtureSetUp]
        public override void SetUp()
        {
            var ctx = new FakeHttpContextFactory("/");
            CacheProvider = new PerHttpRequestCacheProvider(ctx.HttpContext);
        }

        #endregion
    }
}