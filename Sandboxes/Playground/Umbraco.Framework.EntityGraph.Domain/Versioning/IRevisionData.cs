using System.Diagnostics.Contracts;

namespace Umbraco.Framework.EntityGraph.Domain.Versioning
{
    /// <summary>
    ///   Represents the revision of an object
    /// </summary>
    [ContractClass(typeof (IRevisionDataCodeContract))]
    public interface IRevisionData
    {
        /// <summary>
        ///   Gets or sets the revision id.
        /// </summary>
        /// <value>The revision id.</value>
        IMappedIdentifier RevisionId { get; set; }

        /// <summary>
        ///   Gets or sets the changeset.
        /// </summary>
        /// <value>The changeset.</value>
        IChangeset Changeset { get; set; }
    }
}