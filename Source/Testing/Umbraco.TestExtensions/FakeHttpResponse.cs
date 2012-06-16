using System.Web;

namespace Umbraco.Tests.Extensions
{
    internal class FakeHttpResponse : HttpResponseBase
    {
        public override string ApplyAppPathModifier(string virtualPath)
        {
            return virtualPath;
        }
    }
}