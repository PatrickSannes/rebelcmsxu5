namespace Umbraco.Cms.Web.Macros
{
    /// <summary>
    /// Represents the basic definition of a macro which defines the macro type and the macro item/file to render
    /// </summary>    
    public class MacroDefinition
    {          
        /// <summary>
        /// The MacroEngine type/name to execute the macro
        /// </summary>
        public string MacroEngineName { get; set; }

        /// <summary>
        /// Represents the selected file/item to run the macro
        /// </summary>     
        public string SelectedItem { get; set; }

    }
}