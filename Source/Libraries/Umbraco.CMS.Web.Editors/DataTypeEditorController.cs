using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;

using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.DataTypeEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class DataTypeEditorController : StandardEditorController
    {

        public DataTypeEditorController(
            IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _propertyEditors = requestContext.RegisteredComponents.PropertyEditors;
            //exclude internal ones and only include ones made for editing content
            _sortedEditors = _propertyEditors
                .Where(x => !x.Metadata.IsInternalUmbracoEditor && x.Metadata.IsContentPropertyEditor)
                .Select(x => x.Metadata).OrderBy(x => x.Name).ToArray();
        }

        private readonly IEnumerable<Lazy<PropertyEditor, PropertyEditorMetadata>> _propertyEditors;
        private readonly PropertyEditorMetadata[] _sortedEditors;

        #region Actions

        /// <summary>
        /// Action to render the editor
        /// </summary>
        /// <returns></returns>
        public override ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IContentStore>())
            {
                var dataTypeEntity = uow.Repositories.Schemas.Get<AttributeType>(id.Value);
                if (dataTypeEntity == null)
                    throw new ArgumentException(string.Format("No AttributeType found for id: {0} on action Edit", id));

                var dataTypeViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<AttributeType, DataTypeEditorModel>(dataTypeEntity);

                EnsurePropEditorListViewBagData();

                return View(dataTypeViewModel);
            }                  
        }

        /// <summary>
        /// Action to handle the posted contents of the editor
        /// </summary>
        /// <returns></returns>
        [ActionName("Edit")]
        [HttpPost]
        [Save]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [PersistTabIndexOnRedirect]
        public ActionResult EditForm(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();
            
            using (var uow = BackOfficeRequestContext.Application.Hive.OpenWriter<IContentStore>())
            {
                var dataTypeEntity = uow.Repositories.Schemas.Get<AttributeType>(id.Value);
                if (dataTypeEntity == null)
                    throw new ArgumentException(string.Format("No AttributeType found for id: {0} on action EditForm", id));

                var dataTypeViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<AttributeType, DataTypeEditorModel>(dataTypeEntity);

                return ProcessSubmit(dataTypeViewModel, dataTypeEntity);
            }           
        }


        /// <summary>
        /// Renders the create new document type wizard step
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult CreateNew()
        {
            //lookup the doc type for the node id, find out which doc type children are allowed
            EnsurePropEditorListViewBagData();

            return View(new CreateDataTypeModel());
        }

        /// <summary>
        /// Handles the post back for the create wizard step
        /// </summary>
        /// <param name="createModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("CreateNew")]
        [Save]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        public virtual ActionResult CreateNewForm(CreateDataTypeModel createModel)
        {

            if (!TryValidateModel(createModel))
            {
                EnsurePropEditorListViewBagData();
                return View(createModel);
            }

            var propertyEditor = _propertyEditors.Where(x => x.Metadata.Id == createModel.PropertyEditorId).Single();
            var dataType = new DataType(string.Empty, string.Empty, propertyEditor.Value);
            var dataTypeEditorViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<DataType, DataTypeEditorModel>(dataType);

            return ProcessSubmit(dataTypeEditorViewModel, null);

            return RedirectToAction("Create", new { propertyEditorId = createModel.PropertyEditorId, name = createModel.Name });
        }

        [HttpGet]
        public virtual ActionResult Create(Guid? propertyEditorId, string name)
        {
            if (!propertyEditorId.HasValue || propertyEditorId == default(Guid)) return HttpNotFound();
            if (name == null) return HttpNotFound();

            var propertyEditor = _propertyEditors.Where(x => x.Metadata.Id == propertyEditorId.Value).Single();

            var dt = new DataType(name, name.ToUmbracoAlias() ,propertyEditor.Value);

            var model = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<DataType, DataTypeEditorModel>(dt);
            EnsurePropEditorListViewBagData();
            return View("Edit", model);
        }

        [ActionName("Create")]
        [HttpPost]
        [Save]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        public virtual ActionResult CreateForm()
        {
            //get the property editor Id from the values
            var propertyEditorId = Guid.Parse(ValueProvider.GetValue("PropertyEditorId").AttemptedValue);
            //this will throw an exception if its not found
            var propertyEditor = _propertyEditors.Where(x => x.Metadata.Id == propertyEditorId).Single();
            var dataType = new DataType(string.Empty, string.Empty, propertyEditor.Value);
            var dataTypeEditorViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<DataType, DataTypeEditorModel>(dataType);

            return ProcessSubmit(dataTypeEditorViewModel, null);
        }

        /// <summary>
        /// JSON action to delete a node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public JsonResult Delete(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenWriter<IContentStore>())
            {
                uow.Repositories.Schemas.Delete<AttributeType>(id);
                uow.Complete();
            }
            
            //return a successful JSON response

            Notifications.Add(new NotificationMessage("Delete.Successful".Localize(this), NotificationType.Success));            
            var obj = new { message = "Success", notifications = Notifications };
            return new CustomJsonResult(() => obj.ToJsonString());
        } 
        
        #endregion

        #region Protected/Private methods

        /// <summary>
        /// Processes the submit for insert/update
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entity"></param>
        protected ActionResult ProcessSubmit(DataTypeEditorModel model, AttributeType entity)
        {
            Mandate.ParameterNotNull(model, "model");

            model.BindModel(this);

            model.Id = entity != null ? entity.Id : HiveId.Empty;

            if (!ModelState.IsValid)
            {
                AddValidationErrorsNotification();
                EnsurePropEditorListViewBagData();
                return View(model);
            }

            //persist the data
            using (var uow = BackOfficeRequestContext.Application.Hive.OpenWriter<IContentStore>())
            {
                if (entity == null)
                {
                    //map to new entity
                    entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<DataTypeEditorModel, AttributeType>(model);
                }
                else
                {
                    //map back to existing entity from updated model
                    BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(model, entity);    
                }

                uow.Repositories.Schemas.AddOrUpdate(entity);
                uow.Complete();

                Notifications.Add(new NotificationMessage(
                        "DataType.Save.Message".Localize(this),
                        "DataType.Save.Title".Localize(this),
                        NotificationType.Success));
                
                //add path for entity for SupportsPathGeneration (tree syncing) to work,
                //we need to manually contruct the path because of the static root node id.
                GeneratePathsForCurrentEntity(new EntityPathCollection(entity.Id, new[]{ new EntityPath(new[]
                    {
                        new HiveId(FixedSchemaTypes.SystemRoot, null, new HiveIdValue(new Guid(CorePluginConstants.DataTypeTreeRootNodeId))), 
                        entity.Id
                    })
                }));
                
                return RedirectToAction("Edit", new { id = entity.Id });
            }
            
        }

        /// <summary>
        /// This adds some required elements to the ViewBag so that the Create view renders correctly
        /// </summary>
        private void EnsurePropEditorListViewBagData()
        {
            //add the available doc types to the view bag
            ViewBag.AvailablePropertyEditors = _sortedEditors;
        } 

        #endregion

    }
}
