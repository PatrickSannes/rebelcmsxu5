

namespace Umbraco.Framework.Persistence.Abstractions.Versioning
{
    /// <summary>
    ///   Represents a group of changesets
    /// </summary>
    public interface IBranch
    {
        /// <summary>
        ///   Gets or sets the branch id.
        /// </summary>
        /// <value>The branch id.</value>
        HiveEntityUri BranchId { get; set; }

        /// <summary>
        ///   Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }
    }
}