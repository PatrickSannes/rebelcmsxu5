namespace Umbraco.Cms.Web.Routing
{
    /// <summary>
    /// The result status when trying to find an entity based on a Url
    /// </summary>
    public enum EntityRouteStatus
    {
        /// <summary>
        /// The entity was found in the first ordinal root branch that doesn't have a host name assigned
        /// </summary>
        SuccessWithoutHostname = 0,
        
        /// <summary>
        /// The entity was found with a hostname identified in the entity's branch
        /// </summary>
        SuccessByHostname = 1,

        /// <summary>
        /// The entity was found based on an Id formatted Url
        /// </summary>
        SuccessById = 2,

        /// <summary>
        /// Could not route to entity with the given revision status (i.e. If Published was requested but the item is not published)
        /// </summary>
        FailedRevisionStatusNotFound = 10,

        /// <summary>
        /// Could not route to entity because the entity exists in root branch with an sort ordinal greater than 0 (not the first) that doesn't have a hostname assigned
        /// </summary>
        /// <remarks>
        /// This only occurs if there is more than one root branch and one doesn't have a hostname assigned in it.
        /// </remarks>
        FailedRequiresHostname = 11,

        /// <summary>
        /// Could not find an entity with the specified path, one of the url names in the path didn't resolve to an entity at its particular level.
        /// </summary>
        FailedNotFoundByName = 12
    }
}