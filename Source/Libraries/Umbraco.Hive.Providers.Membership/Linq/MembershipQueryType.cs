namespace Umbraco.Hive.Providers.Membership.Linq
{
    public enum MembershipQueryType
    {
        /// <summary>
        /// Uses the in-built membership provider search
        /// </summary>
        ByUsername,

        /// <summary>
        /// Uses the in-built membership provider search
        /// </summary>
        ById,

        /// <summary>
        /// Uses the in-built membership provider search
        /// </summary>
        ByEmail,

        /// <summary>
        /// A custom search which requires to return the full collection of users in memory to search against
        /// </summary>
        Custom,

        /// <summary>
        /// Used to return nothing
        /// </summary>
        None
    }
}