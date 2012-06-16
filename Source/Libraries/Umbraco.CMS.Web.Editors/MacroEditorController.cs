using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Framework;


using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using File = Umbraco.Framework.Persistence.Model.IO.File;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.MacroEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class MacroEditorController : StandardEditorController
    {
        private readonly GroupUnitFactory<IFileStore> _hive;

        public MacroEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext
                .Application
                .Hive
                .GetWriter<IFileStore>(new Uri("storage://macros"));
        }

        public GroupUnitFactory<IFileStore> Hive
        {
            get { return _hive; }
        }

        public override ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = Hive.Create())
            {
                var macroFile = uow.Repositories.Get<File>(id.Value);
                if (macroFile != null)
                {
                    var model = MacroSerializer.FromFile(macroFile);
                    EnsureModelListData(model);                    

                    return View(model);
                }
            }

            return HttpNotFound();
        }

        [HttpPost]
        [Save]
        [ActionName("Edit")]
        [PersistTabIndexOnRedirect]
        [SupportsPathGeneration]
        public ActionResult EditForm(MacroEditorModel model)
        {
            Mandate.ParameterNotNull(model, "model");

            //validate the model
            TryUpdateModel(model);

            model.Alias = model.Alias.ToUmbracoAlias();

            //if at this point the model state is invalid, return the result which is the CreateNew view
            if (!ModelState.IsValid)
            {
                EnsureModelListData(model);
                return View("Edit", model);
            }

            return ProcessSubmit(model);
        }

        [HttpGet]
        public ActionResult CreateNew()
        {
            var model = new CreateMacroModel
                {
                    AvailableMacroTypes = new SelectList(BackOfficeRequestContext.RegisteredComponents.MacroEngines.Select(x => x.Metadata.EngineName))
                };
            return View(model);
        }

        [ActionName("CreateNew")]
        [HttpPost]
        [Save]
        [PersistTabIndexOnRedirect]
        [SupportsPathGeneration]
        public ActionResult CreateNewForm(CreateMacroModel createModel)
        {
            Mandate.ParameterNotNull(createModel, "createModel");            
            Mandate.That<NullReferenceException>(!string.IsNullOrEmpty(createModel.Name));

            //validate the model
            TryUpdateModel(createModel);
            //if at this point the model state is invalid, return the result which is the CreateNew view
            if (!ModelState.IsValid)
            {
                return View(createModel);
            }

            //everything is valid, now we need to render out the editor for this macro
            var editorModel = new MacroEditorModel
            {
                Name = createModel.Name,
                MacroType = createModel.MacroType,
                Alias = createModel.Name.ToUmbracoAlias(),
                SelectedItem = ""
            };

            EnsureModelListData(editorModel);


            return ProcessSubmit(editorModel);
        }

        /// <summary>
        /// JSON action to delete a node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public virtual JsonResult Delete(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = Hive.Create())
            {
                uow.Repositories.Delete<File>(id);
                uow.Complete();
            }

            //return a successful JSON response
            return Json(new { message = "Success" });
        }

        [HttpPost]
        public JsonResult PopulateMacroParameters(MacroDefinition macroDefinition)
        {
            var engine = this.BackOfficeRequestContext.RegisteredComponents.MacroEngines.SingleOrDefault(x => x.Metadata.EngineName.InvariantEquals(macroDefinition.MacroEngineName));
            if (engine == null)
            {
                throw new InvalidOperationException("Could not find a MacroEngine registered with the name: " + macroDefinition.MacroEngineName);
            }

            try
            {
                //get the parameters from the engine's response
                var macroParams = engine.Value.GetMacroParameters(this.BackOfficeRequestContext, macroDefinition);
                return JsonParametersSuccess(macroParams);
            }
            catch (Exception ex)
            {
                return JsonParametersException(ex);
            }

        }

        private JsonResult JsonParametersSuccess(IEnumerable<MacroParameter> parameters)
        {
            Notifications.Add(new NotificationMessage(
                    "Macro.ParamsPopulatedSuccess.Message".Localize(this),
                    "Macro.ParamsPopulated.Title".Localize(this), NotificationType.Success));
            return new CustomJsonResult(new
            {
                success = true, 
                parameters,
                notifications = Notifications
            }.ToJsonString);
        }

        /// <summary>
        /// Returns the Json notifications when an exception occurs
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private JsonResult JsonParametersException(Exception ex)
        {
            Notifications.Add(new NotificationMessage(
                        ex.Message,
                        "Macro.ParamsPopulated.Title".Localize(this),
                        NotificationType.Error));
            return new CustomJsonResult(new
            {
                success = false,
                notifications = Notifications
            }.ToJsonString);
        }

        internal File PerformSave(MacroEditorModel model)
        {
            Mandate.ParameterNotNull(model, "model");

            using (var uow = Hive.Create())
            {
                var fileName = model.Alias.ToUmbracoAlias() + ".macro";
                var newFileId = new HiveId(fileName);
                var macroFile = new File
                {
                    Name = fileName,
                    ContentBytes = Encoding.UTF8.GetBytes(MacroSerializer.ToXml(model).ToString())
                };
                var existing = uow.Repositories.Get<File>(newFileId);
                //delete the file first before re-saving as AddOrUpdate seems to append to the file
                if (existing != null)
                    uow.Repositories.Delete<File>(newFileId);
                uow.Repositories.AddOrUpdate(macroFile);
                //TODO: the Hive IO provider commit seems to do nothing :( ... needs to be implemented!
                uow.Complete();

                return macroFile;
            }
        }

        private ActionResult ProcessSubmit(MacroEditorModel model)
        {
            Mandate.ParameterNotNull(model, "model");

            var macroFile = PerformSave(model);

            //set the notification
            Notifications.Add(new NotificationMessage(
                "Macro.Save.Message".Localize(this),
                "Macro.Save.Title".Localize(this),
                NotificationType.Success));

            //add path for entity for SupportsPathGeneration (tree syncing) to work     
            using (var uow = Hive.Create())
            {
                GeneratePathsForCurrentEntity(uow.Repositories.GetEntityPaths<File>(macroFile.Id, FixedRelationTypes.DefaultRelationType));
            }

            return RedirectToAction("Edit", new { id = macroFile.Id });
        }

        /// <summary>
        /// Ensure the ViewBag has all of its required list data
        /// </summary>
        private void EnsureModelListData(MacroEditorModel model)
        {
            model.AvailableParameterEditors = GetMacroParameterEditors();
            model.AvailableMacroItems = BackOfficeRequestContext.RegisteredComponents
                .MacroEngines
                .Select(x => new Tuple<string, IEnumerable<SelectListItem>>(
                                 x.Metadata.EngineName,
                                 x.Value.GetMacroItems(BackOfficeRequestContext)));
        }

        /// <summary>
        /// Returns all of the registered macro parameter editors
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ParameterEditorMetadata> GetMacroParameterEditors()
        {
            return BackOfficeRequestContext.RegisteredComponents.ParameterEditors
                .Select(x => x.Metadata);
        }

    }
}