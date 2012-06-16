using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ViewEngines;

using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{

    [Editor(CorePluginConstants.MediaTypeEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    [AlternateViewEnginePath("DocumentTypeEditor")]
    public class MediaTypeEditorController : AbstractSchemaEditorController<MediaTypeEditorModel, CreateContentModel>
    {
        public MediaTypeEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        public override Hive.ProviderGrouping.GroupUnitFactory Hive
        {
            get { return BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("media://")); }
        }

        protected override string CreateNewTitle
        {
            get { return "Create a new media type"; }
        }

        protected override HiveId RootSchema
        {
            get { return FixedHiveIds.MediaRootSchema; }
        }

        protected override MediaTypeEditorModel CreateNewEditorModel(string name = null, Tab genericTab = null)
        {
            var model = new MediaTypeEditorModel();
            if (!name.IsNullOrWhiteSpace())
            {
                model.Alias = name.ToUmbracoAlias();
                model.Name = name;
            }
            if (genericTab != null)
            {
                model.DefinedTabs = new HashSet<Tab>(new[] { genericTab });
                model.AvailableTabs = new SelectList(new[] { genericTab }, "Id", "Name");
            }
            return model;
        }    
    }
}