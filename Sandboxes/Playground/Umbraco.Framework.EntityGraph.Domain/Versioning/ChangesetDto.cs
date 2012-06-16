namespace Umbraco.Framework.EntityGraph.Domain.Versioning
{
    public class ChangesetDto : IChangeset
    {
        #region Implementation of IChangeset

        /// <summary>
        /// Gets or sets the id of the changeset.
        /// </summary>
        /// <value>The id.</value>
        public IMappedIdentifier Id { get; set; }

        /// <summary>
        /// Gets or sets the branch in which this changeset resides.
        /// </summary>
        /// <value>The branch.</value>
        public IBranch Branch { get; set; }

        #endregion
    }
}