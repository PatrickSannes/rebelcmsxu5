using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Framework;

using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.UserGroupEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class UserGroupEditorController : AbstractContentEditorController
    {
        public UserGroupEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("security://user-groups"));

            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route security://user-groups"));
        }

        private readonly GroupUnitFactory _hive;

        public override GroupUnitFactory Hive
        {
            get { return _hive; }
        }


        #region Actions

        /// <summary>
        /// Action to render the editor
        /// </summary>
        /// <returns></returns>        
        public override ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = Hive.Create<ISecurityStore>())
            {
                var userEntity = uow.Repositories.Get<UserGroup>(id.Value);
                if (userEntity == null)
                    throw new ArgumentException(string.Format("No user group found for id: {0} on action Edit", id));

                var userViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<UserGroup, UserGroupEditorModel>(userEntity);

                PopulatePermissions(userViewModel);

                return View(userViewModel);
            }
        }

        /// <summary>
        /// Handles the editor post back
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [ActionName("Edit")]
        [HttpPost]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [PersistTabIndexOnRedirect]
        [Save]
        public ActionResult EditForm(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = Hive.Create<ISecurityStore>())
            {
                var userEntity = uow.Repositories.Get<UserGroup>(id);

                if (userEntity == null)
                    throw new ArgumentException(string.Format("No entity for id: {0} on action EditForm", id));

                var userGroupViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<UserGroup, UserGroupEditorModel>(userEntity);

                PopulatePermissions(userGroupViewModel);

                //need to ensure that all of the Ids are mapped correctly, when editing existing content the only reason for this
                //is to ensure any new document type properties that have been created are reflected in the new content revision
                ReconstructModelPropertyIds(userGroupViewModel);

                return ProcessSubmit(userGroupViewModel, userEntity);
            }
        }

        /// <summary>
        /// Displays the Create user editor 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult Create()
        {
            //create the new user item
            var userGroupViewModel = CreateNewUserGroup();

            PopulatePermissions(userGroupViewModel);

            return View("Edit", userGroupViewModel);
        }

        /// <summary>
        /// Creates a new user based on posted values
        /// </summary>
        /// <returns></returns>
        [ActionName("Create")]
        [HttpPost]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [PersistTabIndexOnRedirect]
        [Save]
        public ActionResult CreateForm()
        {
            var userGroupViewModel = CreateNewUserGroup();

            PopulatePermissions(userGroupViewModel);

            //map the Ids correctly to the model so it binds
            ReconstructModelPropertyIds(userGroupViewModel);

            return ProcessSubmit(userGroupViewModel, null);
        }


        #endregion

        #region Protected/Private methods

        /// <summary>
        /// Populates the permissions for the given model.
        /// </summary>
        /// <param name="model">The model.</param>
        private void PopulatePermissions(UserGroupEditorModel model)
        {
            // Get all permissions
            var permissions = BackOfficeRequestContext.RegisteredComponents.Permissions
                .Select(x => x.Metadata)
                .OrderByDescending(x => x.Type)
                .ThenBy(x => x.Name)
                .ToList();

            var permissionStatusModels = permissions.Select(x => BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<PermissionStatusModel>(x)).ToList();
            
            //TODO: There is currently a bug with hive id when returned by relations where the ProviderGroupRoot is null, so we have to create a similar hive id to do comparisons with
            //var userGroupId = new HiveId((Uri)null, model.Id.ProviderId, model.Id.Value);
            var userGroupId = model.Id;

            foreach (var permissionStatusModel in permissionStatusModels)
            {
                // Set status
                var permissionInheritKey = "__permission___" + permissionStatusModel.PermissionId + "_inherit";
                var permissionStatusKey = "__permission___" + permissionStatusModel.PermissionId + "_status";
                permissionStatusModel.Status = !string.IsNullOrWhiteSpace(Request.Form[permissionInheritKey]) 
                    ? PermissionStatus.Inherit
                    : !string.IsNullOrWhiteSpace(Request.Form[permissionStatusKey])
                        ? (PermissionStatus)Enum.Parse(typeof(PermissionStatus), Request.Form[permissionStatusKey])
                        : BackOfficeRequestContext.Application.Security.GetExplicitPermission(permissionStatusModel.PermissionId, new[] { userGroupId }, FixedHiveIds.SystemRoot).Status;
            }

            model.Permissions = permissionStatusModels;
        }

        /// <summary>
        /// Creates a blank user model based on the document type/entityschema for the user
        /// </summary>
        /// <returns></returns>
        private UserGroupEditorModel CreateNewUserGroup()
        {
            using (var uow = Hive.Create<ISecurityStore>())
            {
                var userSchema = uow.Repositories.Schemas.GetAll<EntitySchema>()
                    .Where(x => x.Alias == UserGroupSchema.SchemaAlias)
                    .Single();
                //get doc type model
                var docType = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(userSchema);
                //map (create) content model from doc type model
                return BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<DocumentTypeEditorModel, UserGroupEditorModel>(docType);
            }
        }

        protected ActionResult ProcessSubmit(UserGroupEditorModel model, UserGroup entity)
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
            using (var uow = Hive.Create<ISecurityStore>())
            {

                if (entity == null)
                {
                    //map to new entity
                    entity =
                        BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map
                            <UserGroupEditorModel, UserGroup>(model);
                }
                else
                {
                    //map to existing entity
                    BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(model, entity);
                }

                uow.Repositories.AddOrUpdate(entity);

                // Save permissions
                var metaDataumList = new List<RelationMetaDatum>();
                foreach (var permissionModel in model.Permissions)
                {
                    var permission = BackOfficeRequestContext.RegisteredComponents.Permissions.SingleOrDefault(x => x.Metadata.Id == permissionModel.PermissionId);
                    if (permission == null)
                        throw new NullReferenceException("Could not find permission with id " + permissionModel.PermissionId);

                    metaDataumList.Add(BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<RelationMetaDatum>(permissionModel));
                }

                // Change permissions relation
                uow.Repositories.ChangeOrCreateRelationMetadata(entity.Id, FixedHiveIds.SystemRoot, FixedRelationTypes.PermissionRelationType, metaDataumList.ToArray());
                
                uow.Complete();

                Notifications.Add(new NotificationMessage(
                    "UserGroup.Save.Message".Localize(this),
                    "UserGroup.Save.Title".Localize(this),
                    NotificationType.Success));

                //add path for entity for SupportsPathGeneration (tree syncing) to work
                GeneratePathsForCurrentEntity(uow.Repositories.GetEntityPaths<TypedEntity>(entity.Id, FixedRelationTypes.DefaultRelationType));
                
                return RedirectToAction("Edit", new { id = entity.Id });
            }

        }

        #endregion

    }
}
