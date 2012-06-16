using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.TinyMCE.InsertMacro;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Resources.InsertMacro.SelectMacro.js", "application/x-javascript")]
[assembly: WebResource("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Resources.InsertMacro.InsertMacro.js", "application/x-javascript")]

namespace Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Controllers
{
    [Editor("B54BAEC7-C838-4162-BB72-C3C3CA074CE5")]
    [UmbracoEditor]
    public class InsertMacroController : AbstractEditorController, IModelUpdator
    {
        public InsertMacroController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        #region Actions

        /// <summary>
        /// Inserts the macro.
        /// </summary>
        /// <param name="contentId">The current node id.</param>
        /// <param name="isNew">if set to <c>true</c> [is new].</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SelectMacro(HiveId contentId, bool isNew)
        {
            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://macros")))
            {
                var macros = uow.Repositories.GetAll<File>()
                    .Select(MacroSerializer.FromFile).Where(x => x.UseInEditor)
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Alias });

                var model = new Umbraco.Cms.Web.Model.BackOffice.TinyMCE.InsertMacro.SelectMacroModel
                {
                    ContentId = contentId,
                    IsNew = isNew,
                    AvailableMacroItems = macros
                };

                return View(
                    EmbeddedViewPath.Create("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMacro.SelectMacro.cshtml, Umbraco.Cms.Web.PropertyEditors"),
                    model);
            }
        }

        /// <summary>
        /// Inserts the macro.
        /// </summary>
        /// <param name="contentId">The current node id.</param>
        /// <param name="macroAlias">The macro alias.</param>
        /// <param name="isNew">if set to <c>true</c> is new.</param>
        /// <param name="inlineMacroId">The inline macro id.</param>
        /// <param name="macroParameters">The macro parameters.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SetParameters(HiveId contentId, string macroAlias, bool isNew, string inlineMacroId = "", string macroParameters = "")
        {
            var viewModel = new SetParametersModel
            {
                ContentId = contentId,
                IsNew = isNew,
                InlineMacroId = inlineMacroId
            };

            var macroEditorModel = GetMacroByAlias(macroAlias);

            BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map
                <MacroEditorModel, SetParametersModel>(macroEditorModel, viewModel);

            if (!string.IsNullOrEmpty(macroParameters))
            {
                var macroParamsDict = macroParameters.DecodeMacroParameters();

                foreach (var macroParam in viewModel.MacroParameters.Where(macroParam => macroParamsDict.ContainsKey(macroParam.Alias)))
                {
                    macroParam.ParameterEditorModel.SetModelValue(macroParamsDict[macroParam.Alias]);
                }
            }

            //return View(EmbeddedViewEngine.EmbeddedViewPath.Create("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Views.InsertMacroDialog.cshtml", "Umbraco.Cms.Web.PropertyEditors"));
            return View(EmbeddedViewPath.Create("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMacro.SetParameters.cshtml, Umbraco.Cms.Web.PropertyEditors"), viewModel);
        }

        /// <summary>
        /// Inserts the macro.
        /// </summary>
        /// <param name="contentId">The content id.</param>
        /// <param name="macroAlias">The macro alias.</param>
        /// <param name="isNew">if set to <c>true</c> is new.</param>
        /// <param name="inlineMacroId">The inline macro id.</param>
        /// <returns></returns>
        [ActionName("SetParameters")]
        [HttpPost]
        public ActionResult SetParametersForm(HiveId contentId, string macroAlias, bool isNew, string inlineMacroId = "")
        {
            // Create the view model
            var setParamsViewModel = new SetParametersModel
            {
                ContentId = contentId,
                IsNew = isNew,
                InlineMacroId = inlineMacroId
            };

            // Populate view model with default content from macro definition
            var macroEditorModel = GetMacroByAlias(macroAlias);
            BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map
                <MacroEditorModel, SetParametersModel>(macroEditorModel, setParamsViewModel);

            // Bind the post data back to the view model
            setParamsViewModel.BindModel(this);

            // Convert model
            var insertMacroViewModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map
                <SetParametersModel, InsertMacroModel>(setParamsViewModel);

            return View(EmbeddedViewPath.Create("Umbraco.Cms.Web.PropertyEditors.RichTextBox.TinyMCE.Views.InsertMacro.InsertMacro.cshtml, Umbraco.Cms.Web.PropertyEditors"), insertMacroViewModel);
        }

        #endregion

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
