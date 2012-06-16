using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.TinyMCE.InsertMacro
{
    public class InsertMacroModel
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
        public IDictionary<string, object> MacroParameters { get; set; }
    }
}
