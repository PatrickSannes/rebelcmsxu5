using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Framework;

using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using System.Linq;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using File = Umbraco.Framework.Persistence.Model.IO.File;
using MSIO = System.IO;

namespace Umbraco.Cms.Web.Editors
{
    public abstract class AbstractFileEditorController : StandardEditorController
    {
        protected AbstractFileEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        public abstract GroupUnitFactory<IFileStore> Hive { get; }

        protected abstract string SaveSuccessfulTitle { get; }
        protected abstract string SaveSuccessfulMessage { get; }

        protected abstract string CreateNewTitle { get; }

        /// <summary>
        /// The allowed file extensions supported for the editor
        /// </summary>
        protected abstract string[] AllowedFileExtensions { get; }

        /// <summary>
        /// Displays the editor
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = Hive.Create())
            {
                var file = uow.Repositories.Get<File>(id.Value);
                if (file != null)
                {
                    var model = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<FileEditorModel>(file);
                    EnsureViewData(model, file);
                    return View(model);
                }
            }

            return HttpNotFound();
        }

        [HttpGet]
        public virtual ActionResult CreateNew(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            var currentFolderPath = "/";
            if (id != FixedHiveIds.SystemRoot)
            {
                using (var uow = Hive.Create())
                {
                    var parentFile = uow.Repositories.Get<File>(id.Value);
                    if (parentFile == null)
                        throw new ArgumentException("No folder could be found for the id specified");
                    currentFolderPath = "/" + parentFile.GetFilePathForDisplay();
                }
            }
            var model = new CreateFileModel { ParentId = id.Value, CurrentFolderPath = currentFolderPath };
            EnsureViewData(model);
            return View(model);
        }

        [HttpPost]
        [ActionName("CreateNew")]
        [Save]
        [SupportsPathGeneration(IsRequired = false)]
        public virtual ActionResult CreateNewForm(CreateFileModel createModel)
        {
            Mandate.ParameterNotNull(createModel, "createModel");
            Mandate.That<NullReferenceException>(createModel.Name != null);
            Mandate.That<NullReferenceException>(!createModel.ParentId.IsNullValueOrEmpty());

            EnsureViewData(createModel);

            //validate the model
            if (!TryUpdateModel(createModel))
            {
                return View(createModel);
            }

            using (var uow = Hive.Create())
            {
                if (createModel.ParentId != FixedHiveIds.SystemRoot)
                {
                    //validate the parent
                    var parentFile = uow.Repositories.Get<File>(createModel.ParentId);
                    if (parentFile == null)
                        throw new ArgumentException("No folder could be found for the parent id specified");
                }


                //if its a folder, then we just create it and return success.                
                if (createModel.CreateType == CreateFileType.Folder)
                {
                    var folder = new File()
                        {
                            IsContainer = true,
                            RootedPath = createModel.ParentId == FixedHiveIds.SystemRoot
                                ? createModel.Name.ToUmbracoAlias(removeSpaces: true)
                                : (string)createModel.ParentId.Value + "/" + createModel.Name.ToUmbracoAlias(removeSpaces: true)
                        };
                    uow.Repositories.AddOrUpdate(folder);
                    uow.Complete();

                    //add notification
                    Notifications.Add(new NotificationMessage("Folder.Save.Message".Localize(this), "Folder.Save.Title".Localize(this), NotificationType.Success));

                    //add path for entity for SupportsPathGeneration (tree syncing) to work
                    GeneratePathsForCurrentEntity(CreatePaths(folder));

                    return RedirectToAction("CreateNew", new { id = folder.Id });

                }

                var model = FileEditorModel.CreateNew();
                model.Name = createModel.Name.ToUmbracoAlias(removeSpaces:true) + createModel.FileExtension;
                model.ParentId = createModel.ParentId;

                if (!createModel.Stub.IsNullValueOrEmpty())
                    PopulateFileContentFromStub(model, createModel.Stub.Value);

                EnsureViewData(model, null);

                OnBeforeCreate(createModel, model);

                var file = PerformSave(model);

                OnAfterCreate(file);

                //add notification
                Notifications.Add(new NotificationMessage(SaveSuccessfulMessage, SaveSuccessfulTitle, NotificationType.Success));

                //add path for entity for SupportsPathGeneration (tree syncing) to work
                GeneratePathsForCurrentEntity(CreatePaths(file));

                return RedirectToAction("Edit", new { id = file.Id });

            }

        }

        /// <summary>
        /// Handles the AJAX post to save the file
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual JsonResult Save(FileEditorModel model)
        {
            Mandate.ParameterNotNull(model, "model");

            if (!TryValidateModel(model))
            {
                return ModelState.ToJsonErrors();
            }

            OnBeforeUpdate(model);

            var file = PerformSave(model);

            using (var uow = Hive.Create())
            {
                Notifications.Add(new NotificationMessage(SaveSuccessfulMessage, SaveSuccessfulTitle, NotificationType.Success));
                var path = CreatePaths(file).ToJson();
                return new CustomJsonResult(new
                {
                    id = file.Id.ToString(),
                    //need to send the path back so we can sync the tree
                    path,
                    notifications = Notifications,
                    success = true
                }.ToJsonString);
            }
        }

        /// <summary>
        /// JSON action to delete a node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public virtual JsonResult Delete(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = Hive.Create())
            {
                uow.Repositories.Delete<File>(id);
                uow.Complete();
            }

            //return a successful JSON response
            return Json(new { message = "Success" });
        }

        /// <summary>
        /// Used by inheritors to make any changes to the file before it is persisted
        /// </summary>
        /// <param name="file"></param>
        protected virtual void OnBeforeSave(File file)
        {            
        }

        /// <summary>
        /// Used by inheritors to make any changes to the model before creation
        /// </summary>
        /// <param name="model"></param>
        protected virtual void OnBeforeUpdate(FileEditorModel editorModel)
        {

        }

        /// <summary>
        /// Used by inheritors to make any changes to the model before creation
        /// </summary>
        /// <param name="model"></param>
        protected virtual void OnBeforeCreate(CreateFileModel createModel, FileEditorModel editorModel)
        {
            
        }

        /// <summary>
        /// Used by inheritors to make any changes to the model before creation
        /// </summary>
        /// <param name="model"></param>
        protected virtual void OnAfterCreate(File file)
        {

        }

        /// <summary>
        /// Used by inheritors to make any changes to the model before redirecting
        /// </summary>
        /// <param name="file"></param>
        protected virtual EntityPathCollection CreatePaths(File file)
        {
            using (var uow = Hive.Create())
            {
                return uow.Repositories.GetEntityPaths<File>(file.Id, FixedRelationTypes.DefaultRelationType);
            }
        }

        /// <summary>
        /// Does the save.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        internal File PerformSave(FileEditorModel model)
        {
            using (var uow = Hive.Create())
            {
                var repo = uow.Repositories;

                File file;
                if (model.Id.IsNullValueOrEmpty())
                {
                    //its new

                    file = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<File>(model);

                    //we are creating a new file, need to validate the parent id and make sure we are saving it under the parent)
                    if (model.ParentId != FixedHiveIds.SystemRoot)
                    {
                        if (model.ParentId.IsNullValueOrEmpty())
                            throw new ArgumentNullException("Creating a new file requires the ParentId property to be set");
                        var parent = uow.Repositories.Get<File>(model.ParentId);
                        if (parent == null)
                            throw new ArgumentException("No folder could be found for the parent id specified");

                        //ensure that we save the file under the current parent
                        file.RootedPath = Path.Combine(parent.RootedPath, model.Name);
                    }
                }
                else
                {
                    //it exists

                    file = repo.Get<File>(model.Id);
                    if (file == null)
                        throw new ArgumentException("No file could be found for the id specified");

                    BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(model, file);
                }

                OnBeforeSave(file);

                repo.AddOrUpdate(file);
                uow.Complete();

                return file;
            }
        }

        /// <summary>
        /// This adds some required elements to the ViewBag so that the Create view renders correctly
        /// </summary>
        /// <param name="model"></param>
        protected virtual void EnsureViewData(CreateFileModel model)
        {
            var controllerName = UmbracoController.GetControllerName(this.GetType());

            // Update thumbnails
            var thumbnailFolder = Url.Content(BackOfficeRequestContext.Application.Settings.UmbracoFolders.DocTypeThumbnailFolder);
            model.AvailableFileExtensions = AllowedFileExtensions;
            model.FileThumbnail = thumbnailFolder + "/doc.png";
            model.FolderThumbnail = thumbnailFolder + "/folder.png";

            // Populate avilable stubs
            var stubDirHiveId = new HiveId(new Uri("storage://stubs"), "stubs", new HiveIdValue(controllerName));
            var stubHive = BackOfficeRequestContext.Application.Hive.GetReader<IFileStore>(stubDirHiveId.ToUri());
            using (var uow = stubHive.CreateReadonly())
            {
                var stubFileRelations = uow.Repositories.GetChildRelations(stubDirHiveId, FixedRelationTypes.DefaultRelationType);
                if(stubFileRelations.Any())
                {
                    var stubFiles = uow.Repositories.Get<File>(true, stubFileRelations.Select(x => x.DestinationId).ToArray());
                    if(stubFiles.Any())
                    {
                        model.AvailableStubs = stubFiles.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() });
                    }
                }
            }

            // Populate viewbag
            ViewBag.Title = CreateNewTitle;
            ViewBag.ControllerId = UmbracoController.GetControllerId<EditorAttribute>(GetType());
        }

        protected virtual void EnsureViewData(FileEditorModel model, File file)
        {
            // To be overridden
        }

        protected virtual void PopulateFileContentFromStub(FileEditorModel model, HiveId stubFileId)
        {
            PopulateFileContentFromStub(model, stubFileId, new Dictionary<string, string>());
        }

        protected virtual void PopulateFileContentFromStub(FileEditorModel model, HiveId stubFileId, IDictionary<string, string> replacements)
        {
            var stubHive = BackOfficeRequestContext.Application.Hive.GetReader<IFileStore>(stubFileId.ToUri());
            using (var uow = stubHive.CreateReadonly())
            {
                var stubFile = uow.Repositories.Get<File>(stubFileId);
                if(stubFile != null)
                {
                    var fileContent = Encoding.UTF8.GetString(stubFile.ContentBytes);

                    foreach (var replacement in replacements)
                        fileContent = fileContent.Replace(replacement.Key, replacement.Value);

                    model.FileContent = fileContent;
                }
            }
        }
    }
}
