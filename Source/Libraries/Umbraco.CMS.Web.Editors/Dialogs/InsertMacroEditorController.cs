using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.TinyMCE.InsertMacro;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors.Dialogs
{
    [Editor(CorePluginConstants.InsertMacroEditorControllerId)]
    [UmbracoEditor]
    public class InsertMacroEditorController : AbstractEditorController, IModelUpdator
    {
         public InsertMacroEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        public ActionResult Index(HiveId id)
        {
            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://macros")))
            {
                var macros = uow.Repositories.GetAll<File>()
                    .Select(MacroSerializer.FromFile)
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Alias });

                var model = new SelectMacroModel
                {
                    AvailableMacroItems = macros
                };

                return View(model);
            }
        }

        public ActionResult SetParameters(string macroAlias)
        {
            var viewModel = new SetParametersModel();

            var macroEditorModel = GetMacroByAlias(macroAlias);

            if(macroEditorModel.MacroParameters.Count == 0)
            {
                return RedirectToAction("InsertMacro", new { macroAlias });
            }

            BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map
                <MacroEditorModel, SetParametersModel>(macroEditorModel, viewModel);

            return View(viewModel);
        }

        public ActionResult InsertMacro(string macroAlias)
        {
            // Create the view model
            var setParamsViewModel = new SetParametersModel();

            // Populate view model with default content from macro definition
            var macroEditorModel = GetMacroByAlias(macroAlias);
            BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map
                <MacroEditorModel, SetParametersModel>(macroEditorModel, setParamsViewModel);

            // Bind the post data back to the view model
            setParamsViewModel.BindModel(this);

            // Convert model
            var insertMacroViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map
                <SetParametersModel, InsertMacroModel>(setParamsViewModel);

            return View(insertMacroViewModel);
        }

        /// <summary>
        /// Gets a macro by alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        private MacroEditorModel GetMacroByAlias(string alias)
        {
            using (var uow = BackOfficeRequestContext
                .Application
                .Hive.OpenReader<IFileStore>(new Uri("storage://macros")))
            {
                var filename = alias + ".macro";
                var macroFile = uow.Repositories.Get<File>(new HiveId(filename));
                if (macroFile == null)
                    throw new ApplicationException("Could not find a macro with the specified alias: " + alias);

                return MacroSerializer.FromXml(Encoding.UTF8.GetString(macroFile.ContentBytes));
            }
        }

        /// <summary>
        /// Binds the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="fieldPrefix">The field prefix.</param>
        /// <returns></returns>
        public bool BindModel(dynamic model, string fieldPrefix)
        {
            Mandate.ParameterNotNull(model, "model");
            if (string.IsNullOrEmpty(fieldPrefix))
            {
                return TryUpdateModel(model, ValueProvider);
            }
            return TryUpdateModel(model, fieldPrefix, new string[] { }, new string[] { }, ValueProvider);
        }
    }
}
