using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Templates;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors.Dialogs
{
    [Editor(CorePluginConstants.InsertPartialEditorControllerId)]
    [UmbracoEditor]
    public class InsertPartialEditorController : AbstractEditorController
    {
        private readonly ReadonlyGroupUnitFactory<IFileStore> _partialsStore;

        public InsertPartialEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        {
            _partialsStore = BackOfficeRequestContext
                .Application
                .Hive
                .GetReader<IFileStore>(new Uri("storage://partials"));
        }

        [HttpGet]
        public ActionResult Index(HiveId id)
        {
            var model = new InsertPartialModel { AvailablePartials = Enumerable.Empty<SelectListItem>() };

            using (var uow = _partialsStore.CreateReadonly())
            {
                var rootId = _partialsStore.GetRootNodeId();
                var root = uow.Repositories.Get<File>(rootId);
                var partialIds = uow.Repositories.GetDescendantIds(_partialsStore.GetRootNodeId(), FixedRelationTypes.DefaultRelationType);
                var partials = uow.Repositories.Get<File>(true, partialIds).Where(x => !x.IsContainer);

                var availablePartials = new List<SelectListItem>();
                foreach (var @partial in partials)
                {
                    availablePartials.Add(new SelectListItem{ Text = @partial.GetFilePathForDisplay(), Value = @partial.GetFilePathWithoutExtension()});
                }

                model.AvailablePartials = availablePartials;
            }

            return View(model);
        }
    }
}
