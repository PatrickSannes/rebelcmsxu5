

namespace Umbraco.Framework.Persistence.Abstractions.Versioning
{
    /// <summary>
    ///   Represents a set of changes
    /// </summary>
    public interface IChangeset
    {
        /// <summary>
        ///   Gets or sets the id of the changeset.
        /// </summary>
        /// <value>The id.</value>
        HiveEntityUri Id { get; set; }

        /// <summary>
        ///   Gets or sets the branch in which this changeset resides.
        /// </summary>
        /// <value>The branch.</value>
        IBranch Branch { get; set; }
    }
}