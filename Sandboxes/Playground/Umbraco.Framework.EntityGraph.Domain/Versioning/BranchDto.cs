namespace Umbraco.Framework.EntityGraph.Domain.Versioning
{
    public class BranchDto : IBranch
    {
        #region Implementation of IBranch

        /// <summary>
        /// Gets or sets the branch id.
        /// </summary>
        /// <value>The branch id.</value>
        public IMappedIdentifier BranchId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        #endregion
    }
}