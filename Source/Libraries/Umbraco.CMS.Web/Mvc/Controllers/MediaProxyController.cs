using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ActionInvokers;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mvc.Controllers
{
    [InstalledFilter]
    public class MediaProxyController : Controller, IRequiresRoutableRequestContext
    {
        private readonly static Regex SizePattern = new Regex(@".*_([0-9]+)\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IRoutableRequestContext RoutableRequestContext { get; set; }

        public MediaProxyController()
            : this(DependencyResolver.Current.GetService<IRoutableRequestContext>())
        { }

        public MediaProxyController(IRoutableRequestContext routableRequestContext)
        {
            ActionInvoker = new RoutableRequestActionInvoker(RoutableRequestContext);
            RoutableRequestContext = routableRequestContext;
        }

        public virtual ActionResult Proxy(string propertyAlias, string mediaId, int size, string fileName)
        {
            var app = RoutableRequestContext.Application;
            using (var uow = app.Hive.OpenReader<IContentStore>())
            {
                // Lookup a TypedEntity with an Upload field that has a MediaId of the mediaId property
                var entity =
                    uow.Repositories.SingleOrDefault(
                        x => x.Attribute<string>(propertyAlias, "MediaId") == mediaId);

                if (entity == null)
                    return HttpNotFound();

                // Check permissions
                //var authAttr = new UmbracoAuthorizeAttribute { AllowAnonymous = true, Permissions = new[] { FixedPermissionIds.View } };
                //var authorized = authAttr.IsAuthorized(HttpContext, entity.Id);
                //if (!authorized)
                //    return null; // Not authorized so return null

                //NOTE: THIS IS TEMPORARY CODE UNTIL MEMBER PERMISSIONS IS DONE
                // Just perform an Anonymour permission check
                var resultIds = entity.Id.AsEnumerableOfOne().ToArray();

                using (var securityUow = app.Hive.OpenReader<ISecurityStore>())
                    resultIds = resultIds.FilterAnonymousWithPermissions(app.Security, uow, securityUow, new Guid(FixedPermissionIds.View)).ToArray();

                if (resultIds.Length == 0 && !HttpContext.User.Identity.IsAuthenticated)
                {
                    return null;
                }

                //NOTE: END TEMP CODE

                // Find the upload property
                var property = entity.Attributes.SingleOrDefault(x => x.AttributeDefinition.AttributeType.RenderTypeProvider.Equals(CorePluginConstants.FileUploadPropertyEditorId, StringComparison.InvariantCultureIgnoreCase) && x.Values["MediaId"].ToString() == mediaId);

                if (property == null)
                    return HttpNotFound();

                // Get the file
                var fileId = new HiveId(property.DynamicValue);
                using (var uow2 = app.Hive.OpenReader<IFileStore>(fileId.ToUri()))
                {
                    var file = uow2.Repositories.Get<File>(fileId);

                    if (size > 0)
                    {
                        // Look for thubnail file
                        var relation = uow2.Repositories.GetLazyChildRelations(fileId, FixedRelationTypes.ThumbnailRelationType)
                            .SingleOrDefault(x => x.MetaData.Single(y => y.Key == "size").Value == size.ToString());

                        if (relation != null && relation.Destination != null)
                        {
                            var thumbnail = (File)relation.Destination;
                            return File(thumbnail.ContentBytes, thumbnail.GetMimeType());
                        }

                        return HttpNotFound();
                    }

                    if (file != null)
                        return File(file.ContentBytes, file.GetMimeType());
                }
            }

            return HttpNotFound();
        }
    }
}
