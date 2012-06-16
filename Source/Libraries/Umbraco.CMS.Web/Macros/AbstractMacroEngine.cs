using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework.Dynamics;

namespace Umbraco.Cms.Web.Macros
{
    public class MacroParameter
    {
        public string ParameterName { get; set; }

        public Guid ParameterEditorId { get; set; }
    }

    /// <summary>
    /// Abstract class representing a MacroEngine
    /// </summary>
    public abstract class AbstractMacroEngine
    {
        protected AbstractMacroEngine()
        {
            //Locate the editor attribute
            var macroEngineAttributes = GetType()
                .GetCustomAttributes(typeof(MacroEngineAttribute), false)
                .OfType<MacroEngineAttribute>();

            if (!macroEngineAttributes.Any())
            {
                throw new InvalidOperationException("MacroEngine is missing the " + typeof(EditorAttribute).FullName + " attribute");
            }

            //assign the properties of this object to those of the metadata attribute
            var attr = macroEngineAttributes.First();
            Name = attr.EngineName;     
        }

        /// <summary>
        /// Gets the name of the Macro Engine
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Executes the macro engine to render the macro
        /// </summary>
        /// <returns></returns>
        public abstract ActionResult Execute(Content currentNode, IDictionary<string, string> macroParams, MacroDefinition macro, ControllerContext currentControllerContext, IRoutableRequestContext routableRequestContext);

        /// <summary>
        /// Gets a list of Macro parameters for the definition
        /// </summary>
        /// <param name="backOfficeRequestContext"></param>
        /// <param name="macroDefinition"></param>
        /// <returns></returns>
        public abstract IEnumerable<MacroParameter> GetMacroParameters(IBackOfficeRequestContext backOfficeRequestContext, MacroDefinition macroDefinition);

        /// <summary>
        /// Returns the macro items that can be used by this Engine
        /// </summary>
        /// <param name="backOfficeRequestContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// Each MacroEngine may render different items, such as PartialViews or ChildActions or Xslt files, Python files, etc...
        /// The Value set for each of the select list items will be the value passed into the MacroEngine's Execute method as the SelectedItem
        /// property of the MacroDefinition.
        /// </remarks>
        public abstract IEnumerable<SelectListItem> GetMacroItems(IBackOfficeRequestContext backOfficeRequestContext);

    }
}