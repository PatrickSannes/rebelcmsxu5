using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model
{
    public class Field : NodeWithAlias
    {

        public Field(FieldDefinition definition)
        {
            Definition = definition;
        }

        /// <summary>
        /// Gets or sets the alias of the object. The alias is a string to which this object
        /// can be referred programmatically, and is often a normalised version of the <see cref="IReferenceByName.Name"/> property.
        /// </summary>
        /// <value>The alias.</value>
        /// <remarks></remarks>
        public override string Alias
        {
            get { return Definition.Alias; }
            set { return; }
        }

        /// <summary>
        /// Gets or sets the name of the object. The name is a string intended to be human-readable, and
        /// is often a more descriptive version of the <see cref="IReferenceByName.Alias"/> property.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks></remarks>
        public override LocalizedString Name
        {
            get { return Definition.Name; }
            set { return; }
        }

        /// <summary>
        /// Gets or sets the definition for the field.
        /// </summary>
        /// <value>The definition.</value>
        /// <remarks></remarks>
        public FieldDefinition Definition { get; protected set; }

        /// <summary>
        /// Gets or sets the values of this field.
        /// </summary>
        /// <value>The values.</value>
        /// <remarks></remarks>
        public IEnumerable<KeyedFieldValue> Values { get; set; }

        public override int GetHashCode()
        {
            // Generate hashcode by Xoring Alias and Id hashcodes, but check for nulls
            int aliasHash = (string.IsNullOrEmpty(Alias)) ? 0 : Alias.GetHashCode();
            int idHash = (Id.IsNullValueOrEmpty()) ? 0 : Id.GetHashCode();
            int hash = aliasHash ^ idHash;
            // Guard against the hash Xor being 0
            return (hash == 0) ? base.GetHashCode() : hash;
        }
    }
}