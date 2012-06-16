using System.Web;
using System.Web.Routing;
using Umbraco.Cms.Web.Model;

namespace Umbraco.Cms.Web.Context
{
    public interface IRenderModelFactory
    {
        IUmbracoRenderModel Create(HttpContextBase httpContext, string rawUrl);
    }
}