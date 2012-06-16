using System;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.MemberEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class MemberEditorController : AbstractContentEditorController
    {
        public MemberEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("security://members"));

            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route security://members"));
        }

        private readonly GroupUnitFactory _hive;

        public override GroupUnitFactory Hive
        {
            get { return _hive; }
        }

        public override ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = Hive.Create<ISecurityStore>())
            {
                var userEntity = uow.Repositories.Get<Member>(id.Value);
                if (userEntity == null)
                    throw new ArgumentException(string.Format("No member found for id: {0} on action Edit", id));

                var userViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<Member, MemberEditorModel>(userEntity);
                return View(userViewModel);
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
            var userViewModel = CreateNewMember();

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
            var userViewModel = CreateNewMember();
            
            return ProcessSubmit(userViewModel, null);
        }

        /// <summary>
        /// Creates a blank member model based on the document type/entityschema for the user
        /// </summary>
        /// <returns></returns>
        private MemberEditorModel CreateNewMember()
        {
            using (var uow = Hive.Create<ISecurityStore>())
            {
                //var memberSchema = new MemberSchema();
                //get doc type model
                //var docType = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(memberSchema);
                //map (create) content model from doc type model
                var member = new Member();
                return BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<Member, MemberEditorModel>(member);
            }
        }

        /// <summary>
        /// Processes the submit.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected ActionResult ProcessSubmit(MemberEditorModel model, Member entity)
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
                    entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<MemberEditorModel, Member>(model);
                }
                else
                {
                    //map to existing entity
                    BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(model, entity);
                }

                uow.Repositories.AddOrUpdate(entity);

                //// Remove any removed user groups
                //foreach (var relation in uow.Repositories.GetParentRelations(entity.Id, FixedRelationTypes.UserGroupRelationType)
                //    .Where(x => !model.UserGroups.Contains(x.SourceId)))
                //{
                //    uow.Repositories.RemoveRelation(relation);
                //}

                //// Add any new user groups
                //var existingRelations = uow.Repositories.GetParentRelations(entity.Id, FixedRelationTypes.UserGroupRelationType).Select(x => x.SourceId).ToArray();
                //foreach (var userGroupId in model.UserGroups.Where(x => !existingRelations.Contains(x)))
                //{
                //    uow.Repositories.AddRelation(new Relation(FixedRelationTypes.UserGroupRelationType, userGroupId, entity.Id));
                //}

                uow.Complete();

                ////we may have changed the user data, so we need to ensure that the latest user data exists in the Identity object so we'll re-issue a forms auth ticket here
                //if (HttpContext.User.Identity.Name.InvariantEquals(entity.Username))
                //{
                //    HttpContext.CreateUmbracoAuthTicket(entity);
                //}

                Notifications.Add(new NotificationMessage(
                       "User.Save.Message".Localize(this),
                       "User.Save.Title".Localize(this),
                       NotificationType.Success));

                //add path for entity for SupportsPathGeneration (tree syncing) to work
                GeneratePathsForCurrentEntity(uow.Repositories.GetEntityPaths<TypedEntity>(entity.Id, FixedRelationTypes.DefaultRelationType));

                return RedirectToAction("Edit", new { id = entity.Id });
            }



        }
        
    }
}