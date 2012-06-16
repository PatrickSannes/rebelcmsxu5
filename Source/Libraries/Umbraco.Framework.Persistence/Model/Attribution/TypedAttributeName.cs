using System;

using Umbraco.Foundation;

namespace Umbraco.Framework.Persistence.Model.Attribution
{
    public class TypedAttributeName : IReferenceByAlias, IComparable<TypedAttributeName>
    {
        #region Implementation of IReferenceByAlias

        /// <summary>
        /// Gets or sets the alias of the object. The alias is a string to which this object
        /// can be referred programmatically, and is often a normalised version of the <see cref="IReferenceByAlias.Name"/> property.
        /// </summary>
        /// <value>The alias.</value>
        public virtual string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the object. The name is a string intended to be human-readable, and
        /// is often a more descriptive version of the <see cref="IReferenceByAlias.Alias"/> property.
        /// </summary>
        /// <value>The name.</value>
        public virtual LocalizedString Name { get; set; }

        #endregion

        #region Implementation of IComparable<in ITypedAttributeName>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(TypedAttributeName other)
        {
            Mandate.ParameterNotNull(other, "other");
            return string.Compare(other.Alias, Alias, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}