using System.Diagnostics.Contracts;

namespace Umbraco.Framework.EntityGraph.Domain.Versioning
{
    /// <summary>
    ///   Represents a set of changes
    /// </summary>
    [ContractClass(typeof (IChangesetCodeContract))]
    public interface IChangeset
    {
        /// <summary>
        ///   Gets or sets the id of the changeset.
        /// </summary>
        /// <value>The id.</value>
        IMappedIdentifier Id { get; set; }

        /// <summary>
        ///   Gets or sets the branch in which this changeset resides.
        /// </summary>
        /// <value>The branch.</value>
        IBranch Branch { get; set; }
    }
}