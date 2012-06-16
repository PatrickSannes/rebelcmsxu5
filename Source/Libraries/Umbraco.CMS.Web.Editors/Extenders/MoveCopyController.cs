using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Mvc.ViewEngines;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors.Extenders
{
    /// <summary>
    /// Used to move & copy content
    /// </summary>
    public class MoveCopyController : ContentControllerExtenderBase        
    {
        
        private Guid _treeId;

        /// <summary>
        /// Constructor
        /// </summary>
        public MoveCopyController(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        {
            
        }

        /// <summary>
        /// This checks for the parent controller type and validates it, then sets the appropriate properties
        /// </summary>
        /// <param name="requestContext"></param>
        protected override void Initialize(global::System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            //get the extender data stored to access the additional parameters
            var extenderData = RouteData.GetControllerExtenderParameters();
            if (extenderData.Length != 1 && extenderData[0] is string)
                throw new LocalizedNotSupportedException("The AdditionalParameters must contain one item as a string representing the tree id");            
            _treeId = new Guid(extenderData[0].ToString());
        }

        protected override GroupUnitFactory Hive
        {
            get { return ContentController.Hive; }
        }

        /// <summary>
        /// Displays the copy dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Copy })]  
        public virtual ActionResult Copy(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            return MoveCopyView(id.Value, new CopyModel());
        }

        /// <summary>
        /// Handles the ajax request for the copy dialog
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("Copy")]
        [HttpPost]
        //[UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Copy })]  
        public virtual JsonResult CopyForm(CopyModel model)
        {
            if (!TryValidateModel(model))
            {
                return ModelState.ToJsonErrors();
            }
            
            return ProcessMoveCopy(model.SelectedItemId, model.ToId, (copyFrom, parentEntity, uow) =>
                {
                    //TODO: When doing a deep copy some things can't be copied such as the domains assigned to the node! SD.

                    //create a new copied entity
                    var copied = copyFrom.CreateDeepCopyToNewParentInRepo(parentEntity, FixedRelationTypes.DefaultRelationType, 0, uow);

                    uow.Repositories.AddOrUpdate(copied);
                    uow.Complete();

                    //TODO: How do we 'Relate to original' ?

                    return new Tuple<string, EntityPathCollection, string>(
                        "Copy.Success.Message".Localize(this, new
                            {
                                FromName = copyFrom.GetAttributeValueAsString(NodeNameAttributeDefinition.AliasValue, "Name"),
                                ToName = parentEntity.GetAttributeValueAsString(NodeNameAttributeDefinition.AliasValue, "Name")
                            }, encode: false),
                        uow.Repositories.GetEntityPaths<TypedEntity>(copied, FixedRelationTypes.DefaultRelationType),
                        "copy");
                });
        }

        /// <summary>
        /// Displays the move dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Move })]  
        public virtual ActionResult Move(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            return MoveCopyView(id.Value, new MoveModel());
        }

        /// <summary>
        /// Handles the ajax request for the move dialog
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("Move")]
        [HttpPost]
        //[UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Move })]  
        public virtual JsonResult MoveForm(MoveModel model)
        {
            if (!TryValidateModel(model))
            {
                return ModelState.ToJsonErrors();
            }

            return ProcessMoveCopy(model.SelectedItemId, model.ToId, (selectedEntity, toEntity, uow) =>
                {
                    var previousParents = uow.Repositories.GetParentRelations(selectedEntity, FixedRelationTypes.DefaultRelationType);
                    if (!previousParents.Any())
                        throw new InvalidOperationException(
                            "Could not find any parents for entity '{0}' with id {1}".InvariantFormat(
                                model.SelectedItemName, model.SelectedItemId));

                    // TODO: (APN) At the moment the MoveModel does not provide the single parent from which we're moving, 
                    // so we've had to load all parents. We need to change one, and remove the rest.
                    var toChange = previousParents.FirstOrDefault();
                    var rest = previousParents.Skip(1);
                    uow.Repositories.ChangeRelation(toChange, toEntity.Id, toChange.DestinationId);
                    rest.ForEach(x => uow.Repositories.RemoveRelation(x));

                    uow.Complete();

                    return new Tuple<string, EntityPathCollection, string>(
                        "Move.Success.Message".Localize(this, new
                            {
                                FromName = selectedEntity.GetAttributeValueAsString(NodeNameAttributeDefinition.AliasValue, "Name"),
                                ToName = toEntity.GetAttributeValueAsString(NodeNameAttributeDefinition.AliasValue, "Name")
                            }, encode: false),
                        uow.Repositories.GetEntityPaths<TypedEntity>(toChange.DestinationId, FixedRelationTypes.DefaultRelationType),
                        "move");

                });
        }

        /// <summary>
        /// Returns the ViewResult for use in the Move/Copy dialogs
        /// </summary>
        /// <returns></returns>
        protected ActionResult MoveCopyView(HiveId id, MoveModel model)
        {

            using (var uow = Hive.Create<IContentStore>())
            {
                var contentData = uow.Repositories.Get<TypedEntity>(id);
                if (contentData == null)
                    throw new ArgumentException(string.Format("No content found for id: {0} on action Move/Copy", id));

                //TODO: need to filter the start id based on the user's start node id, or just put that funcionality into the url helper

                //get the tree url for use in the copy/move dialog
                var treeUrl = Url.GetTreeUrl(new HiveId(_treeId), _treeId,
                                             new
                                                 {
                                                     //specify that the tree is in dialog mode
                                                     DialogMode = true,
                                                     //specify the onClick JS method handler for the node
                                                     OnNodeClick = "Umbraco.Editors.MoveCopyDialog.getInstance().nodeClickHandler"
                                                 });

                var contentItem = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<TypedEntity, ContentEditorModel>(contentData);

                model.SelectedItemId = contentItem.Id;
                model.SelectedItemName = contentItem.Name;
                model.TreeRenderModel = new TreeRenderModel(treeUrl, "moveCopyTree")
                    {
                        ShowContextMenu = false
                    };
                return View(model);
            }
        }

        /// <summary>
        /// Helper method to retreive the from/to entities and validate if the move/copy is allowed
        /// </summary>
        /// <param name="selectedItemId"></param>
        /// <param name="toId"></param>
        /// <param name="performSave"></param>
        protected JsonResult ProcessMoveCopy(
            HiveId selectedItemId, 
            HiveId toId,
            Func<TypedEntity, TypedEntity, IGroupUnit<IContentStore>, Tuple<string, EntityPathCollection, string>> performSave)
        {
            using (var uow = Hive.Create<IContentStore>())
            {
                var copyFrom = uow.Repositories.Get<TypedEntity>(selectedItemId);
                if (copyFrom == null)
                    ModelState.AddDataValidationError(string.Format("No content found for id: {0}", selectedItemId));

                var parentEntity = uow.Repositories.Get<TypedEntity>(toId);
                if (parentEntity == null)
                    ModelState.AddDataValidationError(string.Format("No content found for id: {0}", toId));

                //if it's not the root node, need to validate the move there
                if (ContentController.VirtualRootNodeId != toId)
                {
                    //Validate that we are allowed to go here
                    //TODO: We manually load the EntitySchema again here to ensure it has the correct provider id, pending a fix to have deep-level objects
                    //have their id remapped when loaded from Hive
                    var entitySchema = uow.Repositories.Schemas.Get<EntitySchema>(copyFrom.EntitySchema.Id);
                    if (entitySchema == null)
                        ModelState.AddDataValidationError(string.Format("Could not find the document type for the selected content, with id: {0}", parentEntity.EntitySchema.Id));

                    if (!ModelState.IsValid) return ModelState.ToJsonErrors();

                    var toDocType = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(parentEntity.EntitySchema);
                    if (!toDocType.AllowedChildIds.Contains(entitySchema.Id, new HiveIdComparer(true)))
                    {
                        ModelState.AddDataValidationError("The current node is not allowed under the chosen node because of its type");
                        return ModelState.ToJsonErrors();
                    }
                }

                //need to clear route engine cache, since we're moving/copying we need to clear all routingn cache
                this.BackOfficeRequestContext.RoutingEngine.ClearCache(clearAll:true);

                var result = performSave(copyFrom, parentEntity, uow);

                return new CustomJsonResult(new
                {
                    operation = result.Item3,
                    path = result.Item2.ToJson(),
                    success = true,
                    msg = result.Item1
                }.ToJsonString);
            }
        }
    }
}