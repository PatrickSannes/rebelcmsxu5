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
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors.Dialogs
{
    [Editor(CorePluginConstants.InsertFieldEditorControllerId)]
    [UmbracoEditor]
    public class InsertFieldEditorController : AbstractEditorController
    {
        private ReadonlyGroupUnitFactory<IContentStore> _contentStore;

        public InsertFieldEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        {
            _contentStore = BackOfficeRequestContext.Application.Hive.GetReader<IContentStore>();
        }

        [HttpGet]
        public ActionResult Index(HiveId id)
        {
            var model = new InsertFieldModel();

            using (var uow = _contentStore.CreateReadonly())
            {
                var schemaIds = uow.Repositories.Schemas.GetDescendentRelations(FixedHiveIds.ContentRootSchema, FixedRelationTypes.DefaultRelationType)
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();

                var schemas = uow.Repositories.Schemas.Get<EntitySchema>(true, schemaIds);
                var attributeDefs = schemas.SelectMany(x => x.AttributeDefinitions)
                    .DistinctBy(x => x.Alias);
                var attributeAliases = new List<string>();

                // This isn't ideal as we are currently hard coding which property editors
                // have complex data types. This will need to be updated when issue U5-332
                // is addressed.
                foreach (var attributeDef in attributeDefs)
                {
                    switch (attributeDef.AttributeType.RenderTypeProvider)
                    {
                        case CorePluginConstants.NodeNamePropertyEditorId:
                            attributeAliases.Add("Name");
                            attributeAliases.Add("UrlName");
                            break;
                        case CorePluginConstants.SelectedTemplatePropertyEditorId:
                            attributeAliases.Add("CurrentTemplateId");
                            break;
                        case CorePluginConstants.FileUploadPropertyEditorId:
                            attributeAliases.Add(attributeDef.Alias + " - MediaId");
                            attributeAliases.Add(attributeDef.Alias + " - Value");
                            break;
                        default:
                            attributeAliases.Add(attributeDef.Alias);
                            break;
                    }
                }

                model.AvailableFields = attributeAliases
                        .OrderBy(x => x.StartsWith("system-") ? 1 : -1) // Force system fields to the bottom
                        .ThenBy(x => x);

                model.AvailableCasingTypes = Enum.GetNames(typeof(UmbracoRenderItemCaseType))
                    .OrderBy(x => x == "Unchanged" ? -1 : 1);

                model.AvailableEncodingTypes = Enum.GetNames(typeof(UmbracoRenderItemEncodingType))
                    .OrderBy(x => x == "Unchanged" ? -1 : 1);
            }

            return View(model);
        }
    }
}
