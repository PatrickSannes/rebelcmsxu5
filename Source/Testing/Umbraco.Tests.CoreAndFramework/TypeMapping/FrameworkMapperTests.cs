using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Framework.TypeMapping;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.TypeMapping
{
    [TestFixture]
    public class FrameworkMapperTests : AbstractPartialTrustFixture<FrameworkMapperTests>
    {

        private FrameworkModelMapper _frameworkMapper;

        #region LocalizedString <-> string

        [Test]
        public void LocalizedString_To_String()
        {
            var input = new LocalizedString("hello");
            var output = _frameworkMapper.Map<string>(input);
            Assert.AreEqual("hello", output);
        }

        [Test]
        public void String_To_LocalizedString()
        {
            var input = "hello";
            var output = _frameworkMapper.Map<LocalizedString>(input);
            Assert.AreEqual("hello", (string)output);
        }

        #endregion

        public override void TestSetup()
        {
            _frameworkMapper = new FrameworkModelMapper(new FakeFrameworkContext());
            _frameworkMapper.Configure();
        }

        public override void TestTearDown()
        {            
        }
    }
}