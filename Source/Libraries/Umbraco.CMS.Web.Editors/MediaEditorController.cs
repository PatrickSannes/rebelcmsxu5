using System;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors.Extenders;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ActionInvokers;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Mvc.ViewEngines;

using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    //[Editor(CorePluginConstants.MediaEditorControllerId)]
    //[UmbracoEditor]
    //public class MemberEditorController : AbstractContentEditorController
    //{

    //}

    [Editor(CorePluginConstants.MediaEditorControllerId)]
    [UmbracoEditor]        
    [AlternateViewEnginePath("ContentEditor")]
    [ExtendedBy(typeof(MoveCopyController), AdditionalParameters = new object[] { CorePluginConstants.MediaTreeControllerId })]
    public class MediaEditorController : AbstractRevisionalContentEditorController<MediaEditorModel>
    {

        public MediaEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("media://"));
            _readonlyHive = BackOfficeRequestContext.Application.Hive.GetReader<IContentStore>();

            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route media://"));
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
            get { return FixedHiveIds.MediaRecylceBin; }
        }

        /// <summary>
        /// Return the media root as the virtual root node
        /// </summary>
        public override HiveId VirtualRootNodeId
        {
            get { return FixedHiveIds.MediaVirtualRoot; }
        }

        /// <summary>
        /// Returns the media virtual root
        /// </summary>
        public override HiveId RootSchemaNodeId
        {
            get { return FixedHiveIds.MediaRootSchema; }
        }

        public override string CreateNewTitle
        {
            get { return "Create new media"; }
        }

        protected override void OnEditing(MediaEditorModel model, EntitySnapshot<TypedEntity> entity)
        {
            //we need to flag if this model is editable based on whether or not it is in the recycle bin
            using (var uow = Hive.Create<IContentStore>())
            {
                var ancestorIds = uow.Repositories.GetAncestorIds(entity.Revision.Item.Id, FixedRelationTypes.DefaultRelationType);
                if (ancestorIds.Contains(FixedHiveIds.MediaRecylceBin, new HiveIdComparer(true)))
                {
                    model.IsEditable = false;
                }
            }
        }

    }

}
