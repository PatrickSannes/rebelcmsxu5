using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.SelectedTemplate
{

    /// <summary>
    /// Represents the Selected Template editor for a content item
    /// </summary>
    [UmbracoPropertyEditor]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [PropertyEditor(CorePluginConstants.SelectedTemplatePropertyEditorId, "SelectedTemplateEditor", "Selected Template Editor")]
    public class SelectedTemplateEditor : ContentAwarePropertyEditor<SelectedTemplateEditorModel>
    {
        
        private readonly IBackOfficeRequestContext _backOfficeRequestContext;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedTemplateEditor"/> class.
        /// </summary>
        /// <param name="backOfficeRequestContext">The back office request context.</param>
        public SelectedTemplateEditor(IBackOfficeRequestContext backOfficeRequestContext)
        {
            Mandate.ParameterNotNull(backOfficeRequestContext, "backOfficeRequestContext");
            _backOfficeRequestContext = backOfficeRequestContext;
        }

        /// <summary>
        /// Creates the editor model.
        /// </summary>
        /// <returns></returns>
        public override SelectedTemplateEditorModel CreateEditorModel()
        { 
            var model = new SelectedTemplateEditorModel();
            var availableTemplates = new List<SelectListItem>();
            availableTemplates.Add(new SelectListItem { Text = "No template", Value = HiveId.Empty.ToString() });

            if (IsDocumentTypeAvailable)
            {
                using (var uow = _backOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://templates")))
                {
                    //now fill in the select list item
                    var alltemplates = uow.Repositories.GetAllNonContainerFiles();

                    //get the allowed template ids from the currently rendering content item
                    var allowedTemplateIds = GetDocumentTypeValue(x => x.AllowedTemplateIds, new HashSet<HiveId>());

                    availableTemplates.AddRange((from t in allowedTemplateIds
                                                 select alltemplates.Where(x => x.Id.EqualsIgnoringProviderId(t)).SingleOrDefault()
                                                     into template
                                                     where template != null
                                                     select new SelectListItem
                                                     {
                                                         Text = template.GetFileNameForDisplay(),
                                                         Value = template.Id.ToString(),
                                                         Selected = template.Id == GetDocumentTypeValue(x => x.DefaultTemplateId, HiveId.Empty)
                                                     }));
                }

                
            }

            model.AvailableTemplates = availableTemplates;

            return model;
        }
        
    }
}
