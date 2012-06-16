using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;

namespace Umbraco.Cms.Web.DependencyManagement.DemandBuilders
{
    /// <summary>
    /// Adds all abstracted web objects to the container per request
    /// </summary>
    /// <remarks>
    /// logic taken from the AutofacWebTypesModule: http://code.google.com/p/autofac/source/browse/src/Source/Autofac.Integration.Mvc/AutofacWebTypesModule.cs
    /// </remarks>
    public class WebTypesDemandBuilder : IDependencyDemandBuilder
    {
        private readonly HttpApplication _app;

        public WebTypesDemandBuilder(HttpApplication app)
        {
            _app = app;
        }

        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {

            containerBuilder.ForFactory(x => new HttpContextWrapper(HttpContext.Current))
                .KnownAs<HttpContextBase>();

            // HttpContext properties

            containerBuilder.ForFactory(x => x.Resolve<HttpContextBase>().Request)
                .KnownAsSelf()
                .ScopedAs.HttpRequest();

            containerBuilder.ForFactory(x => x.Resolve<HttpContextBase>().Response)
                .KnownAsSelf()
                .ScopedAs.HttpRequest();

            containerBuilder.ForFactory(x => x.Resolve<HttpContextBase>().Server)
                .KnownAsSelf()
                .ScopedAs.HttpRequest();

            containerBuilder.ForFactory(x => x.Resolve<HttpContextBase>().Session)
                .KnownAsSelf()
                .ScopedAs.HttpRequest();

            containerBuilder.ForFactory(x => x.Resolve<HttpContextBase>().Application)
                .KnownAsSelf()
                .ScopedAs.HttpRequest();

            // HttpRequest properties

            containerBuilder.ForFactory(x => x.Resolve<HttpRequestBase>().Browser)
                .KnownAsSelf()
                .ScopedAs.HttpRequest();

            containerBuilder.ForFactory(x => x.Resolve<HttpRequestBase>().Files)
                .KnownAsSelf()
                .ScopedAs.HttpRequest();

            // HttpResponse properties

            containerBuilder.ForFactory(x => x.Resolve<HttpResponseBase>().Cache)
                .KnownAsSelf()
                .ScopedAs.HttpRequest();

            // HostingEnvironment properties

            containerBuilder.ForFactory(x => HostingEnvironment.VirtualPathProvider)
                .KnownAsSelf()
                .ScopedAs.HttpRequest();

            //Url Helper

            Func<IResolutionContext, UrlHelper> urlHelperDelegate = x =>
                    {
                        var httpContextBase = x.Resolve<HttpContextBase>();
                        if (httpContextBase != null)
                        {
                            try
                            {
                                if (httpContextBase.Request != null)
                                    return
                                        new UrlHelper(
                                            httpContextBase.Request.
                                                RequestContext);
                            }
                            catch (HttpException ex)
                            {
                                LogHelper.Error<WebTypesDemandBuilder>("Tried to get a UrlHelper before Request is available on context", ex);
                            }
                        }
                        return new UrlHelper(new RequestContext(httpContextBase, new RouteData()));
                    };

            containerBuilder.ForFactory(urlHelperDelegate)
                .KnownAsSelf();
        }
    }
}