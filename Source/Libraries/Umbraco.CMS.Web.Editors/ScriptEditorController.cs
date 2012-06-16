using System;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;

using Umbraco.Framework.Localization;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.ScriptEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class ScriptEditorController : AbstractFileEditorController
    {
        private readonly GroupUnitFactory<IFileStore> _hive;

        public ScriptEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext
                .Application
                .Hive
                .GetWriter<IFileStore>(new Uri("storage://scripts"));
        }

        public override GroupUnitFactory<IFileStore> Hive
        {
            get
            {
                return _hive;
            }
        }

        protected override string SaveSuccessfulTitle
        {
            get { return "Script.Save.Title".Localize(this); }
        }

        protected override string SaveSuccessfulMessage
        {
            get { return "Script.Save.Message".Localize(this); }
        }

        protected override string CreateNewTitle
        {
            get { return "Create a script"; }
        }

        protected override string[] AllowedFileExtensions
        {
            get { return new[] {".js"}; }
        }
    }
}
