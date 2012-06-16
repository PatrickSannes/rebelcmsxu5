using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Dictionary;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using System;
using Umbraco.Cms.Web.Model.BackOffice.Editors;

namespace Umbraco.Cms.Web
{
    /// <summary>
    /// A utility class for use with front-end development containing many methods for accessing common APIs in Umbraco
    /// </summary>
    public class UmbracoHelper
    {
        private readonly UrlHelper _urlHelper;
        private readonly IRoutableRequestContext _requestContext;
        private readonly ControllerContext _controllerContext;
        private readonly IRenderModelFactory _modelFactory;
        private readonly Content _currentPage;

        public UmbracoHelper(ControllerContext controllerContext, IRoutableRequestContext requestContext, IRenderModelFactory modelFactory)
        {
            _requestContext = requestContext;
            _controllerContext = controllerContext;
            _modelFactory = modelFactory;

            _currentPage = _modelFactory.Create(_controllerContext.HttpContext, _controllerContext.HttpContext.Request.RawUrl).CurrentNode;

            _urlHelper = new UrlHelper(_controllerContext.RequestContext);
        }

        #region GetUrl

        /// <summary>
        /// Gets the URL for the given entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public string GetUrl(TypedEntity entity)
        {
            return _requestContext.RoutingEngine.GetUrl(entity.Id);
        }

        /// <summary>
        /// Gets the URL for the entity with the given id.
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public string GetUrl(HiveId entityId)
        {
            return _requestContext.RoutingEngine.GetUrl(entityId);
        }

        /// <summary>
        /// Gets the URL for the entity with the given id.
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public string GetUrl(string entityId)
        {
            return _requestContext.RoutingEngine.GetUrl(HiveId.Parse(entityId));
        }

        #endregion

        #region GetMediaUrl

        #region ByTypedEntity

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the given TypedEntity
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public string GetMediaUrl(TypedEntity entity)
        {
            return _urlHelper.GetMediaUrl(entity);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the given TypedEntity
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns></returns>
        public string GetMediaUrl(TypedEntity entity, string propertyAlias)
        {
            return _urlHelper.GetMediaUrl(entity, propertyAlias);
        }

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the given TypedEntity at the specific size
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public string GetMediaUrl(TypedEntity entity, int size)
        {
            return _urlHelper.GetMediaUrl(entity, size);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the given TypedEntity at the specific size
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public string GetMediaUrl(TypedEntity entity, string propertyAlias, int size)
        {
            return _urlHelper.GetMediaUrl(entity, propertyAlias, size);
        }

        #endregion

        #region ByHiveId

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the TypedEntity with the given id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public string GetMediaUrl(HiveId id)
        {
            return _urlHelper.GetMediaUrl(id);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the TypedEntity with the given id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns></returns>
        public string GetMediaUrl(HiveId id, string propertyAlias)
        {
            return _urlHelper.GetMediaUrl(id, propertyAlias);
        }

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the TypedEntity with the given id, at the specified size
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public string GetMediaUrl(HiveId id, int size)
        {
            return _urlHelper.GetMediaUrl(id, size);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the TypedEntity with the given id, at the specified size
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public string GetMediaUrl(HiveId id, string propertyAlias, int size)
        {
            return _urlHelper.GetMediaUrl(id, propertyAlias, size);
        }

        #endregion

        #region ByString

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the TypedEntity with the given id
        /// </summary>
        /// <param name="idOrPropertyAlias">The id or property alias.</param>
        /// <returns></returns>
        public string GetMediaUrl(string idOrPropertyAlias)
        {
            return _currentPage.Attributes.Any(x => x.AttributeDefinition.Alias == idOrPropertyAlias)
                ? _urlHelper.GetMediaUrl(_currentPage.Id, idOrPropertyAlias)
                : _urlHelper.GetMediaUrl(idOrPropertyAlias);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the TypedEntity with the given id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns></returns>
        public string GetMediaUrl(string id, string propertyAlias)
        {
            return _urlHelper.GetMediaUrl(id, propertyAlias);
        }

        /// <summary>
        /// Gets the URL of the file in the first upload field found on the TypedEntity with the given id, at the specified size
        /// </summary>
        /// <param name="idOrPropertyAlias">The id or property alias.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public string GetMediaUrl(string idOrPropertyAlias, int size)
        {
            return _currentPage.Attributes.Any(x => x.AttributeDefinition.Alias == idOrPropertyAlias)
                ? _urlHelper.GetMediaUrl(_currentPage.Id, idOrPropertyAlias, size)
                : _urlHelper.GetMediaUrl(idOrPropertyAlias, size);
        }

        /// <summary>
        /// Gets the URL of the file in the upload field with the given property alias on the TypedEntity with the given id, at the specified size
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="size">The size (must be a prevalue on the upload property editor).</param>
        /// <returns></returns>
        public string GetMediaUrl(string id, string propertyAlias, int size)
        {
            return _urlHelper.GetMediaUrl(id, propertyAlias, size);
        }

        #endregion

        #endregion

        #region GetEntityById

        /// <summary>
        /// Gets the typed entity by the given id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public TypedEntity GetEntityById(HiveId id)
        {
            var hive = _requestContext.Application.Hive.GetReader<IContentStore>(id.ToUri());
            using (var uow = hive.CreateReadonly())
            {
                return uow.Repositories.Get(id);
            }
        }

        /// <summary>
        /// Gets the typed entity by the given id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public TypedEntity GetEntityById(string id)
        {
            return GetEntityById(HiveId.Parse(id));
        }

        #endregion

        #region GetContentById

        public Content GetContentById(HiveId id)
        {
            return _requestContext.Application.Hive.Cms().Content.GetById(id);
        }

        public Content GetContentById(string id)
        {
            return GetContentById(HiveId.Parse(id));
        }

        #endregion

        #region GetDynamicContentById

        public dynamic GetDynamicContentById(HiveId id, object defaultValue = null)
        {
            var content = _requestContext.Application.Hive.Cms().Content.GetById(id);
            if (content != null) return content.Bend(_requestContext.Application.Hive);
            return defaultValue ?? new BendyObject();
        }

        public dynamic GetDynamicContentById(string id)
        {
            return GetDynamicContentById(HiveId.Parse(id));
        }

        #endregion

        #region RenderMacro

        /// <summary>
        /// Renders the macro with the specified alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias)
        {
            return RenderMacro(alias, new {});
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias, object parameters)
        {
            var macroRenderer = new MacroRenderer(_requestContext.RegisteredComponents, _requestContext);
            var result = macroRenderer.RenderMacroAsString(alias, parameters.ToDictionary<string>(), _controllerContext, false, () => _currentPage);
            return new HtmlString(result);
        }

        #endregion

        #region GetPreValueModel

        /// <summary>
        /// Gets the pre value model for the datatype with the specified id.
        /// </summary>
        /// <param name="dataTypeId">The data type id.</param>
        /// <returns></returns>
        public PreValueModel GetPreValueModel(HiveId dataTypeId)
        {
            using (var uow = _requestContext.Application.Hive.OpenReader<IContentStore>())
            {
                var dataTypeEntity = uow.Repositories.Schemas.Get<AttributeType>(dataTypeId);
                if (dataTypeEntity == null)
                    throw new ArgumentException(string.Format("No AttributeType found for id: {0} on action Edit", dataTypeId));

                var dataType = _requestContext.Application.FrameworkContext.TypeMappers.Map<AttributeType, DataType>(dataTypeEntity);
                return dataType.GetPreValueModel();
            }
        }

        /// <summary>
        /// Gets the pre value model for the datatype with the specified id.
        /// </summary>
        /// <param name="dataTypeId">The data type id.</param>
        /// <returns></returns>
        public PreValueModel GetPreValueModel(string dataTypeId)
        {
            return GetPreValueModel(HiveId.Parse(dataTypeId));
        }

        #endregion

        #region Field

        public IHtmlString Field(string fieldAlias, string valueAlias = "",
            string altFieldAlias = "", string altValueAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
            bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
            UmbracoRenderItemCaseType casing = UmbracoRenderItemCaseType.Unchanged,
            UmbracoRenderItemEncodingType encoding = UmbracoRenderItemEncodingType.Unchanged)
        {
            return Field(_currentPage, fieldAlias, valueAlias, altFieldAlias, altValueAlias,
                altText, insertBefore, insertAfter, recursive, convertLineBreaks, removeParagraphTags,
                casing, encoding);
        }

        public IHtmlString Field(Content currentPage, string fieldAlias, string valueAlias = "",
            string altFieldAlias = "", string altValueAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
            bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
            UmbracoRenderItemCaseType casing = UmbracoRenderItemCaseType.Unchanged,
            UmbracoRenderItemEncodingType encoding = UmbracoRenderItemEncodingType.Unchanged)
        {
            return new FieldRenderer()
                .RenderField(_requestContext, _controllerContext, currentPage, fieldAlias, valueAlias, altFieldAlias, altValueAlias,
                altText, insertBefore, insertAfter, recursive, convertLineBreaks, removeParagraphTags,
                casing, encoding);
        }

        #endregion

        #region Dictionary

        public string GetDictionaryItem(string key, string defaultValue = null)
        {
            var dictionaryHelper = new DictionaryHelper(_requestContext.Application);
            return dictionaryHelper.GetDictionaryItemValue(key, defaultValue);
        }

        public string GetDictionaryItemForLanguage(string key, string language, string defaultValue = null)
        {
            var dictionaryHelper = new DictionaryHelper(_requestContext.Application);
            return dictionaryHelper.GetDictionaryItemValueForLanguage(key, language, defaultValue);
        }

        #endregion

        #region Truncate (for dynamics)
        public string Truncate(string original, int maxLength, string suffix = "...")
        {
            if (original == null) return string.Empty;
            return original.Truncate(maxLength, suffix);
        }
        #endregion
    }
}