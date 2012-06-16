

namespace Umbraco.Framework.Persistence.Abstractions.Versioning
{
    /// <summary>
    ///   Represents the revision of an object
    /// </summary>
    public interface IRevisionData
    {
        /// <summary>
        ///   Gets or sets the revision id.
        /// </summary>
        /// <value>The revision id.</value>
        HiveEntityUri RevisionId { get; set; }

        /// <summary>
        ///   Gets or sets the changeset.
        /// </summary>
        /// <value>The changeset.</value>
        IChangeset Changeset { get; set; }
    }
}