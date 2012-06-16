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
    [Editor(CorePluginConstants.ImplementSectionEditorControllerId)]
    [UmbracoEditor]
    public class ImplementSectionEditorController : AbstractEditorController
    {
        private readonly ReadonlyGroupUnitFactory<IFileStore> _templateStore;

        public ImplementSectionEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        {
            _templateStore = BackOfficeRequestContext
                .Application
                .Hive
                .GetReader<IFileStore>(new Uri("storage://templates"));
        }

        [HttpGet]
        public ActionResult Index(HiveId id)
        {
            var model = new ImplementSectionModel { AvailableSections = Enumerable.Empty<string>() };

            using (var uow = _templateStore.CreateReadonly())
            {
                var template = uow.Repositories.Get<File>(id);
                if(template != null)
                {
                    var parser = new TemplateParser();
                    var result = parser.Parse(template);
                    if (!string.IsNullOrWhiteSpace(result.Layout))
                    {
                        var layoutFilePath = TemplateHelper.ResolveLayoutPath(result.Layout, template);
                        var layoutFile = uow.Repositories.GetAll<File>().Where(x => x.RootedPath == layoutFilePath).FirstOrDefault();
                        var layoutResult = parser.Parse(layoutFile);

                        model.AvailableSections = layoutResult.Sections.Where(x => x != "Body").ToArray();
                    }
                }
            }

            return View(model);
        }
    }
}
