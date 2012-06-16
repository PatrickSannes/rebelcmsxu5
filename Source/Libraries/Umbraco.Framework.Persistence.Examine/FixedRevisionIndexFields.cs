namespace Umbraco.Framework.Persistence.Examine
{
    /// <summary>
    /// Fields for storing data about revisions
    /// </summary>
    public static class FixedRevisionIndexFields
    {
        /// <summary>
        /// Used to store the revision status name
        /// </summary>
        public const string RevisionStatusName = "RevStatusName";

        /// <summary>
        /// Used to store the revision status alias
        /// </summary>
        public const string RevisionStatusAlias = "RevStatusAlias";

        /// <summary>
        /// Used to store the revision id
        /// </summary>
        public const string RevisionId = "RevId";

        /// <summary>
        /// Used to store the revision status id
        /// </summary>
        public const string RevisionStatusId = "RevStatusId";

        /// <summary>
        /// Used to a flag of whether or not the revision status is a system type
        /// </summary>
        public const string RevisionStatusIsSystem = "RevStatusSys";

        /// <summary>
        /// Used to flag a revision as being the latest
        /// </summary>
        /// <remarks>>
        /// 
        /// This doesn't guarantee that there will be only 1 document flagged as RevLatest but it does gaurantee that
        /// there will be far fewer docs tagged with a true (1) value for querying after returning from Lucene.
        /// 
        /// Any revision with a latest status is flagged as IsLatest. For example if an Entity has 3 revisions with different revision
        /// statuses, then there will be 3 documents for that entity flagged as IsLatest.
        /// </remarks>
        public const string IsLatest = "RevLatest";

    }
}