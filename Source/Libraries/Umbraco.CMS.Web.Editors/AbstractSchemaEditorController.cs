using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    public abstract class AbstractSchemaEditorController<TEditorModel, TCreateModel> : StandardEditorController
        where TEditorModel : AbstractSchemaEditorModel
        where TCreateModel : CreateContentModel, new()
    {
        protected AbstractSchemaEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        protected abstract string CreateNewTitle { get; }
        public abstract GroupUnitFactory Hive { get; }
        protected abstract HiveId RootSchema { get; }

        private EntitySchema _rootSchema = null;
        protected EntitySchema RootSchemaObj
        {
            get
            {
                if (_rootSchema != null) return _rootSchema;
                using (var uow = Hive.Create<IContentStore>())
                {
                    _rootSchema = uow.Repositories.Schemas.Get<EntitySchema>(RootSchema);
                }
                return _rootSchema;
            }
        }

        /// <summary>
        /// Action to render the editor
        /// </summary>
        /// <returns></returns>
        public override ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            //get the item from the repo
            using (var uow = Hive.Create<IContentStore>())
            {
                var item = uow.Repositories.Schemas.GetComposite<EntitySchema>(id.Value);

                if (item == null) return HttpNotFound();

                //create a editor model from it and map properties
                var model = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, TEditorModel>(item);
                EnsureSelectListData(model);
                EnsureNoInBuiltProperties(model);

                return View(model);
            }
        }

        /// <summary>
        /// Action to edit the data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("Edit")]
        [HttpPost]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [PersistTabIndexOnRedirect]
        [Save(SaveAttribute.DefaultSaveButtonId, "submit.Tab", "submit.DeleteTab", "submit.DeletePanel")]
        public ActionResult EditForm(HiveId? id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = Hive.Create<IContentStore>())
            {
                var item = uow.Repositories.Schemas.GetComposite<EntitySchema>(id.Value);

                if (item == null)
                    throw new ArgumentException(string.Format("No document type found for id: {0} on action EditForm", id));

                //create a editor model from it and map properties
                var model = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, TEditorModel>(item);

                OnBeforeUpdate(model); //TODO: Make this an event/task?

                var redirectResult = ProcessSubmit(model, item, uow);

                uow.Complete();

                //Path generation needs to happen in a new unit of work to ensure update of relations
                using (var uow2 = Hive.Create<IContentStore>())
                {
                    //add path for entity for SupportsPathGeneration (tree syncing) to work
                    GeneratePathsForCurrentEntity(uow2.Repositories.Schemas.GetEntityPaths<EntitySchema>(id.Value, FixedRelationTypes.DefaultRelationType));
                }

                return redirectResult;
            }
        }

        public virtual void OnBeforeUpdate(TEditorModel model)
        {
            //to be overriden
        }

        /// <summary>
        /// JSON action to delete a node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public JsonResult Delete(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = Hive.Create<IContentStore>())
            {
                uow.Repositories.Schemas.Delete<EntitySchema>(id);
                uow.Complete();
            }

            //return a successful JSON response
            return Json(new { message = "Success" });
        }

        /// <summary>
        /// Renders the create new document type wizard step
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult CreateNew(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            EnsureCreateWizardViewBagData(id.Value);

            return View(new TCreateModel());
        }

        /// <summary>
        /// Handles the post back for the create wizard step
        /// </summary>
        /// <param name="createModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CreateNew")]
        [SupportsPathGeneration]
        [Save]
        public virtual ActionResult CreateNewForm(TCreateModel createModel)
        {
            if (!TryValidateModel(createModel))
            {
                EnsureCreateWizardViewBagData(createModel.ParentId);
                return View(createModel);
            }

            //everything is valid, now we need to render out the editor for this document type without any data
            using (var uow = Hive.Create<IContentStore>())
            {
                var parentId = HiveId.Empty;

                if (!createModel.SelectedDocumentTypeId.IsNullValueOrEmpty())
                {
                    var parentSchema = uow.Repositories.Schemas.Get<EntitySchema>(createModel.SelectedDocumentTypeId);
                    if (parentSchema == null)
                        throw new ArgumentException(string.Format("No parent type found for id: {0} on action CreateNewForm",
                                          createModel.SelectedDocumentTypeId));
                    parentId = parentSchema.Id;
                }

                var generalGroup = FixedGroupDefinitions.GeneralGroup;
                var generalTab = new Tab
                {
                    Alias = generalGroup.Alias,
                    Id = generalGroup.Id,
                    Name = generalGroup.Name,
                    SortOrder = generalGroup.Ordinal
                };

                //create a new model to bind values to
                var model = CreateNewEditorModel(createModel.Name, generalTab);

                model.Alias = createModel.Name.ToUmbracoAlias();
                model.InheritFromIds = new List<HiveId> { parentId };

                EnsureSelectListData(model);
                EnsureNoInBuiltProperties(model);

                OnBeforeCreate(createModel, model); //TODO: Make this an event/task?

                return ProcessCreate(model);
            }
        }

        public virtual void OnBeforeCreate(TCreateModel createModel, TEditorModel editorModel)
        {
            //to be overriden
        }

        protected abstract TEditorModel CreateNewEditorModel(string name = null, Tab genericTab = null);

        private ActionResult ProcessCreate(TEditorModel model)
        {
            Mandate.ParameterNotNull(model, "model");

            using (var uow = Hive.Create<IContentStore>())
            {
                //Map model to entit
                var entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TEditorModel, EntitySchema>(model);

                //Save the entity
                uow.Repositories.Schemas.AddOrUpdate(entity);

                // Manage relations
                EnsureInheritanceRelations(model, entity, uow);

                uow.Complete();

                Notifications.Add(new NotificationMessage(
                                      "DocumentType.Save.Message".Localize(this),
                                      "DocumentType.Save.Title".Localize(this),
                                      NotificationType.Success));

                //add path for entity for SupportsPathGeneration (tree syncing) to work
                GeneratePathsForCurrentEntity(uow.Repositories.Schemas.GetEntityPaths<EntitySchema>(entity.Id, FixedRelationTypes.DefaultRelationType));

                return RedirectToAction("Edit", new { id = entity.Id });
            }
        }

        /// <summary>
        /// When editing or creating a document type, this binds the model, checks for errors, determines which 
        /// actions to take based on the button pressed, adds appropriate notifications and persists the data.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private ActionResult ProcessSubmit(TEditorModel model, EntitySchema entity, IGroupUnit<IContentStore> uow)
        {
            Mandate.ParameterNotNull(model, "model");

            EnsureSelectListData(model);

            //bind the model to the posted values
            model.BindModel(this);

            //process creating a new tab
            ProcessCreatingTab(model);
            //process deleting a tab
            var tabToDelete = ProcessDeletingTab(model);
            //process deleting property
            var propToDelete = ProcessDeletingProperty(model);

            //check if we are NOT adding a new property, if not then remove the invalid required validation
            if (!model.IsCreatingNewProperty)
            {
                foreach (var m in ModelState.Where(x => x.Key.StartsWith("NewProperty")))
                {
                    m.Value.Errors.Clear();
                }
            }

            if (!ModelState.IsValid)
            {
                AddValidationErrorsNotification();
                return View("Edit", model);
            }

            //convert to persistence entity
            if (entity == null)
            {
                //map to a new entity
                entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TEditorModel, EntitySchema>(model);
            }
            else
            {
                //map to existing
                BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(model, entity);
            }

            //if we're creating a new property, then add one
            if (model.IsCreatingNewProperty)
            {
                //need to set the ordinal to the max sort order for the properties on the tab its being saved to
                var defsOnGroup = entity.AttributeDefinitions.Where(x => x.AttributeGroup.Id == model.NewProperty.TabId).ToArray();
                var maxOrdinal = defsOnGroup.Any() ? defsOnGroup.Max(x => x.Ordinal) : 0;


                model.NewProperty.SortOrder = entity.AttributeDefinitions.Any()
                                                  ? maxOrdinal + 1
                                                  : 0;
                var propertyEntity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<DocumentTypeProperty, AttributeDefinition>(model.NewProperty);
                entity.AttributeDefinitions.Add(propertyEntity);
            }

            // Manage relations
            EnsureInheritanceRelations(model, entity, uow);

            //Save the entity
            uow.Repositories.Schemas.AddOrUpdate(entity);

            Notifications.Add(new NotificationMessage(
                                  "DocumentType.Save.Message".Localize(this),
                                  "DocumentType.Save.Title".Localize(this),
                                  NotificationType.Success));

            return RedirectToAction("Edit", new { id = entity.Id });
        }

        protected virtual void EnsureInheritanceRelations(TEditorModel model, EntitySchema entity, IGroupUnit<IContentStore> uow)
        {
            var currentRelations =
                uow.Repositories.GetParentRelations(entity.Id, FixedRelationTypes.DefaultRelationType).ToArray();

            var selectedInheritFrom =
                model.InheritFrom.Where(
                    x => x.Selected && !model.InheritFrom.Any(y => y.Selected && y.ParentValues.Contains(x.Value))).Select(
                        x => HiveId.Parse(x.Value)).ToArray();

            // Remove relations provided we're not going to remove the only relation to the root
            if (!selectedInheritFrom.Any())
            {
                foreach (var relation in currentRelations.Where(x => x.SourceId != RootSchemaObj.Id))
                {
                    uow.Repositories.RemoveRelation(relation);
                }

                // Ensure we have a relation to the root schema
                uow.Repositories.AddRelation(new Relation(FixedRelationTypes.DefaultRelationType, RootSchemaObj.Id, entity.Id));
            }
            else
            {
                foreach (var relation in currentRelations.Where(x => !selectedInheritFrom.Any(hiveId => hiveId == x.SourceId)))
                {
                    uow.Repositories.RemoveRelation(relation);
                }
            }

            // Go through the selected inheritance and add a relation
            foreach (var id in selectedInheritFrom.Where(id => !currentRelations.Any(y => y.SourceId == id)))
            {
                uow.Repositories.AddRelation(new Relation(FixedRelationTypes.DefaultRelationType, id, entity.Id));
            }
        }

        /// <summary>
        /// Processing deleting a property if the delete property button is pressed
        /// </summary>
        /// <param name="model"></param>
        private DocumentTypeProperty ProcessDeletingProperty(TEditorModel model)
        {
            if (ValueProvider.GetValue("submit.DeletePanel") != null)
            {
                var propertyId = ValueProvider.GetValue("submit.DeletePanel").AttemptedValue;

                var toDelete = model.Properties.Where(x => x.Id == HiveId.Parse(propertyId)).Single();

                model.Properties.Remove(toDelete);

                Notifications.Add(new NotificationMessage(
                                      "DocumentType.PropertyDeleted.Message".Localize(this),
                                      "DocumentType.PropertyDeleted.Title".Localize(this),
                                      NotificationType.Success));

                return toDelete;
            }
            return null;
        }

        /// <summary>
        /// Handles processing of deleting a tab if the delete tab button was clicked
        /// </summary>
        /// <param name="model"></param>
        /// <returns>
        /// Returns the attribute group def to delete
        /// </returns>
        private Tab ProcessDeletingTab(TEditorModel model)
        {
            if (ValueProvider.GetValue("submit.DeleteTab") != null)
            {
                var tabIdToDelete = ValueProvider.GetValue("submit.DeleteTab").AttemptedValue;

                //need to see if we have the generic properties tab available to move the properties on this tab to.
                //generally there should always be a generic properties tab, but we'll double check to make sure
                var genericTab =
                    model.DefinedTabs.Where(x => x.Alias == FixedGroupDefinitions.GeneralGroupAlias).
                        SingleOrDefault();
                if (genericTab == null)
                {
                    //we need to create the generic tab
                    genericTab = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<AttributeGroup, Tab>(FixedGroupDefinitions.GeneralGroup);
                    model.DefinedTabs.Add(genericTab);
                }

                //now, we need to move all propertie that were on the tab that we want to delete to the generic tab
                var tabToDelete = model.DefinedTabs.Where(x => x.Id == HiveId.Parse(tabIdToDelete)).Single();
                foreach (var prop in model.Properties.Where(x => x.TabId == tabToDelete.Id))
                {
                    prop.TabId = genericTab.Id;
                }

                //now we can finally delete the tab
                model.DefinedTabs.Remove(tabToDelete);

                Notifications.Add(new NotificationMessage(
                                      "DocumentType.TabDeleted.Message".Localize(this),
                                      "DocumentType.TabDeleted.Title".Localize(this),
                                      NotificationType.Success));

                return tabToDelete;
            }

            return null;
        }

        /// <summary>
        /// Checks if a tab requires to be added, if so does the processing
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private void ProcessCreatingTab(TEditorModel model)
        {
            //validate the empty tab if the tab button was pressed
            if (ValueProvider.GetValue("submit.Tab") != null)
            {
                if (string.IsNullOrEmpty(model.NewTabName))
                {
                    //the add tab button was pressed, need to ensure that the tab has a name
                    ModelState.AddModelError("NewTabName", "DocumentType.Validation.RequiredTabName".Localize(this));
                }
                else if (model.DefinedTabs.Select(x => x.Alias).Contains(model.NewTabName.ToUmbracoAlias()))
                {
                    //need to ensure there isn't a duplicate tab name added
                    ModelState.AddModelError("NewTabName", "DocumentType.Validation.DuplicateTab".Localize(this));
                }
                else
                {
                    var nextSortOrder = model.DefinedTabs.Any(x => x.Alias != FixedGroupDefinitions.GeneralGroupAlias)
                                            ? model.DefinedTabs
                                                  .Where(x => x.Alias != FixedGroupDefinitions.GeneralGroupAlias)
                                                  .Max(x => x.SortOrder) + 1
                                            : 0;
                    model.DefinedTabs.Add(
                        new Tab
                            {
                                Alias = model.NewTabName.ToUmbracoAlias(),
                                Name = model.NewTabName,
                                SortOrder = nextSortOrder
                            });

                    Notifications.Add(new NotificationMessage(
                                          "DocumentType.TabAdded.Message".Localize(this),
                                          "DocumentType.TabAdded.Title".Localize(this),
                                          NotificationType.Success));
                }
            }
        }

        /// <summary>
        /// Ensures the select lists are built for the model
        /// </summary>
        /// <param name="model"></param>
        protected virtual void EnsureSelectListData(TEditorModel model)
        {
            //set the icons/thumbnails select list
            model.AvailableIcons = new List<SelectListItem>(BackOfficeRequestContext.DocumentTypeIconResolver
                .Resolve()
                .Select(f =>
                    {
                        if (f.IconType == IconType.Image)
                        {
                            return new SelectListItem { Text = f.Name, Value = f.Url };
                        }
                        return new SelectListItem { Text = "." + f.Name, Value = DocumentTypeIconFileResolver.SpriteNamePrefixValue + f.Name };
                    })).OrderBy(x => x.Text).ToArray();

            model.AvailableThumbnails = new List<SelectListItem>(BackOfficeRequestContext.DocumentTypeThumbnailResolver
                .Resolve()
                .Select(f =>
                    {
                        if (f.IconType == IconType.Image)
                        {
                            return new SelectListItem { Text = f.Name, Value = VirtualPathUtility.GetFileName(f.Url) };
                        }
                        return new SelectListItem { Text = "." + f.Name, Value = DocumentTypeThumbnailFileResolver.SpriteNamePrefixValue + f.Name };
                    })).OrderBy(x => x.Text).ToArray();

            model.IconsBaseUrl = BackOfficeRequestContext.Application.Settings.UmbracoFolders.DocTypeIconFolder;
            model.ThumbnailsBaseUrl = BackOfficeRequestContext.Application.Settings.UmbracoFolders.DocTypeThumbnailFolder;

            model.SpriteFileUrls = BackOfficeRequestContext.DocumentTypeIconResolver.Sprites.Select(x =>
                BackOfficeRequestContext.Application.Settings.UmbracoFolders.DocTypeIconFolder + "/" + x.Value.Name);

            using (var uow = Hive.Create<IContentStore>())
            {
                //get the children for the current virtual root
                //var children = uow.Repositories.Schemas.GetEntityByRelationType<EntitySchema>(FixedRelationTypes.DefaultRelationType, RootSchema).ToArray();

                var allSchemaTypeIds = uow.Repositories.Schemas.GetDescendentRelations(RootSchema, FixedRelationTypes.DefaultRelationType)
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();
                var allSchemaTypes = uow.Repositories.Schemas.Get<EntitySchema>(true, allSchemaTypeIds);

                var entityDesendentSchemaTypeIds = uow.Repositories.Schemas.GetDescendentRelations(model.Id, FixedRelationTypes.DefaultRelationType)
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();


                //get the allowed child document types select list
                model.AllowedChildren = new List<SelectListItem>(
                    allSchemaTypes.Where(x => !x.IsAbstract)
                                  .Select(x =>
                                    new SelectListItem
                                        {
                                            Text = x.Name,
                                            Value = x.Id.ToString(),
                                            Selected = model.AllowedChildIds.Contains(x.Id, new HiveIdComparer(true))
                                        })).OrderBy(x => x.Text).ToArray();

                model.InheritFrom = new List<HierarchicalSelectListItem>(
                    allSchemaTypes.Where(x => x.Id != model.Id && !entityDesendentSchemaTypeIds.Contains(x.Id)).Select(x =>
                                    new HierarchicalSelectListItem
                                    {
                                        Text = x.Name,
                                        Value = x.Id.ToString(),
                                        ParentValues = x.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType).Where(y => y.Item.SourceId.Value != RootSchema.Value).Select(y => y.Item.SourceId.ToString()).ToArray(),
                                        Selected = model.InheritFromIds.Contains(x.Id, new HiveIdComparer(true))
                                    })).OrderBy(x => x.Text).ToArray();

                //get the data type list
                model.AvailableDataTypes = uow.Repositories.Schemas.GetAll<AttributeType>()
                    .Where(x => !x.Id.IsSystem())
                    .Select(x =>
                            new SelectListItem
                                {
                                    Text = x.Name,
                                    Value = x.Id.ToString()
                                }).OrderBy(x => x.Text).ToArray();

            }

        }

        /// <summary>
        /// remove the 'in-built' properties before rendering
        /// </summary>
        /// <param name="model"></param>
        protected void EnsureNoInBuiltProperties(TEditorModel model)
        {
            model.Properties.RemoveWhere(x => x.Alias.StartsWith("system-"));
            model.InheritedProperties.RemoveWhere(x => x.Alias.StartsWith("system-"));
        }

        /// <summary>
        /// This adds some required elements to the ViewBag so that the Create view renders correctly
        /// </summary>
        protected virtual void EnsureCreateWizardViewBagData(HiveId parentId)
        {
            ViewBag.ControllerId = UmbracoController.GetControllerId<EditorAttribute>(GetType());
            ViewBag.Title = CreateNewTitle;

            using (var uow = Hive.Create<IContentStore>())
            {
                //get all the document types under the specified parent
                var schemas = uow.Repositories.Schemas.GetEntityByRelationType<EntitySchema>(FixedRelationTypes.DefaultRelationType, parentId);
                var docTypesInfo = schemas.Select(BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeInfo>);

                ViewBag.AvailableDocumentTypes =
                    new[] { new DocumentTypeInfo() { Id = parentId, Name = "[None]" } }.Concat(docTypesInfo);
                //add the thumbnail path to the view bag
                ViewBag.DocTypeThumbnailBaseUrl = Url.Content(BackOfficeRequestContext.Application.Settings.UmbracoFolders.DocTypeThumbnailFolder);
            }

        }
    }
}