namespace Umbraco.Framework.EntityGraph.Domain.Versioning
{
    public class RevisionDataDto : IRevisionData
    {
        #region Implementation of IRevisionData

        /// <summary>
        /// Gets or sets the revision id.
        /// </summary>
        /// <value>The revision id.</value>
        public IMappedIdentifier RevisionId { get; set; }

        /// <summary>
        /// Gets or sets the changeset.
        /// </summary>
        /// <value>The changeset.</value>
        public IChangeset Changeset { get; set; }

        #endregion
    }
}