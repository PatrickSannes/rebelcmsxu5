using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors.Extenders;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionInvokers;
using Umbraco.Cms.Web.Mvc.ViewEngines;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Cms.Web.Editors
{
    using Umbraco.Hive.RepositoryTypes;

    [Editor(CorePluginConstants.DictionaryEditorControllerId)]
    [UmbracoEditor]
    [AlternateViewEnginePath("ContentEditor")]
    [ExtendedBy(typeof(MoveCopyController), AdditionalParameters = new object[] { CorePluginConstants.DictionaryTreeControllerId })]
    public class DictionaryEditorController : AbstractRevisionalContentEditorController<DictionaryItemEditorModel>
    {
        private readonly GroupUnitFactory _hive;
        private readonly ReadonlyGroupUnitFactory _readonlyHive;

        public DictionaryEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("dictionary://"));
            _readonlyHive = BackOfficeRequestContext.Application.Hive.GetReader<IContentStore>();

            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route dictionary://"));
        }

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

        public override HiveId VirtualRootNodeId
        {
            get { return FixedHiveIds.DictionaryVirtualRoot; }
        }

        public override HiveId RootSchemaNodeId
        {
            get { return FixedHiveIds.DictionaryRootSchema; }
        }

        public override HiveId RecycleBinId
        {
            get { return HiveId.Empty; }
        }

        public override string CreateNewTitle
        {
            get { return "Create new Dictionary Item"; }
        }
    }
}
