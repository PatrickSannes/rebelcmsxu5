using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Cms.Web.Configuration.Languages;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ViewEngines;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.LanguageEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class LanguageEditorController : StandardEditorController
    {
        public LanguageEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        /// <summary>
        /// Displays the Create language editor 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult Create()
        {
            var model = CreateLanguageEditorModel();

            return View("Edit", model);
        }

        /// <summary>
        /// Creates a new language based on posted values
        /// </summary>
        /// <returns></returns>
        [ActionName("Create")]
        [HttpPost]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [Save]
        public ActionResult CreateForm()
        {
            var model = CreateLanguageEditorModel();

            //TODO: Need to check with Shannon what I need to set path to in RouteData collection to get SuccessfulOnRedirect to work
            return ProcessSubmit(model, null);
        }

        /// <summary>
        /// Displays the edit language form.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            var isoCode = id.Value.Value.ToString();
            var language = BackOfficeRequestContext.Application.Settings.Languages.SingleOrDefault(x => x.IsoCode == isoCode);
            if (language == null)
                throw new ArgumentException(string.Format("No language found for iso code: {0} on action Edit", isoCode));

            var model = CreateLanguageEditorModel(id.Value);

            BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(language, model);

            return View("Edit", model);
        }

        /// <summary>
        /// Updates a language based on posted values.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [ActionName("Edit")]
        [HttpPost]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [PersistTabIndexOnRedirect]
        [Save]
        public ActionResult EditForm(HiveId? id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            var isoCode = id.Value.Value.ToString();
            var language = BackOfficeRequestContext.Application.Settings.Languages.SingleOrDefault(x => x.IsoCode == isoCode);
            if (language == null)
                throw new ArgumentException(string.Format("No language found for iso code: {0} on action Edit", isoCode));

            var model = CreateLanguageEditorModel(id.Value);

            BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(language, model);

            return ProcessSubmit(model, language);
        }

        /// <summary>
        /// JSON action to delete a language
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public virtual JsonResult Delete(HiveId? id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            // Persist the entity
            var configFile = Path.Combine(HttpContext.Server.MapPath("~/App_Data/Umbraco/Config"), "umbraco.cms.languages.config");
            var configXml = XDocument.Load(configFile);

            // Remove entries
            configXml.Descendants("fallback").Where(x => x.Attribute("isoCode").Value == id.Value.Value.ToString()).Remove();
            configXml.Descendants("language").Where(x => x.Attribute("isoCode").Value == id.Value.Value.ToString()).Remove();

            configXml.Save(configFile);

            //return a successful JSON response
            return Json(new { message = "Success" });
        }

        #region Protected/Private methods

        protected LanguageEditorModel CreateLanguageEditorModel(HiveId id = default(HiveId))
        {
            return new LanguageEditorModel
            {
                Id = id,
                InstalledLanguages = BackOfficeRequestContext.Application.Settings.Languages
                    .Where(x => x.IsoCode != id.Value.ToString())
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.IsoCode
                    }).ToList(),
                AvailableLanguages = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                    .OrderBy(x => x.EnglishName)
                    .Select(x => new SelectListItem
                    {
                        Text = x.EnglishName,
                        Value = x.Name
                    }).ToList()
            };
        }

        protected ActionResult ProcessSubmit(LanguageEditorModel model, LanguageElement entity)
        {
            Mandate.ParameterNotNull(model, "model");

            //bind it's data
            model.BindModel(this);

            // Check to see if a language already exists with the given ISO code
            if (model.Id.IsNullValueOrEmpty() || model.Id.Value.ToString() != model.IsoCode)
            {
                if (BackOfficeRequestContext.Application.Settings.Languages.Any(x => x.IsoCode == model.IsoCode))
                {
                    ModelState.AddModelError("DuplicateLanguage", "A language with the ISO code '" + model.IsoCode + "' already exists.");
                }
            }

            //if there's model errors, return the view
            if (!ModelState.IsValid)
            {
                AddValidationErrorsNotification();
                return View("Edit", model);
            }

            // Map the language
            if (entity == null)
            {
                entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<LanguageEditorModel, LanguageElement>(model);
            }
            else
            {
                //map to existing entity
                BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map(model, entity);
            }
            
            // Persist the entity
            var configFile = Path.Combine(HttpContext.Server.MapPath("~/App_Data/Umbraco/Config"), "umbraco.cms.languages.config");
            var configXml = XDocument.Load(configFile);

            // Remove previous entry
            configXml.Descendants("language").Where(x => x.Attribute("isoCode").Value == model.Id.Value.ToString()).Remove();

            // Add new entry
            configXml.Element("languages").Add(XElement.Parse(entity.ToXmlString()));

            //TODO: When updating and name changes, should we reassign any fallbacks linked to old iso code? or orphan them? Or just prevent language from being changed?

            configXml.Save(configFile);

            Notifications.Add(new NotificationMessage(
                   "Language.Save.Message".Localize(this),
                   "Language.Save.Title".Localize(this),
                   NotificationType.Success));
            
            var id = new HiveId(entity.IsoCode);

            //add path for entity for SupportsPathGeneration (tree syncing) to work,
            //we need to manually contruct the path because of the static root node id.
            GeneratePathsForCurrentEntity(new EntityPathCollection(id, new[]{ new EntityPath(new[]
                {
                    new HiveId(FixedSchemaTypes.SystemRoot, null, new HiveIdValue(new Guid(CorePluginConstants.LanguageTreeRootNodeId))), 
                    id
                })
            }));

            return RedirectToAction("Edit", new { id });
        }

        #endregion
    }
}
