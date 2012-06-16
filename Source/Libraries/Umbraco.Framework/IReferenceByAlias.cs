namespace Umbraco.Framework
{
    public interface IReferenceByAlias
    {
        /// <summary>
        /// Gets or sets the alias of the object. The alias is a string to which this object
        /// can be referred programmatically, and is often a normalised version of the <see cref="Name"/> property.
        /// </summary>
        /// <value>The alias.</value>
        string Alias { get; set; }
    }
}