using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;

using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Context
{
    using Umbraco.Cms.Web.Model.BackOffice.Editors;
    using Umbraco.Cms.Web.Security;
    using Umbraco.Cms.Web.Security.Permissions;
    using Umbraco.Framework.Persistence.Model.Versioning;
    using Umbraco.Framework.Security;
    using Umbraco.Hive;
    using Umbraco.Hive.RepositoryTypes;

    /// <summary>
    /// The default IRenderModelFactory
    /// </summary>
    public class DefaultRenderModelFactory : IRenderModelFactory
    {
        private const string ResponseLifecycleCacheKey = "DefaultRenderModelFactory-b23r98ysjdnfsuk";
        private readonly IUmbracoApplicationContext _applicationContext;

        public DefaultRenderModelFactory(IUmbracoApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// Creates or returns an existing IUmbracoRenderModel based on the request data
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="rawUrl"></param>
        /// <returns></returns>
        public IUmbracoRenderModel Create(HttpContextBase httpContext, string rawUrl)
        {
            using (DisposableTimer.TraceDuration<DefaultRenderModelFactory>("Begin find/create context", "End find/create"))
            {
                return _applicationContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IUmbracoRenderModel>(ResponseLifecycleCacheKey, () =>
                    {
                        LogHelper.TraceIfEnabled<DefaultRenderModelFactory>("IUmbracoRenderModel requires creation");
                        var model = new UmbracoRenderModel(_applicationContext, () => ResolveItem(httpContext, rawUrl));
                        return model;
                    });
            }
        }

        private Content ResolveItem(HttpContextBase httpContext, string requestUrl)
        {
            using (DisposableTimer.TraceDuration<DefaultRenderModelFactory>("Begin ResolveItem", "End ResolveItem"))
            {
                //Sorry, gonna need to use the resolver here
                var urlUtility = DependencyResolver.Current.GetService<IRoutingEngine>();

                //var revisionStatusType = httpContext.Request.QueryString["revisionStatusType"];
                //var actualStatusType = (revisionStatusType.IsNullOrWhiteSpace()) ? FixedStatusTypes.Published : new RevisionStatusType(revisionStatusType, revisionStatusType);

                bool isPreview = false;
                bool.TryParse(httpContext.Request.QueryString[ContentEditorModel.PreviewQuerystringKey], out isPreview);

                var result = urlUtility.FindEntityByUrl(httpContext.Request.Url, isPreview ? null : FixedStatusTypes.Published);

                if (result != null && result.IsRoutable())
                {
                    var viewPermissionId = new ViewPermission().Id;
                    if (isPreview)
                    {
                        // First get the user from the backoffice membership provider, which may be different than from the site running
                        var user = BackOfficeAuthenticationModule.GetUmbracoBackOfficeIdentity(httpContext);

                        // Check user is allowed to preview
                        var userId = (user != null && user.IsAuthenticated)
                            ? user.Id
                            : HiveId.Empty;

                        PermissionResults permissionResult = this._applicationContext.Security.GetEffectivePermissions(userId, result.RoutableEntity.Id, viewPermissionId, new CreatePermission().Id);

                        if (!permissionResult.AreAllAllowed())
                        {
                            // Redirect to insufficient permissions pages
                            throw new HttpException((int)global::System.Net.HttpStatusCode.Forbidden, "You do not have permission to view this resource.");
                        }
                    }

                    var resultId = result.RoutableEntity.Id.AsEnumerableOfOne().ToArray();

                    using (var contentUow = _applicationContext.Hive.OpenReader<IContentStore>())
                    using (var securityUow = _applicationContext.Hive.OpenReader<ISecurityStore>())
                        resultId = resultId.FilterAnonymousWithPermissions(_applicationContext.Security, contentUow, securityUow, viewPermissionId).ToArray();

                    if (resultId.Length == 0)
                        throw new HttpException((int)global::System.Net.HttpStatusCode.Forbidden, "You do not have permission to view this resource.");

                    var content = _applicationContext.FrameworkContext.TypeMappers.Map<Content>(result.RoutableEntity);

                    // Swap out the current template if an alt template is in querystring, and current node has an alternative template with that alias
                    if (!string.IsNullOrWhiteSpace(httpContext.Request.QueryString["altTemplate"]))
                        content.TrySwapTemplate(httpContext.Request.QueryString["altTemplate"]);

                    return content;
                }

                LogHelper.TraceIfEnabled<DefaultRenderModelFactory>("Could not find item for route '{0}'", () => requestUrl);
                return null;
            }
        }
    }
}
