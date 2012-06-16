using System;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.ParameterEditors
{
    /// <summary>
    /// Defines a ParameterEditor
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ParameterEditorAttribute : PluginAttribute
    {
        public ParameterEditorAttribute(string id, string alias, string name, string propertyEditorId)
            : base(id)
        {
            Mandate.ParameterNotNullOrEmpty(id, "id");
            Mandate.ParameterNotNullOrEmpty(alias, "alias");
            Mandate.ParameterNotNullOrEmpty(name, "name");

            Alias = alias;
            Name = name;
            PropertyEditorId = Guid.Parse(propertyEditorId);
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
        public Guid PropertyEditorId { get; set; }
    }
}
