using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.TinyMCE.InsertMacro
{
    public class SetParametersModel : IModelBindAware
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is new.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is new; otherwise, <c>false</c>.
        /// </value>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the content id.
        /// </summary>
        /// <value>
        /// The content id.
        /// </value>
        public HiveId ContentId { get; set; }

        /// <summary>
        /// The Id that the client side generates for the inline macro inside of the RTE, this is to determine different macro entries since many entires can exist with the same alias
        /// </summary>
        public string InlineMacroId { get; set; }

        /// <summary>
        /// Gets or sets the macro alias.
        /// </summary>
        /// <value>
        /// The macro alias.
        /// </value>
        public string MacroAlias { get; set; }

        /// <summary>
        /// Gets or sets the macro parameters.
        /// </summary>
        /// <value>
        /// The macro parameters.
        /// </value>
        public IEnumerable<MacroParameterModel> MacroParameters { get; set; }

        /// <summary>
        /// Binds the model with the IUpdator
        /// </summary>
        /// <param name="modelUpdator"></param>
        public void BindModel(IModelUpdator modelUpdator)
        {
            Mandate.ParameterNotNull(modelUpdator, "updator");

            //First, bind the dynamic properties, then the normal properties, this is because we are overriding the 'Name' 
            //property which comes from the NodeName dynamic property
            foreach (var macroParameter in MacroParameters.Where(x => x.ParameterEditorModel != null))
            {
                modelUpdator.BindModel(macroParameter.ParameterEditorModel.PropertyEditorModel, macroParameter.Alias);
            }

            modelUpdator.BindModel(this, string.Empty);
        }
    }
}
