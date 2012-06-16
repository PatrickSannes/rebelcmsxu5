using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Context;


using System.Web.Mvc;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.UserEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class UserEditorController : AbstractContentEditorController
    {
        public UserEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("security://users"));
           
            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route security://users"));
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
                var userEntity = uow.Repositories.Get<User>(id.Value);
                if (userEntity == null)
                    throw new ArgumentException(string.Format("No user found for id: {0} on action Edit", id));

                var userViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<User, UserEditorModel>(userEntity);
                //TODO: Shouldn't this be done in the mappings ?
                userViewModel.UserGroups = uow.Repositories.GetParentRelations(userEntity.Id, FixedRelationTypes.UserGroupRelationType)
                    .Select(x => x.SourceId).ToArray();

                EnsureViewBagData();

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
        public ActionResult EditForm(HiveId? id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = Hive.Create<ISecurityStore>())
            {
                var userEntity = uow.Repositories.Get<User>(id.Value);

                if (userEntity == null)
                    throw new ArgumentException(string.Format("No entity for id: {0} on action EditForm", id));

                var userViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<User, UserEditorModel>(userEntity);
                userViewModel.UserGroups =
                    uow.Repositories.GetParentRelations(userEntity.Id, FixedRelationTypes.UserGroupRelationType).Select
                        (x => x.SourceId).ToArray();

                //need to ensure that all of the Ids are mapped correctly, when editing existing content the only reason for this
                //is to ensure any new document type properties that have been created are reflected in the new content revision
                ReconstructModelPropertyIds(userViewModel);

                EnsureViewBagData();

                return ProcessSubmit(userViewModel, userEntity);
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
            var userViewModel = CreateNewUser();

            EnsureViewBagData();

            return View("Edit", userViewModel);
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
            var userViewModel = CreateNewUser();

            //need to ensure that all of the Ids are mapped correctly, when editing existing content the only reason for this
            //is to ensure any new document type properties that have been created are reflected in the new content revision
            ReconstructModelPropertyIds(userViewModel);

            EnsureViewBagData();

            return ProcessSubmit(userViewModel, null);
        }
        

        #endregion

        #region Protected/Private methods

        private void EnsureViewBagData()
        {
            var uowFactory = BackOfficeRequestContext.Application.Hive.GetReader<ISecurityStore>();
            using (var uow = uowFactory.CreateReadonly())
            {
                ViewBag.AvailableUserGroups = uow.Repositories.GetEntityByRelationType<UserGroup>(
                    FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                    .OrderBy(x => x.Name);
            }
        }

        /// <summary>
        /// Creates a blank user model based on the document type/entityschema for the user
        /// </summary>
        /// <returns></returns>
        private UserEditorModel CreateNewUser()
        {
            using (var uow = Hive.Create<ISecurityStore>())
            {
                var userSchema = uow.Repositories.Schemas.GetAll<EntitySchema>()
                    .Where(x => x.Alias == UserSchema.SchemaAlias)
                    .Single();
                //get doc type model
                var docType = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(userSchema);
                //map (create) content model from doc type model
                return BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<DocumentTypeEditorModel, UserEditorModel>(docType);            
            }            
        }

        /// <summary>
        /// Processes the submit.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected ActionResult ProcessSubmit(UserEditorModel model, User entity)
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
                // Map the user
                if (entity == null)
                {
                    //map to new entity, set default date values
                    model.LastPasswordChangeDate = DateTime.UtcNow;
                    model.LastActivityDate = DateTime.UtcNow;
                    model.LastLoginDate = DateTime.UtcNow;
                    entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<UserEditorModel, User>(model);
                }
                else
                {
                    //map to existing entity
                    BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(model, entity);
                }

                uow.Repositories.AddOrUpdate(entity);

                // Remove any removed user groups
                foreach (var relation in uow.Repositories.GetParentRelations(entity.Id, FixedRelationTypes.UserGroupRelationType)
                    .Where(x => !model.UserGroups.Contains(x.SourceId)))
                {
                    uow.Repositories.RemoveRelation(relation);
                }

                // Add any new user groups
                var existingRelations = uow.Repositories.GetParentRelations(entity.Id, FixedRelationTypes.UserGroupRelationType).Select(x => x.SourceId).ToArray();
                foreach (var userGroupId in model.UserGroups.Where(x => !existingRelations.Contains(x)))
                {
                    uow.Repositories.AddRelation(new Relation(FixedRelationTypes.UserGroupRelationType, userGroupId, entity.Id));
                }

                uow.Complete();

                //we may have changed the user data, so we need to ensure that the latest user data exists in the Identity object so we'll re-issue a forms auth ticket here
                if (HttpContext.User.Identity.Name.InvariantEquals(entity.Username))
                {
                    HttpContext.CreateUmbracoAuthTicket(entity);
                }

                Notifications.Add(new NotificationMessage(
                       "User.Save.Message".Localize(this),
                       "User.Save.Title".Localize(this),
                       NotificationType.Success));

                //add path for entity for SupportsPathGeneration (tree syncing) to work
                GeneratePathsForCurrentEntity(uow.Repositories.GetEntityPaths<TypedEntity>(entity.Id, FixedRelationTypes.DefaultRelationType));

                return RedirectToAction("Edit", new { id = entity.Id });
            }

            

        }

        #endregion
    }
}
