using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors.Dialogs
{
    [Editor(CorePluginConstants.DefineSectionEditorControllerId)]
    [UmbracoEditor]
    public class DefineSectionEditorController : AbstractEditorController
    {
        public DefineSectionEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        [HttpGet]
        public ActionResult Index(HiveId id)
        {
            return View();
        }
    }
}
