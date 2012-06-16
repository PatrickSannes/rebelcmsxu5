using System;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ViewEngines;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Security;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors.Extenders
{
    /// <summary>
    /// Used for the publish dialog
    /// </summary> 
    public class PublishController : ContentControllerExtenderBase
    {
        public PublishController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// Displays the publish dialog
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Publish })] 
        public ActionResult Publish(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = Hive.Create<IContentStore>())
            {
                var contentEntity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(id.Value);
                if (contentEntity == null)
                    throw new ArgumentException(string.Format("No entity found for id: {0} on action Publish", id));

                return View(new PublishModel
                    {
                        Id = contentEntity.Item.Id,
                        Name = contentEntity.Item.GetAttributeValueAsString(NodeNameAttributeDefinition.AliasValue, "UrlName")
                    });
            }
        }

        /// <summary>
        /// Handles the ajax request for the publish dialog
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("Publish")]
        //[UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Publish })] 
        public JsonResult PublishForm(PublishModel model)
        {
            if (!TryValidateModel(model))
            {
                return ModelState.ToJsonErrors();
            }

            using (var uow = Hive.Create<IContentStore>())
            {
                var contentEntity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(model.Id);
                if (contentEntity == null)
                    throw new ArgumentException(string.Format("No entity found for id: {0} on action PublishForm", model.Id));

                //get its children recursively
                if (model.IncludeChildren)
                {
                    // Get all descendents
                    var descendents = uow.Repositories.GetDescendentRelations(model.Id, FixedRelationTypes.DefaultRelationType);

                    foreach (var descendent in descendents)
                    {
                        //get the revision 
                        var revisionEntity = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(descendent.DestinationId);

                        //publish it if it's already published or if the user has specified to publish unpublished content
                        if (revisionEntity != null && (revisionEntity.MetaData.StatusType.Alias == FixedStatusTypes.Published.Alias) || model.IncludeUnpublishedChildren)
                        {
                            var publishRevision = revisionEntity.CopyToNewRevision(FixedStatusTypes.Published);
                            uow.Repositories.Revisions.AddOrUpdate(publishRevision);
                        }
                    }
                }

                //publish this node
                var toPublish = contentEntity.CopyToNewRevision(FixedStatusTypes.Published);
                uow.Repositories.Revisions.AddOrUpdate(toPublish);

                //save
                uow.Complete();

                var contentViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<Revision<TypedEntity>, ContentEditorModel>(toPublish);

                Notifications.Add(new NotificationMessage(
                                      model.IncludeChildren
                                          ? "Publish.ChildrenSuccess.Message".Localize(this, new { contentViewModel.Name }, encode: false)
                                          : "Publish.SingleSuccess.Message".Localize(this, new { contentViewModel.Name }, encode: false),
                                      "Publish.Title".Localize(this), NotificationType.Success));
                return new CustomJsonResult(new
                    {
                        success = true,
                        notifications = Notifications,
                        msg = model.IncludeChildren
                                  ? "Publish.ChildrenSuccess.Message".Localize(this, new { contentViewModel.Name }, encode: false)
                                  : "Publish.SingleSuccess.Message".Localize(this, new { contentViewModel.Name }, encode: false)
                    }.ToJsonString);

            }
        }
    }
}
