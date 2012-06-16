using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ClientDependency.Core;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.System;
using Umbraco.Cms.Web.Trees;

using Umbraco.Framework;
using System.Web.Routing;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web
{

    /// <summary>
    /// UrlHelper extension methods
    /// </summary>
    public static class UrlHelperExtensions
    {

        #region GetDashboardUrl Extensions

        /// <summary>
        /// Returns the dashboard url for the currently active application
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetCurrentDashboardUrl(this UrlHelper url)
        {
            var currApp = "content";
            if (url.RequestContext.HttpContext.Request.Cookies["UMBAPP"] != null)
            {
                currApp = url.RequestContext.HttpContext.Request.Cookies["UMBAPP"].Value;
            }
            return url.GetDashboardUrl(currApp);
        }

        /// <summary>
        /// Returns the dashboard url for the currently active application
        /// </summary>
        /// <param name="url"></param>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public static string GetCurrentDashboardUrl(this UrlHelper url, Type controllerType)
        {
            var currApp = "content";
            if (url.RequestContext.HttpContext.Request.Cookies["UMBAPP"] != null)
            {
                currApp = url.RequestContext.HttpContext.Request.Cookies["UMBAPP"].Value;
            }
            return url.GetDashboardUrl(controllerType, currApp);
        }

        /// <summary>
        /// Returns the default dashboard url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="appAlias">The application (i.e. content) to render the dashboard for</param>
        /// <returns></returns>
        public static string GetDashboardUrl(this UrlHelper url, string appAlias)
        {
            var dashboardControllerName = UmbracoController.GetControllerName(typeof(DashboardEditorController));
            var dashboardControllerId =
                UmbracoController.GetControllerId<EditorAttribute>(typeof(DashboardEditorController));
            return url.Action("Dashboard",
                                dashboardControllerName,
                                new { editorId = dashboardControllerId.ToString("N"), appAlias = appAlias });
        }

        /// <summary>
        /// Returns the dashboard Url for a specified controller
        /// </summary>
        /// <param name="url"></param>
        /// <param name="controllerType"></param>
        /// <param name="appAlias">The application (i.e. content) to render the dashboard for</param>
        /// <returns></returns>
        public static string GetDashboardUrl(this UrlHelper url, Type controllerType, string appAlias)
        {
            Mandate.ParameterNotNull(controllerType, "controllerType");

            if (!controllerType.GetCustomAttributes(typeof(EditorAttribute), false).Any())
            {
                throw new InvalidCastException("The controller type specified is not of type BaseEditorController");
            }
            return url.Action("Dashboard",
                                UmbracoController.GetControllerName(controllerType),
                                new
                                    {
                                        editorId = UmbracoController.GetControllerId<EditorAttribute>(controllerType).ToString("N"),
                                        appAlias = appAlias
                                    });
        }
        #endregion

        //public static string IdToUrlString(this HiveId id)
        //{
        //    return id.ToString().ToUrlBase64();
        //}

        #region GetSurfaceUrl Extensions

        /// <summary>
        /// Returns the controller/action URL for the given Surface
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="id"></param>
        /// <param name="surfaceId"></param>
        /// <returns></returns>
        public static string GetSurfaceUrl(this UrlHelper url, string action, string id, Guid surfaceId)
        {
            //TODO: Is this the best way to get access to this service? Trying not to use the DependencyResolver for much
            var surfaceMetadata = DependencyResolver.Current.GetService<ComponentRegistrations>()
                .SurfaceControllers
                .Where(x => x.Metadata.Id == surfaceId)
                .SingleOrDefault();
            if (surfaceMetadata == null)
                throw new InvalidOperationException("Could not find the surface controller with id " + surfaceId);

            var settings = DependencyResolver.Current.GetService<UmbracoSettings>();
            var area = settings.UmbracoPaths.BackOfficePath;
            var idVal = string.IsNullOrEmpty(id) ? (object)UrlParameter.Optional : id;

            //now, need to figure out what area this editor belongs too...
            var pluginDefition = surfaceMetadata.Metadata.PluginDefinition;
            if (pluginDefition.HasRoutablePackageArea())
            {
                area = pluginDefition.PackageName;    
            }

            var resolvedUrl = url.Action(action, surfaceMetadata.Metadata.ControllerName, new { area, id = idVal, surfaceId = surfaceId.ToString("N") });
            return resolvedUrl;
        }



        #endregion

        #region GetEditorUrl Extensions

        /// <summary>
        /// Returns the editor url using the default action
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="id">The id.</param>
        /// <param name="editorId">The editor id.</param>
        /// <param name="preResolvedComponentRegistrations">The pre resolved component registrations.</param>
        /// <param name="preResovledSettings"></param>
        /// <returns></returns>
        public static string GetEditorUrl(this UrlHelper url, HiveId id, Guid editorId, ComponentRegistrations preResolvedComponentRegistrations, UmbracoSettings preResovledSettings)
        {
            return url.GetEditorUrl("Edit", id, editorId, preResolvedComponentRegistrations, preResovledSettings);
        }

        /// <summary>
        /// Returns the full unique url for an Editor
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="action">The action.</param>
        /// <param name="id">A HiveId object or null if no id is required</param>
        /// <param name="editorId">The editor id.</param>
        /// <param name="preResolvedComponentRegistrations">The pre resolved component registrations.</param>
        /// <param name="preResovledSettings"></param>
        /// <returns></returns>
        public static string GetEditorUrl(this UrlHelper url, string action, HiveId? id, Guid editorId, ComponentRegistrations preResolvedComponentRegistrations, UmbracoSettings preResovledSettings)
        {
            var idVal = GetIdVal(id);
            return url.GetEditorUrl(action, editorId, new { id = idVal }, preResolvedComponentRegistrations, preResovledSettings);
        }

        public static string GetEditorUrl(this UrlHelper url, string action, Guid editorId, object actionParams)
        {
            var preResolvedComponentRegistrations = DependencyResolver.Current.GetService<ComponentRegistrations>();
            var settings = DependencyResolver.Current.GetService<UmbracoSettings>();
            return GetEditorUrl(url, action, editorId, actionParams, preResolvedComponentRegistrations, settings);
        }

        public static string GetEditorUrl(this UrlHelper url, string action, Guid editorId, object actionParams, ComponentRegistrations preResolvedComponentRegistrations, UmbracoSettings preResovledSettings)
        {
            
            var editorMetaData = preResolvedComponentRegistrations
                .EditorControllers
                .Where(x => x.Metadata.Id == editorId)
                .SingleOrDefault();

            if (editorMetaData == null)
                throw new InvalidOperationException("Could not find the editor controller with id " + editorId);
            
            var routeValDictionary = new RouteValueDictionary(actionParams);
            routeValDictionary["editorId"] = editorId.ToString("N");

            var area = preResovledSettings.UmbracoPaths.BackOfficePath;

            //now, need to figure out what area this editor belongs too...
            var pluginDefinition = editorMetaData.Metadata.PluginDefinition;            
            if (pluginDefinition.HasRoutablePackageArea())
            {
                area = pluginDefinition.PackageName;
            }

            //add the plugin area to our collection
            routeValDictionary["area"] = area;

            var resolvedUrl = url.Action(action, editorMetaData.Metadata.ControllerName, routeValDictionary);
            return resolvedUrl;
        }

        private static object GetIdVal(HiveId? id)
        {
            object idVal = null;
            if (id == null)
            {
                idVal = UrlParameter.Optional;
            }
            else
            {
                idVal = id;
            }
            return idVal;
        }

        /// <summary>
        /// Return the editor url using the default action
        /// </summary>
        /// <param name="url"></param>
        /// <param name="controllerType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetEditorUrl(this UrlHelper url, Type controllerType, HiveId? id)
        {
            return url.GetEditorUrl("Edit", controllerType, id);
        }

        //public static string GetEditorUrl(this UrlHelper url, Type controllerType, object id)
        //{
        //    return url.GetEditorUrl("Edit", controllerType, id);
        //}

        /// <summary>
        /// Returns the full unique url for an Editor based on it's type
        /// </summary>
        public static string GetEditorUrl(this UrlHelper url, string action, Type controllerType, HiveId? id)
        {
            if (!controllerType.GetCustomAttributes(typeof(EditorAttribute), false).Any())
            {
                throw new InvalidCastException("The controller type specified is not decorated with an EditorAttribute");
            }
            var componentRegistrations = DependencyResolver.Current.GetService<ComponentRegistrations>();
            var settings = DependencyResolver.Current.GetService<UmbracoSettings>();
            return url.GetEditorUrl(action,                                    
                                    id,
                                    UmbracoController.GetControllerId<EditorAttribute>(controllerType),
                                    componentRegistrations,
                                    settings);
        }
        #endregion

        #region GetTreeUrl Extensions

        /// <summary>
        /// Returns the url for searching using JSON
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetTreeSearchUrl(this UrlHelper url)
        {
            var appTreeControllerName = UmbracoController.GetControllerName(typeof(ApplicationTreeController));
            var appTreeControllerId = UmbracoController.GetControllerId<TreeAttribute>(typeof(ApplicationTreeController));
            return url.Action("Search", appTreeControllerName, new { area = url.GetBackOfficeArea(), treeId = appTreeControllerId.ToString("N") });
            
        }

        /// <summary>
        /// Return the tree url for a specified application
        /// </summary>
        /// <param name="url"></param>
        /// <param name="applicationAlias"></param>
        /// <returns></returns>
        public static string GetTreeUrl(this UrlHelper url, string applicationAlias)
        {
            var appTreeControllerName = UmbracoController.GetControllerName(typeof(ApplicationTreeController));
            var appTreeControllerId =
                UmbracoController.GetControllerId<TreeAttribute>(typeof(ApplicationTreeController));
            return url.Action("Index", appTreeControllerName,
                              new { area = url.GetBackOfficeArea(), appAlias = applicationAlias, treeId = appTreeControllerId.ToString("N") });
        }

        /// <summary>
        /// Return the tree url for the default action 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public static string GetTreeUrl(this UrlHelper url, HiveId id, Guid treeId, object queryStrings)
        {
            return url.GetTreeUrl("Index",  id, treeId, queryStrings);
        }

        /// <summary>
        /// Return the tree url with using the default action
        /// </summary>
        /// <param name="url"></param>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public static string GetTreeUrl(this UrlHelper url, HiveId id, Guid treeId, FormCollection queryStrings)
        {
            return url.GetTreeUrl("Index", id, treeId, queryStrings);
        }

        /// <summary>
        /// Returns the full unique url for a Tree
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="id"></param>
        /// <param name="treeId"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public static string GetTreeUrl(this UrlHelper url, string action, HiveId id, Guid treeId, FormCollection queryStrings)
        {            
            var treeMetaData = DependencyResolver.Current.GetService<ComponentRegistrations>()
                .TreeControllers
                .Where(x => x.Metadata.Id == treeId)
                .SingleOrDefault();

            if (treeMetaData == null)
                throw new InvalidOperationException("Could not find the tree controller with id " + treeId);

            var area = url.GetBackOfficeArea();

            //now, need to figure out what area this tree belongs too...
            var pluginDefition = treeMetaData.Metadata.PluginDefinition;            
            if (pluginDefition.HasRoutablePackageArea())
            {
                area = pluginDefition.PackageName;                
            }

            return url.Action(action, treeMetaData.Metadata.ControllerName, new { area, id = GetIdVal(id), treeId = treeId.ToString("N") })
                    + "?" + queryStrings.ToQueryString();

            
        }

        public static string GetTreeUrl(this UrlHelper url, string action, HiveId id, Guid treeId, object queryStrings)
        {
            var queryStringRoutes = new RouteValueDictionary(queryStrings);
            return url.GetTreeUrl(action, id, treeId, queryStringRoutes.ToFormCollection());
        }

        /// <summary>
        /// Get the tree url using the default action
        /// </summary>
        /// <param name="url"></param>
        /// <param name="treeType"></param>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public static string GetTreeUrl(this UrlHelper url, Type treeType, HiveId id, FormCollection queryStrings)
        {
            return url.GetTreeUrl("Index", treeType, id, queryStrings);
        }

        /// <summary>
        /// Returns the full unique url for a Tree based on it's type
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action"></param>
        /// <param name="treeType"></param>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        public static string GetTreeUrl(this UrlHelper url, string action, Type treeType, HiveId id, FormCollection queryStrings)
        {
            if (!treeType.GetCustomAttributes(typeof(TreeAttribute), false).Any())
            {
                throw new InvalidCastException("The controller type specified is not of type TreeController");
            }
            return url.GetTreeUrl(action,
                                  id,
                                  UmbracoController.GetControllerId<TreeAttribute>(treeType), queryStrings);
        }
        #endregion

        #region GetMediaUrl Extensions

        #region ByTypedEntity

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the given TypedEntity
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, TypedEntity entity)
        {
            return url.GetMediaUrl(entity, null, 0);
        }

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the given TypedEntity at the specific size
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, TypedEntity entity, int size)
        {
            return url.GetMediaUrl(entity, null, size);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the given TypedEntity
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, TypedEntity entity, string propertyAlias)
        {
            return url.GetMediaUrl(entity, propertyAlias, 0);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the given TypedEntity at the specific size
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, TypedEntity entity, string propertyAlias, int size)
        {
            //TODO: There is a lot of duplication between this and the MediaProxyController (with slight differences). Need to find a way to reuse code.

            var appContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>();
            using (var securityUow = appContext.Hive.OpenReader<ISecurityStore>())
            {
                // Get anonymous role
                var resultIds = entity.Id.AsEnumerableOfOne().ToArray();

                using (var contentUow = appContext.Hive.OpenReader<IContentStore>())
                    resultIds = resultIds.FilterAnonymousWithPermissions(appContext.Security, contentUow, securityUow, new Guid(FixedPermissionIds.View)).ToArray();

                // Check to see if anonymous view is allowed
                var anonymousViewAllowed = resultIds.Length != 0;

                // Get upload property
                var prop = propertyAlias.IsNullOrWhiteSpace()
                    ? entity.Attributes.SingleOrDefault(x => x.AttributeDefinition.AttributeType.RenderTypeProvider.InvariantEquals(CorePluginConstants.FileUploadPropertyEditorId))
                    : entity.Attributes.SingleOrDefault(x => x.AttributeDefinition.Alias == propertyAlias);
                
                if (prop == null || !prop.Values.ContainsKey("MediaId"))
                    return null; // Couldn't find property so return null

                var mediaId = prop.Values["MediaId"].ToString();
                var fileId = new HiveId(prop.Values["Value"].ToString());

                // Get the file
                using (var fileUow = appContext.Hive.OpenReader<IFileStore>(fileId.ToUri()))
                {
                    var file = fileUow.Repositories.Get<File>(fileId);

                    if (file == null)
                        return null; // Couldn't find file so return null

                    // Fetch the thumbnail
                    if (size > 0)
                    {
                        var relation = fileUow.Repositories.GetLazyChildRelations(fileId, FixedRelationTypes.ThumbnailRelationType)
                                .SingleOrDefault(x => x.MetaData.Single(y => y.Key == "size").Value == size.ToString());

                        file = (relation != null && relation.Destination != null)
                            ? (File)relation.Destination
                            : null;
                    }

                    if (file == null)
                        return null; // Couldn't find file so return null

                    if (anonymousViewAllowed && !file.PublicUrl.StartsWith("~/App_Data/")) // Don't proxy
                    {
                        return url.Content(file.PublicUrl);
                    }
                    else // Proxy
                    {
                        //NOTE: THIS IS TEMPORARY CODE UNTIL MEMBER PERMISSIONS IS DONE
                        if (anonymousViewAllowed)
                        {
                            // If they are anonymous, but media happens to be in app_data folder proxy
                            return url.Action("Proxy", "MediaProxy", new { area = "", propertyAlias = prop.AttributeDefinition.Alias, mediaId, size, fileName = file.Name });
                        }

                        return null;

                        // Check permissions
                        //var authAttr = new UmbracoAuthorizeAttribute {AllowAnonymous = true, Permissions = new [] { FixedPermissionIds.View }};
                        //var authorized = authAttr.IsAuthorized(url.RequestContext.HttpContext, entity.Id);
                        //if (!authorized)
                        //    return null; // Not authorized so return null

                        //return url.Action("Proxy", "MediaProxy", new { area = "", propertyAlias = prop.AttributeDefinition.Alias, mediaId, size, fileName = file.Name });
                    }
                }
            }
            
        }
        
        #endregion

        #region ByHiveId

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the TypedEntity with the given id
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, HiveId id)
        {
            return url.GetMediaUrl(id, null, 0);
        }

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the TypedEntity with the given id, at the specified size
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="id">The id.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, HiveId id, int size)
        {
            return url.GetMediaUrl(id, null, size);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the TypedEntity with the given id
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="id">The id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, HiveId id, string propertyAlias)
        {
            return url.GetMediaUrl(id, propertyAlias, 0);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the TypedEntity with the given id, at the specified size
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="id">The id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, HiveId id, string propertyAlias, int size)
        {
            var hive = DependencyResolver.Current.GetService<IHiveManager>();
            using (var uow = hive.OpenReader<IContentStore>(id.ToUri()))
            {
                var revision = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(id, FixedStatusTypes.Published);

                if (revision == null || revision.Item == null)
                    return null;

                return url.GetMediaUrl(revision.Item, propertyAlias, size);
            }
        }
        
        #endregion

        #region ByString

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the TypedEntity with the given id
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, string id)
        {
            return url.GetMediaUrl(HiveId.Parse(id));
        }

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the TypedEntity with the given id, at the specified size
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="id">The id.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, string id, int size)
        {
            return url.GetMediaUrl(HiveId.Parse(id), size);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the TypedEntity with the given id
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="id">The id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, string id, string propertyAlias)
        {
            return url.GetMediaUrl(HiveId.Parse(id), propertyAlias);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the TypedEntity with the given id, at the specified size
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="id">The id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public static string GetMediaUrl(this UrlHelper url, string id, string propertyAlias, int size)
        {
            return url.GetMediaUrl(HiveId.Parse(id), propertyAlias, size);
        }

        #endregion

        #endregion

        /// <summary>
        /// Returns the path of an embedded web resource
        /// </summary>
        /// <param name="url"></param>
        /// <param name="resourceType"></param>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public static string GetWebResourceUrl(this UrlHelper url, Type resourceType, string resourcePath)
        {
            var page = new Page();
            return page.ClientScript.GetWebResourceUrl(resourceType, resourcePath);
        }

        /// <summary>
        /// Return the ClientDependency styles path
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static BasicPath GetStylesPath(this UrlHelper url)
        {
            return new BasicPath("Styles", url.GetBackOfficeFolder() + "/Content/Styles");
        }

        /// <summary>
        /// Return the ClientDependency scripts path
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static BasicPath GetScriptsPath(this UrlHelper url)
        {
            return new BasicPath("Scripts", url.GetBackOfficeFolder() + "/Scripts");
        }

        /// <summary>
        /// Return the ClientDependency scripts path
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static BasicPath GetModulesPath(this UrlHelper url)
        {
            return new BasicPath("Modules", url.GetBackOfficeFolder() + "/Modules");
        }


        /// <summary>
        /// Gets the umbraco settings.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private static UmbracoSettings GetUmbracoSettings(this UrlHelper url)
        {
            var umbSettings = DependencyResolver.Current.GetService<UmbracoSettings>();
            return umbSettings;
        }

        /// <summary>
        /// Gets the back office area.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string GetBackOfficeArea(this UrlHelper url)
        {
            return url.Content(url.GetUmbracoSettings().UmbracoPaths.BackOfficePath);
        }

        /// <summary>
        /// Gets the back office folder.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string GetBackOfficeFolder(this UrlHelper url)
        {
            return url.Content(url.GetUmbracoSettings().UmbracoFolders.BackOfficeFolder);
        }

        /// <summary>
        /// Gets the application icons folder
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetAppIconsFolder(this UrlHelper url)
        {
            return url.Content(url.GetUmbracoSettings().UmbracoFolders.ApplicationIconFolder);
        }

        /// <summary>
        /// Gets the plugins folder
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetPluginsFolder(this UrlHelper url)
        {
            return url.Content(url.GetUmbracoSettings().PluginConfig.PluginsPath);
        }

    }
}
