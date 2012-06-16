using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors.Extenders;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ActionInvokers;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    using Umbraco.Hive.ProviderGrouping;

    /// <summary>
    /// An Abstract controller containing primary logic for content controllers
    /// </summary>
    [SupportClientNotifications]
    [SupportModelEditing]
    [ExtendedBy(typeof(PublishController))]
    [ExtendedBy(typeof(SortController))]
    [ExtendedBy(typeof(PermissionsController))]
    [ExtendedBy(typeof(RollbackController))]
    public abstract class AbstractRevisionalContentEditorController<TEditorModel> : AbstractContentEditorController
        where TEditorModel : StandardContentEditorModel
    {
        protected AbstractRevisionalContentEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {

        }

        /// <summary>
        /// Returns the recycle bin id used for this controller
        /// </summary>
        public abstract HiveId RecycleBinId { get; }

        /// <summary>
        /// Return the media root as the virtual root node
        /// </summary>
        public abstract HiveId VirtualRootNodeId { get; }

        /// <summary>
        /// Returns the media virtual root
        /// </summary>
        public abstract HiveId RootSchemaNodeId { get; }

        /// <summary>
        /// The title to display in the create new dialog
        /// </summary>
        public abstract string CreateNewTitle { get; }

        public abstract ReadonlyGroupUnitFactory ReadonlyHive { get; }

        /// <summary>
        /// JSON action to send a node to the recycle bin or completely delete it if it is in the recycle bin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Delete })]
        public override JsonResult Delete(HiveId? id)
        {
            //TODO: When deleting an item, do we remove any domains assigned to it ?

            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = Hive.Create<IContentStore>())
            {
                //get the entity and we need to change the content tree relation type to the deleted relation type
                var entity = uow.Repositories.Get(id.Value);
                var ancestors = uow.Repositories.GetAncestorRelations(entity, FixedRelationTypes.DefaultRelationType);
                //check if this node is in the recycle bin, if so just delete it
                if (RecycleBinId.IsNullValueOrEmpty() || ancestors.Select(x => x.SourceId).Contains(RecycleBinId, new HiveIdComparer(true)))
                {
                    uow.Repositories.Delete<TypedEntity>(entity.Id);
                }
                else
                {
                    //var contentParents = entity.RelationProxies.GetParentRelations(FixedRelationTypes.ContentTreeRelationType);
                    var contentParents = ancestors.Where(x => x.DestinationId == entity.Id);
                    foreach (var c in contentParents)
                    {
                        //change original relation type to RecycledTreeRelation
                        uow.Repositories.ChangeRelationType(c, FixedRelationTypes.RecycledRelationType);
                        //add new content relation type to exist under the recycle bin
                        uow.Repositories.AddRelation(RecycleBinId, entity.Id, FixedRelationTypes.DefaultRelationType, 0);
                    }
                    uow.Repositories.AddOrUpdate(entity);
                }
                uow.Complete();
            }

            //clears the domain cache
            BackOfficeRequestContext.RoutingEngine.ClearCache(clearDomains: true, clearGeneratedUrls: true);

            //return a successful JSON response
            return Json(new { message = "Success" });
        }

        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Update })]
        public override ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = ReadonlyHive.CreateReadonly<IContentStore>())
            {
                var entity = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(id.Value) ?? new EntitySnapshot<TypedEntity>(new Revision<TypedEntity>(uow.Repositories.Get<TypedEntity>(id.Value)));

                if (entity.Revision.Item == null)
                    throw new ArgumentException(string.Format("No revision found or could not find unrevisioned entity for id: {0} on action Edit", id));

                entity.Revision.Item.EntitySchema = uow.Repositories.Schemas.GetComposite<EntitySchema>(entity.Revision.Item.EntitySchema.Id);

                var model = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySnapshot<TypedEntity>, TEditorModel>(entity);

                OnEditing(model, entity);

                return View(model);
            }
        }

        /// <summary>
        /// Handles the editor post back
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        [ActionName("Edit")]
        [HttpPost]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [PersistTabIndexOnRedirect]
        [Save(SaveAttribute.DefaultSaveButtonId, "submit.Publish", "submit.Unpublish")]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Update })]
        public virtual ActionResult EditForm(HiveId? id, HiveId? revisionId)
        {
            Mandate.ParameterNotEmpty(id, "id");
            // We don't mandate the revisionId in case a Hive provider doesn't support revisioning

            using (var uow = ReadonlyHive.CreateReadonly<IContentStore>())
            {
                Revision<TypedEntity> revision = null;
                bool isRevisional = (revisionId.HasValue && !revisionId.Value.IsNullValueOrEmpty());

                if (!isRevisional) revision = new Revision<TypedEntity>(uow.Repositories.Get(id.Value));
                else revision = uow.Repositories.Revisions.Get<TypedEntity>(id.Value, revisionId.Value);

                if (revision == null) throw new ArgumentException(string.Format("No revision found for id: {0} on action EditForm", id));

                revision.Item.EntitySchema =
                    uow.Repositories.Schemas.GetComposite<EntitySchema>(revision.Item.EntitySchema.Id);

                var contentViewModel =
                    BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map
                        <Revision<TypedEntity>, TEditorModel>(revision);
                //need to ensure that all of the Ids are mapped correctly, when editing existing content the only reason for this
                //is to ensure any new document type properties that have been created are reflected in the new content revision
                ReconstructModelPropertyIds(contentViewModel);

                return ProcessSubmit(contentViewModel, revision, isRevisional);
            }
        }

        /// <summary>
        /// Allows inheritors to modify the model before being passed to the view
        /// </summary>
        /// <param name="model">The model being returned to the view</param>
        /// <param name="entity">The entity that the model was created from</param>
        protected virtual void OnEditing(TEditorModel model, EntitySnapshot<TypedEntity> entity)
        {
        }

        #region CreateNew

        /// <summary>
        /// Shows the create editor wizard
        /// </summary>
        /// <param name="id">The parent id to create content under</param>
        /// <returns></returns>
        [HttpGet]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Create })]
        public virtual ActionResult CreateNew(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            //store the parent document type to the view
            var model = new CreateContentModel { ParentId = id.Value };
            return CreateNewView(model);
        }

        /// <summary>
        /// Handles the create wizard step post
        /// </summary>
        /// <param name="createModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CreateNew")]
        [Save]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Create }, IdParameterName = "ParentId")]
        [SupportsPathGeneration]
        public virtual ActionResult CreateNewForm(CreateContentModel createModel)
        {
            Mandate.ParameterNotNull(createModel, "createModel");
            Mandate.That<NullReferenceException>(!createModel.ParentId.IsNullValueOrEmpty());
            Mandate.That<NullReferenceException>(!createModel.SelectedDocumentTypeId.IsNullValueOrEmpty());

            //validate the model
            TryUpdateModel(createModel);
            //get the create new result view which will validate that the selected doc type id is in fact allowed
            var result = CreateNewView(createModel);
            //if at this point the model state is invalid, return the result which is the CreateNew view
            if (!ModelState.IsValid)
            {
                return result;
            }

            using (var uow = Hive.Create<IContentStore>())
            {
                var schema = uow.Repositories.Schemas.Get<EntitySchema>(createModel.SelectedDocumentTypeId);
                if (schema == null)
                    throw new ArgumentException(string.Format("No schema found for id: {0} on action Create", createModel.SelectedDocumentTypeId));

                //create the empty content item
                var contentViewModel = CreateNewContentEntity(schema, createModel.Name, createModel.ParentId);
                //map the Ids correctly to the model so it binds
                ReconstructModelPropertyIds(contentViewModel);

                return ProcessCreate(contentViewModel, true);
            }

            //everything is valid, now we need to render out the editor for this document type without any data
            //return RedirectToAction("Create", new
            //{
            //    docTypeId = createModel.SelectedDocumentTypeId,
            //    name = createModel.Name,
            //    parentId = createModel.ParentId
            //});
        }
        #endregion

        /// <summary>
        /// Json action to empty the recycle bin
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.EmptyRecycleBin })]
        public virtual JsonResult EmptyBin()
        {
            using (var uow = Hive.Create<IContentStore>())
            {
                var items = uow.Repositories.GetEntityByRelationType<TypedEntity>(FixedRelationTypes.DefaultRelationType, RecycleBinId);
                foreach (var i in items)
                {
                    uow.Repositories.Delete<TypedEntity>(i.Id);
                }
                uow.Complete();
            }

            return Json(new { success = true, message = "Success" });
        }

        protected virtual ActionResult ProcessCreate(TEditorModel model, bool isRevisional)
        {
            Mandate.ParameterNotNull(model, "model");

            //persist the data
            using (var uow = Hive.Create<IContentStore>())
            {
                //EnsureUniqueName(model);

                var entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TEditorModel, Revision<TypedEntity>>(model);

                var success = this.TryExecuteSecuredMethod(x => x.ProcessSave(model, entity), model.Id).Success;
                if (!success)
                {
                    // Report unathorized
                    NotifyForProcess(NotificationState.SaveUnauthorized, model);
                }

                if (success)
                {
                    if (isRevisional) uow.Repositories.Revisions.AddOrUpdate(entity);
                    else uow.Repositories.AddOrUpdate(entity.Item);
                    uow.Complete();

                    //need to clear the URL cache for this entry
                    BackOfficeRequestContext.RoutingEngine.ClearCache(clearGeneratedUrls: true, clearMappedUrls: true);

                    //add path for entity for SupportsPathGeneration (tree syncing) to work
                    GeneratePathsForCurrentEntity(uow.Repositories.GetEntityPaths<TypedEntity>(entity.Item.Id, FixedRelationTypes.DefaultRelationType));

                    return RedirectToAction("Edit", new { id = entity.Item.Id });
                }

                return View("Edit", model);
            }
        }

        /// <summary>
        /// When editing or creating content, this will bind the model, check the model state errors, add appropriate notifications
        /// return the error view or redirect to the correct place and also persist the data to the repository.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual ActionResult ProcessSubmit(TEditorModel model, Revision<TypedEntity> entity, bool isRevisional)
        {
            Mandate.ParameterNotNull(model, "model");

            //bind it's data
            model.BindModel(this);

            //if there's model errors, return the view
            if (!ModelState.IsValid)
            {
                AddValidationErrorsNotification();
                return View("Edit", model);
            }

            //persist the data
            var success = false;
            using (var uow = Hive.Create<IContentStore>())
            {
                //EnsureUniqueName(model);

                if (entity == null)
                {
                    //map to new entity
                    entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TEditorModel, Revision<TypedEntity>>(model);
                }
                else
                {
                    //map to existing entity
                    BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(model, entity.Item);

                    //then create a new revision
                    entity = entity.CopyToNewRevision();
                }


                // Try publish
                if (ValueProvider.GetValue("submit.Publish") != null)
                {
                    success = this.TryExecuteSecuredMethod(x => x.ProcessPublish(model, entity), model.Id).Success;
                    if (!success)
                    {
                        // Report unathorized
                        NotifyForProcess(NotificationState.PublishUnathorized, model);
                    }
                }
                // Try unpublish
                else if (ValueProvider.GetValue("submit.Unpublish") != null)
                {
                    success = this.TryExecuteSecuredMethod(x => x.ProcessUnpublish(model, entity), model.Id).Success;
                    if (!success)
                    {
                        // Report unathorized
                        NotifyForProcess(NotificationState.UnpublishedUnauthorized, model);
                    }
                }
                // Try save
                else
                {
                    success = this.TryExecuteSecuredMethod(x => x.ProcessSave(model, entity), model.Id).Success;
                    if (!success)
                    {
                        // Report unathorized
                        NotifyForProcess(NotificationState.SaveUnauthorized, model);
                    }
                }

                if (success)
                {
                    if (isRevisional) uow.Repositories.Revisions.AddOrUpdate(entity);
                    else uow.Repositories.AddOrUpdate(entity.Item);
                    uow.Complete();
                }
                else
                {
                    uow.Abandon();
                }
            }

            if (success)
            {
                // Perf: use a readonly unit here rather than delaying the writer
                using (var uow = ReadonlyHive.CreateReadonly<IContentStore>())
                {
                    //need to clear the URL cache for this entry
                    BackOfficeRequestContext.RoutingEngine.ClearCache(clearGeneratedUrls: true, clearMappedUrls: true);

                    //add path for entity for SupportsPathGeneration (tree syncing) to work
                    GeneratePathsForCurrentEntity(uow.Repositories.GetEntityPaths<TypedEntity>(entity.Item.Id, FixedRelationTypes.DefaultRelationType));

                    return RedirectToAction("Edit", new { id = entity.Item.Id });
                }
            }
            return View("Edit", model);
        }

        public enum NotificationState
        {
            Save, Publish, Unpublish, SaveUnauthorized, PublishUnathorized, UnpublishedUnauthorized
        }

        /// <summary>
        /// Outputs the correct messages in Notifications, validation and notice board based on the NotificationState
        /// </summary>
        /// <param name="state"></param>
        /// <param name="model"></param>
        protected virtual void NotifyForProcess(NotificationState state, TEditorModel model)
        {
            switch (state)
            {
                case NotificationState.Save:
                    Notifications.Add(new NotificationMessage(
                                          "Content.Save.Message".Localize(this),
                                          "Content.Save.Title".Localize(this),
                                          NotificationType.Success));
                    break;
                case NotificationState.Publish:
                    Notifications.Add(new NotificationMessage(
                                          "Publish.SingleSuccess.Message".Localize(this, new { model.Name }, encode: false),
                                          "Publish.Title".Localize(this),
                                          NotificationType.Success));
                    break;
                case NotificationState.Unpublish:
                    Notifications.Add(new NotificationMessage(
                                          "Unpublish.SingleSuccess.Message".Localize(this, new { model.Name }, encode: false),
                                          "Unpublish.Title".Localize(this),
                                          NotificationType.Success));
                    break;
                case NotificationState.SaveUnauthorized:
                    var msgSave = new NotificationMessage(
                        "Content.SaveUnathorized.Message".Localize(this, new { model.Name }, encode: false),
                        "Content.SaveUnathorized.Title".Localize(this),
                        NotificationType.Error);
                    //model.NoticeBoard.Add(msg);
                    ModelState.AddDataValidationError(msgSave.Message);
                    Notifications.Add(msgSave);
                    break;
                case NotificationState.PublishUnathorized:
                    var msgPublish = new NotificationMessage(
                        "Publish.SingleUnathorized.Message".Localize(this, new { model.Name }, encode: false),
                        "Publish.Title".Localize(this),
                        NotificationType.Error);
                    ModelState.AddDataValidationError(msgPublish.Message);
                    //model.NoticeBoard.Add(msg);
                    Notifications.Add(msgPublish);
                    break;
                case NotificationState.UnpublishedUnauthorized:
                    var msgUnpublish = new NotificationMessage(
                        "Unpublish.SingleUnathorized.Message".Localize(this, new { model.Name }, encode: false),
                        "Unpublish.Title".Localize(this),
                        NotificationType.Error);
                    //model.NoticeBoard.Add(msgUnpublish);
                    ModelState.AddDataValidationError(msgUnpublish.Message);
                    Notifications.Add(msgUnpublish);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("state");
            }
        }

        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Save })]
        protected void ProcessSave(TEditorModel model, Revision<TypedEntity> entity)
        {
            NotifyForProcess(NotificationState.Save, model);

            entity.MetaData.StatusType = FixedStatusTypes.Draft;
        }

        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Publish })]
        protected void ProcessPublish(TEditorModel model, Revision<TypedEntity> entity)
        {
            model.UtcPublishedDate = DateTimeOffset.UtcNow;

            NotifyForProcess(NotificationState.Publish, model);

            entity.MetaData.StatusType = FixedStatusTypes.Published;
        }

        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Unpublish })]
        protected void ProcessUnpublish(TEditorModel model, Revision<TypedEntity> entity)
        {
            model.UtcPublishedDate = null;

            NotifyForProcess(NotificationState.Unpublish, model);

            entity.MetaData.StatusType = FixedStatusTypes.Unpublished;
        }

        /// <summary>
        /// Ensures the name is unique.
        /// </summary>
        /// <param name="model">The model.</param>
        protected virtual void EnsureUniqueName(TEditorModel model)
        {
            //using (var uow = Hive.Create<IContentStore>())
            //{
            //    var parentId = (model.ParentId.IsNullValueOrEmpty())
            //        ? uow.Repositories.GetParentRelations(model.Id, FixedRelationTypes.ContentTreeRelationType).FirstOrDefault().SourceId
            //        : model.ParentId;

            //    var children = uow.Repositories.GetChildRelations(parentId, FixedRelationTypes.ContentTreeRelationType);
            //    foreach (var childEntity in children.Where(x => x.DestinationId != model.Id).Select(child => (TypedEntity)child.Destination))
            //    {
            //        var nameProperty = childEntity.Attributes[NodeNameAttributeDefinition.AliasValue];
            //        if (nameProperty != null)
            //        {
            //            var name = nameProperty.Values["Name"].ToString();
            //            if (name == model.Name)
            //            {
            //                var indexRegex = new Regex(@"\s\(([0-9]+)\)$", RegexOptions.Singleline | RegexOptions.Compiled);
            //                var match = indexRegex.Match(model.Name);
            //                if(indexRegex.IsMatch(model.Name))
            //                {
            //                    var num = Int32.Parse(match.Groups[1].Value);
            //                    model.Name = indexRegex.Replace(model.Name, " (" + (num + 1) + ")");
            //                }
            //                else
            //                {
            //                    model.Name += " (1)";
            //                }
            //                EnsureUniqueName(model);
            //                break;
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// This adds some required elements to the ViewBag so that the Create view renders correctly
        /// </summary>
        protected virtual void EnsureCreateWizardViewBagData(IEnumerable<EntitySchema> docTypesData)
        {
            var docTypesInfo = docTypesData.Where(x => !x.IsAbstract)
                                           .Select(BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeInfo>);

            //add the available doc types to the view bag            
            ViewBag.AvailableDocumentTypes = docTypesInfo;

            //add the thumbnail path to the view bag
            ViewBag.DocTypeThumbnailBaseUrl = Url.Content(BackOfficeRequestContext.Application.Settings.UmbracoFolders.DocTypeThumbnailFolder);

            ViewBag.Title = CreateNewTitle;

            ViewBag.ControllerId = UmbracoController.GetControllerId<EditorAttribute>(GetType());
        }

        /// <summary>
        /// Creates a new TEditorModel based on the persisted doc type
        /// </summary>
        /// <param name="docTypeData"></param>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        protected virtual TEditorModel CreateNewContentEntity(EntitySchema docTypeData, string name, HiveId parentId)
        {
            Mandate.ParameterNotNull(docTypeData, "docTypeData");
            Mandate.ParameterNotEmpty(parentId, "parentId");

            //get doc type model
            var docType = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(docTypeData);
            //map (create) content model from doc type model
            var contentModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<DocumentTypeEditorModel, TEditorModel>(docType);
            contentModel.ParentId = parentId;
            contentModel.Name = name;
            return contentModel;
        }

        /// <summary>
        /// Returns the ActionResult for the CreateNew wizard view
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual ActionResult CreateNewView(CreateContentModel model)
        {
            Mandate.ParameterNotNull(model, "model");

            //lookup the doc type for the node id, find out which doc type children are allowed

            using (var uow = Hive.Create<IContentStore>())
            {
                var allSchemaTypeIds = uow.Repositories.Schemas.GetDescendentRelations(RootSchemaNodeId, FixedRelationTypes.DefaultRelationType)
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();

                var schemas = uow.Repositories.Schemas.Get<EntitySchema>(true, allSchemaTypeIds);

                //the filtered doc types to choose from based on the parent node (default is all of them)
                var filteredSchemas = schemas;

                //get the parent content if it's not the root
                if (model.ParentId != VirtualRootNodeId)
                {
                    //ensure the parent exists!
                    var parentEntity = uow.Repositories.Get<TypedEntity>(model.ParentId);
                    if (parentEntity == null)
                        throw new ArgumentException(string.Format("No content found for id: {0} on action CreateNew", model.ParentId));

                    //ensure the doc type exists!
                    //TODO: We reload the EntitySchema here so it has the right providerid, but as soon as TypedEntity.EntitySchema.Id gets mapped properly
                    //when loading TypedEntity we won't have to
                    var parentSc = uow.Repositories.Schemas.Get<EntitySchema>(parentEntity.EntitySchema.Id);
                    if (parentSc == null)
                        throw new ArgumentException(string.Format("No doc type found for id: {0} on action CreateNew",
                                                                  parentEntity.EntitySchema.Id));
                    var parentDocType = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(parentSc);

                    //filter the doc types to the allowed ones
                    filteredSchemas = schemas
                        .Where(x => parentDocType.AllowedChildIds.Contains(x.Id, new HiveIdComparer(true)))
                        .ToArray();
                }

                //validate the the selected doc type in the model is in fact one of the child doc types
                if (!model.SelectedDocumentTypeId.IsNullValueOrEmpty())
                {
                    if (!filteredSchemas.Select(x => x.Id)
                        .Contains(model.SelectedDocumentTypeId, new HiveIdComparer(true)))
                    {
                        ModelState.AddModelError("SelectedDocumentTypeId", "The selected document type id specified was not found in the allowed document types collection for the current node");
                    }
                }

                EnsureCreateWizardViewBagData(filteredSchemas);

                if (!filteredSchemas.Any())
                {
                    model.NoticeBoard.Add(new NotificationMessage("Content.NoChildTypesAllowed.Message".Localize(this), NotificationType.Warning));
                }
            }

            return View("CreateNew", model);
        }


    }
}
