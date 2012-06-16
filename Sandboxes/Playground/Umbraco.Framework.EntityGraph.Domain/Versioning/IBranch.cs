using System.Diagnostics.Contracts;

namespace Umbraco.Framework.EntityGraph.Domain.Versioning
{
    /// <summary>
    ///   Represents a group of changesets
    /// </summary>
    [ContractClass(typeof (IBranchCodeContract))]
    public interface IBranch
    {
        /// <summary>
        ///   Gets or sets the branch id.
        /// </summary>
        /// <value>The branch id.</value>
        IMappedIdentifier BranchId { get; set; }

        /// <summary>
        ///   Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }
    }
}