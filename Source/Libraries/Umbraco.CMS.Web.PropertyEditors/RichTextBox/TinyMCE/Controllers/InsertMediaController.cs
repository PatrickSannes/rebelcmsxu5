using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using System.Web.UI;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using System;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework;
using System.Linq;
using System.Web.Compilation;
using System.Reflection;
using System.Collections.Generic;
using Umbraco.Cms.Web.Mvc;
using System.Collections;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Framework.Persistence.Model.Constants;

[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Resources.InsertMedia.InsertMedia.js", "application/x-javascript")]

namespace Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("15833890-81DC-4B5E-8607-768D2E3E6087")]
    [UmbracoEditor]
    public class InsertMediaController : AbstractEditorController
    {
        public InsertMediaController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        #region Actions

        /// <summary>
        /// Inserts the media.
        /// </summary>
        /// <returns></returns>
        public ActionResult InsertMedia()
        {
           return View(EmbeddedViewPath.Create("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMedia.InsertMedia.cshtml, Umbraco.Cms.Web.PropertyEditors"));
        }


        /// <summary>
        /// Gets the media parameters.
        /// </summary>
        /// <param name="EntitySchemaToDocumentTypeEditorModel">The media id.</param>
        /// <returns></returns>
        public JsonResult GetMediaParameters(HiveId mediaId)
        {

            string path = BackOfficeRequestContext.RoutingEngine.GetUrl(mediaId);

            Mandate.That<NullReferenceException>(!string.IsNullOrEmpty(path));

            IEnumerable<object> viewParams = Enumerable.Empty<object>();

            var ext = path.Split('.').LastOrDefault().ToLower();
            object view;


            //first look for custom view
            using (var uow = BackOfficeRequestContext
                .Application
                .Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
            {
                var id = new HiveId(new Uri("storage://templates"), "", new HiveIdValue(string.Format("/MediaTemplates/{0}.cshtml", ext)));
                var customView = uow.Repositories.Get<File>(id);

                if (customView != null)
                {

                    view = BuildManager.CreateInstanceFromVirtualPath(customView.RootRelativePath, typeof(object));
                }

            }

           
            try
            {
                view = BuildManager.CreateInstanceFromVirtualPath(
                    EmbeddedViewPath.GetFullPath(EmbeddedViewPath.Create(
                    string.Format("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMedia.Types.{0}.cshtml, Umbraco.Cms.Web.PropertyEditors", ext)))
                    , typeof(object));

            }
            catch (TypeLoadException e)
            {

                view = BuildManager.CreateInstanceFromVirtualPath(
                    EmbeddedViewPath.GetFullPath(EmbeddedViewPath.Create(
                   "Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMedia.Types.default.cshtml, Umbraco.Cms.Web.PropertyEditors"))
                   , typeof(object));
                
                
            }

            if (view != null)
            {
                viewParams = view.GetType()
                                   .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                   .Select(x => new
                                   {
                                       name = x.Name
                                   });
            }
            return new CustomJsonResult(new
            {
                viewParams

            }.ToJsonString);
            
        }

        /// <summary>
        /// Allowses the upload.
        /// </summary>
        /// <param name="mediaId">The media id.</param>
        /// <returns></returns>
        public JsonResult AllowsUpload(HiveId mediaId)
        {
            bool allowsUpload = false;

            if (mediaId == FixedHiveIds.MediaVirtualRoot)
                allowsUpload = true;
            else
            {
                var hive = BackOfficeRequestContext.Application.Hive.GetReader<IMediaStore>();

                using (var uow = hive.CreateReadonly())
                {
                    var entity = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(mediaId);
                    var schema = entity.Revision.Item.EntitySchema;
                    var docType = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(schema);
                    allowsUpload = docType.AllowedChildIds.Any();

                }
            }
            return new CustomJsonResult(new
            {
                allowsUpload = allowsUpload

            }.ToJsonString);
        }

        /// <summary>
        /// Gets the possible media types.
        /// </summary>
        /// <param name="mediaId">The media id.</param>
        /// <returns></returns>
        public JsonResult GetPossibleMediaTypes(HiveId mediaId)
        {
            ArrayList types = new ArrayList();

            if (mediaId == FixedHiveIds.MediaVirtualRoot)
            {
                using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IContentStore>())
                {

                    //get all the media types
                    var schemas = uow.Repositories.Schemas.GetEntityByRelationType<EntitySchema>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.MediaRootSchema)
                                  .Where(x => !x.Id.IsSystem());

                    foreach (var ent in schemas)
                    {
                        var dt = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(ent);
                        if (dt.Properties.Where(p => p.DataType.PropertyEditor.Id.ToString().ToUpper() == CorePluginConstants.FileUploadPropertyEditorId).Count() > 0)
                            types.Add(new { Value = ent.Id.ToString(), Display = ent.Name.ToString() });
                    }
                   
                }
            }
            else
            {
                var hive = BackOfficeRequestContext.Application.Hive.GetReader<IMediaStore>();

                using (var uow = hive.CreateReadonly())
                {
                    var entity = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(mediaId);

                    var docType =
                        BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(entity.Revision.Item.EntitySchema);

                    foreach (var id in docType.AllowedChildIds)
                    {

                        var ent = uow.Repositories.Schemas.Get<EntitySchema>(id);
                        var dt = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(ent);
                        if (dt.Properties.Where(p => p.DataType.PropertyEditor.Id.ToString().ToUpper() == CorePluginConstants.FileUploadPropertyEditorId).Count() > 0)
                            types.Add(new { Value = ent.Id.ToString(), Display = ent.Name.ToString() });
                    }

                }

            }
            return new CustomJsonResult(new
            {
                possibleMediaTypes = types

            }.ToJsonString);
        }
        /// <summary>
        /// Gets the media preview.
        /// </summary>
        /// <param name="mediaId">The media id.</param>
        /// <returns></returns>
        public ActionResult GetMediaPreview(HiveId mediaId)
        {

            string path = BackOfficeRequestContext.RoutingEngine.GetUrl(mediaId);

            Mandate.That<NullReferenceException>(!string.IsNullOrEmpty(path));

            var ext = path.Split('.').LastOrDefault().ToLower();

            var mediaParamsDict = new Dictionary<string, string>();

            Umbraco.Framework.Dynamics.BendyObject bendyParams = new Framework.Dynamics.BendyObject(mediaParamsDict);

            var model = new Model.BackOffice.TinyMCE.InsertMedia.InsertMediaModel(mediaId, path, bendyParams);
            model.FilePropertyAlias = GetUploadPropertyAlias(mediaId);

            //first look for custom view
            using (var uow = BackOfficeRequestContext
                .Application
                .Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
            {
                var id = new HiveId(new Uri("storage://templates"), "", new HiveIdValue(string.Format("/MediaTemplates/{0}.preview.cshtml", ext)));
                var customView = uow.Repositories.Get<File>(id);

                if (customView != null)
                    return View(ext, model);

            }


            try
            {
                //try returning embedded view for extension
                return View(EmbeddedViewPath.Create(
                    string.Format("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMedia.Types.{0}.preview.cshtml, Umbraco.Cms.Web.PropertyEditors", ext)), model);
            }
            catch (TypeLoadException e)
            {

                //view for extension not found, return default image view
                return View(EmbeddedViewPath.Create(
                   "Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMedia.Types.default.preview.cshtml, Umbraco.Cms.Web.PropertyEditors"), model);
            }
        }

        /// <summary>
        /// Gets the media markup.
        /// </summary>
        /// <param name="mediaId">The media id.</param>
        /// <param name="mediaParameters">The media parameters.</param>
        /// <returns></returns>
        public ActionResult GetMediaMarkup(HiveId mediaId, string mediaParameters = "")
        {
            string path = BackOfficeRequestContext.RoutingEngine.GetUrl(mediaId);
           
            Mandate.That<NullReferenceException>(!string.IsNullOrEmpty(path));

            var ext = path.Split('.').LastOrDefault().ToLower();
           
            var mediaParamsDict = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(mediaParameters))
                mediaParamsDict = (Dictionary<string, string>)mediaParameters.DecodeMacroParameters();

            var bendyParams = new Framework.Dynamics.BendyObject(mediaParamsDict);


            var model = new Model.BackOffice.TinyMCE.InsertMedia.InsertMediaModel(mediaId, path, bendyParams);

                      
            //first look for custom view
            using (var uow = BackOfficeRequestContext
                .Application
                .Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
            {
                var id = new HiveId(new Uri("storage://templates"), "", new HiveIdValue(string.Format("/MediaTemplates/{0}.cshtml", ext)));
                var customView = uow.Repositories.Get<File>(id);

                if (customView != null)
                    return View(ext, model);             
                   
            }


            try
            {
                //try returning embedded view for extension
                return View(EmbeddedViewPath.Create(
                    string.Format("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMedia.Types.{0}.cshtml, Umbraco.Cms.Web.PropertyEditors", ext)), model);
            }
            catch (TypeLoadException e)
            {
                //view for extension not found, return default image view
                var viewType = EmbeddedViewPath.Create("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMedia.Types.default.cshtml, Umbraco.Cms.Web.PropertyEditors");
                return View(viewType, model);
            }
        }

        public JsonResult IsChosenMediaItemValid(HiveId mediaId)
        {
            bool isValid = BackOfficeRequestContext.RoutingEngine.GetUrl(mediaId) != null;

            return new CustomJsonResult(new
            {
                valid = isValid

            }.ToJsonString);
        }
        #endregion

        private string GetUploadPropertyAlias(HiveId mediaId)
        {
            string uploadAlias = string.Empty;

            var hive = BackOfficeRequestContext.Application.Hive.GetReader<IMediaStore>();

            using (var uow = hive.CreateReadonly())
            {
                var entity = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(mediaId);

                var docType =
                    BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(entity.Revision.Item.EntitySchema);

                uploadAlias = docType.Properties.Where(p => p.DataType.PropertyEditor.Id.ToString().ToUpper() == CorePluginConstants.FileUploadPropertyEditorId).First().Alias;
            }

            return uploadAlias;
        }
    }
}
