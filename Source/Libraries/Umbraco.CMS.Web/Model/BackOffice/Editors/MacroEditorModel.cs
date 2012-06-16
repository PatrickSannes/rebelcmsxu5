using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// The model representing a macro
    /// </summary>
    [Bind(Exclude = "AvailableMacroTypes,AvailableMacroItems,AvailableParameterEditors")]
    public class MacroEditorModel : IHasUIElements
    {
        public MacroEditorModel()
        {
            MacroParameters = new List<MacroParameterDefinitionModel>();
        }
        
        /// <summary>
        /// A list of SelectLists containing all available macro items for each macro engine
        /// </summary>
        /// <remarks>
        /// The Tuple contains the MacroEngineName and its available renderable items
        /// </remarks>
        [ReadOnly(true)]
        public IEnumerable<Tuple<string, IEnumerable<SelectListItem>>> AvailableMacroItems { get; set; }

        [ReadOnly(true)]
        public IEnumerable<ParameterEditorMetadata> AvailableParameterEditors { get; set; }

        [Required]
        public string MacroType { get; set; }

        /// <summary>
        /// Represents the selected file/item to run the macro
        /// </summary>
        [Required]
        public string SelectedItem { get; set; }

        /// <summary>
        /// The Id of the macro which equates to it's file name
        /// </summary>
        public HiveId Id { get; set; }

        /// <summary>
        /// The list of macro parameters
        /// </summary>
        public IList<MacroParameterDefinitionModel> MacroParameters { get; set; }

        [Required]
        public string Name { get; set; }

        /// <summary>
        /// The alias is determined by the name and is used as the file name
        /// </summary>
        [Required]
        public string Alias { get; set; }

        /// <summary>
        /// The tab to display
        /// </summary>
        [HiddenInput]
        public int ActiveTabIndex { get; set; }

        public bool UseInEditor { get; set; }

        public bool RenderContentInEditor { get; set; }

        public int CachePeriodSeconds { get; set; }

        public bool CacheByPage { get; set; }

        public bool CachePersonalized { get; set; }

        public IList<UIElement> UIElements
        {
            get { return new List<UIElement> { new SaveButtonUIElement() }; }
        }
    }
}