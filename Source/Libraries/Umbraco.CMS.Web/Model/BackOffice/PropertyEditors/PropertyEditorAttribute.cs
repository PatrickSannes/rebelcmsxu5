using System;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.PropertyEditors
{

    /// <summary>
    /// Defines a PropertyEditor
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PropertyEditorAttribute : PluginAttribute
    {
        public PropertyEditorAttribute(string id, string alias, string name)
            : base(id)
        {
            Mandate.ParameterNotNullOrEmpty(id, "id");
            Mandate.ParameterNotNullOrEmpty(alias, "alias");
            Mandate.ParameterNotNullOrEmpty(name, "name");

            Alias = alias;
            Name = name;

            IsContentPropertyEditor = true;
        }


        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; set; }

        /// <summary>
        /// Flag determining if this property editor is used to edit content
        /// </summary>
        public bool IsContentPropertyEditor { get; set; }

        /// <summary>
        /// Flag determining if this property editor is used to edit parameters
        /// </summary>
        public bool IsParameterEditor { get; set; }
    }
}
