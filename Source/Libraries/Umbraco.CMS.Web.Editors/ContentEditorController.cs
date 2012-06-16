using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors.Extenders;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ActionInvokers;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.ContentEditorControllerId, HasChildActionDashboards = true)]
    [UmbracoEditor]
    [ExtendedBy(typeof(HostnameController))]
    [ExtendedBy(typeof(MoveCopyController), AdditionalParameters = new object[] { CorePluginConstants.ContentTreeControllerId })]
    [ExtendedBy(typeof(LanguageController))]
    public class ContentEditorController : AbstractRevisionalContentEditorController<ContentEditorModel>
    {
        
        public ContentEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {

            _hive = BackOfficeRequestContext.Application.Hive.GetWriter<IContentStore>();
            _readonlyHive = BackOfficeRequestContext.Application.Hive.GetReader<IContentStore>();

            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route content://"));
        }

        private readonly GroupUnitFactory _hive;
        private readonly ReadonlyGroupUnitFactory _readonlyHive;

        /// <summary>
        /// Returns the hive provider used for this controller
        /// </summary>
        public override GroupUnitFactory Hive
        {
            get { return _hive; }
        }

        public override ReadonlyGroupUnitFactory ReadonlyHive
        {
            get
            {
                return _readonlyHive;
            }
        }

        /// <summary>
        /// Returns the recycle bin id used for this controller
        /// </summary>
        public override HiveId RecycleBinId
        {
            get { return FixedHiveIds.ContentRecylceBin; }
        }

        /// <summary>
        /// Returns the virtual root node for the content type that this controller is rendering content for
        /// </summary>
        public override HiveId VirtualRootNodeId
        {
            get { return FixedHiveIds.ContentVirtualRoot; }
        }

        /// <summary>
        /// Returns the schema root node for the current type of content
        /// </summary>
        public override HiveId RootSchemaNodeId
        {
            get { return FixedHiveIds.ContentRootSchema; }
        }

        /// <summary>
        /// Returns the message to display when presenting the CreateNew view
        /// </summary>
        public override string CreateNewTitle
        {
            get { return "Create new content"; }
        }

        /// <summary>
        /// Updates the URLs for the entity and updates the noticeboard if there are not templates
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        protected override void OnEditing(ContentEditorModel model, EntitySnapshot<TypedEntity> entity)
        {
            model.UIElements.Add(new SeperatorUIElement());
            model.UIElements.Add(new ButtonUIElement
            {
                Alias = "Preview",
                Title = "Preview",
                CssClass = "preview-button toolbar-button",
                AdditionalData = new Dictionary<string, string>
                {
                    { "id", "submit_Preview" },
                    { "name", "submit.Preview" }
                }
            });

            PopulateUrls(model, entity.Revision.Item);
            
            var allowedTemplates = model.Properties.Single(x => x.Alias == SelectedTemplateAttributeDefinition.AliasValue);
            if ((allowedTemplates.PropertyEditorModel == null || !Enumerable.Any(allowedTemplates.PropertyEditorModel.AvailableTemplates)))
            {
                model.NoticeBoard.Add(new NotificationMessage("Content.NoTemplates.Message".Localize(this), NotificationType.Info));
            }

            //we need to flag if this model is editable based on whether or not it is in the recycle bin
            using (var uow = ReadonlyHive.CreateReadonly<IContentStore>())
            {
                var ancestorIds = uow.Repositories.GetAncestorIds(entity.Revision.Item.Id, FixedRelationTypes.DefaultRelationType);
                if (ancestorIds.Contains(FixedHiveIds.ContentRecylceBin, new HiveIdComparer(true)))
                {
                    model.IsEditable = false;
                }
            }
        }

        /// <summary>
        /// Override to ensure the URLs are populated in case validation fails
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected override ActionResult ProcessSubmit(ContentEditorModel model, Revision<TypedEntity> entity, bool isRevisional)
        {
            if (entity != null)
            {
                PopulateUrls(model, entity.Item);    
            }


            return base.ProcessSubmit(model, entity, isRevisional);
        }
        
        /// <summary>
        /// Adds the document links to the model
        /// </summary>
        /// <param name="contentViewModel"></param>
        /// <param name="source"></param>
        private void PopulateUrls(ContentEditorModel contentViewModel, TypedEntity source)
        {
            var urlReslt = BackOfficeRequestContext.RoutingEngine.GetAllUrlsForEntity(source);
            if (urlReslt.All(x => x.IsSuccess()))
            {
                contentViewModel.DocumentLinks = urlReslt.Select(x => x.Url).ToArray();
            }
            else
            {
                foreach (var e in urlReslt.Where(x => !x.IsSuccess()))
                {
                    switch (e.Status)
                    {
                        case UrlResolutionStatus.FailedRequiresHostname:
                            contentViewModel.NoticeBoard.Add(
                                new NotificationMessage("Content.UrlResolutionFailedHostname.Message".Localize(this), NotificationType.Error));
                            break;
                        case UrlResolutionStatus.FailedNotRoutableFromCurrentHost:
                        case UrlResolutionStatus.FailedNotPublished:
                            contentViewModel.NoticeBoard.Add(
                                new NotificationMessage("Url resolution failed with status: " + e.Status, NotificationType.Error));
                            break;
                    }   
                }                                
            }
        }


    }
}
